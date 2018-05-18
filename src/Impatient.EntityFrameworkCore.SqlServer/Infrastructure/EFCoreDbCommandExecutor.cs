using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EFCoreDbCommandExecutor : IDbCommandExecutor
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger;

        private readonly Dictionary<IEntityType, EntityLookups> entityLookups
            = new Dictionary<IEntityType, EntityLookups>();

        private IStateManager stateManager;
        private IInternalEntityEntryFactory entryFactory;
        private BufferingDbDataReader unbufferedReader;

        public EFCoreDbCommandExecutor(
            ICurrentDbContext currentDbContext,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            CurrentDbContext = currentDbContext;
            this.logger = logger;
        }

        public ICurrentDbContext CurrentDbContext { get; }

        public IStateManager StateManager => stateManager
            ?? (stateManager = CurrentDbContext.GetDependencies().StateManager);

        public IInternalEntityEntryFactory EntryFactory => entryFactory
            ?? (entryFactory = CurrentDbContext.Context.GetService<IInternalEntityEntryFactory>());

        public TResult ExecuteComplex<TResult>(Action<DbCommand> initializer, Func<DbDataReader, TResult> materializer)
        {
            if (unbufferedReader != null)
            {
                unbufferedReader.Buffer();
                unbufferedReader = null;
            }

            var result = default(object);
            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            connection.Open();

            using (var command = CreateCommand(connection))
            {
                initializer(command);

                var commandId = Guid.NewGuid();
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                logger.CommandExecuting(
                    command,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    false,
                    startTime);

                try
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        logger.CommandExecuted(
                            command,
                            DbCommandMethod.ExecuteReader,
                            commandId,
                            connection.ConnectionId,
                            result,
                            false,
                            startTime,
                            stopwatch.Elapsed);

                        reader.Read();

                        CurrentDbContext.GetDependencies().StateManager.BeginTrackingQuery();

                        return materializer(reader);
                    }
                }
                catch (Exception exception)
                {
                    logger.CommandError(
                        command,
                        DbCommandMethod.ExecuteReader,
                        commandId,
                        connection.ConnectionId,
                        exception,
                        false,
                        startTime,
                        stopwatch.Elapsed);

                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public IEnumerable<TElement> ExecuteEnumerable<TElement>(Action<DbCommand> initializer, Func<DbDataReader, TElement> materializer)
        {
            if (unbufferedReader != null)
            {
                unbufferedReader.Buffer();
                unbufferedReader = null;
            }

            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            connection.Open();

            var command = CreateCommand(connection);

            initializer(command);

            var commandId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            logger.CommandExecuting(
                command,
                DbCommandMethod.ExecuteReader,
                commandId,
                connection.ConnectionId,
                false,
                startTime);

            var reader = default(DbDataReader);

            var caughtException = false;

            try
            {
                reader = command.ExecuteReader();

                logger.CommandExecuted(
                    command,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    reader,
                    false,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                caughtException = true;

                logger.CommandError(
                    command,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    false,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
            finally
            {
                if (caughtException)
                {
                    reader?.Dispose();
                    command.Dispose();
                    connection.Close();
                }
            }

            CurrentDbContext.GetDependencies().StateManager.BeginTrackingQuery();

            try
            {
                if (!connection.IsMultipleActiveResultSetsEnabled)
                {
                    var buffer = new BufferingDbDataReader(reader, ArrayPool<object>.Shared);

                    unbufferedReader = buffer;

                    reader = buffer;
                }

                while (reader.Read())
                {
                    yield return materializer(reader);
                }
            }
            finally
            {
                if (unbufferedReader == reader)
                {
                    unbufferedReader = null;
                }

                reader?.Dispose();
                command.Dispose();
                connection.Close();
            }
        }

        public TResult ExecuteScalar<TResult>(Action<DbCommand> initializer)
        {
            if (unbufferedReader != null)
            {
                unbufferedReader.Buffer();
                unbufferedReader = null;
            }

            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            connection.Open();

            using (var command = CreateCommand(connection))
            {
                initializer(command);

                var commandId = Guid.NewGuid();
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                logger.CommandExecuting(
                    command,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    false,
                    startTime);

                try
                {
                    var result = command.ExecuteScalar();

                    logger.CommandExecuted(
                        command,
                        DbCommandMethod.ExecuteScalar,
                        commandId,
                        connection.ConnectionId,
                        result,
                        false,
                        startTime,
                        stopwatch.Elapsed);

                    return DBNull.Value.Equals(result) ? default : (TResult)result;
                }
                catch (Exception exception)
                {
                    logger.CommandError(
                        command,
                        DbCommandMethod.ExecuteScalar,
                        commandId,
                        connection.ConnectionId,
                        exception,
                        false,
                        startTime,
                        stopwatch.Elapsed);

                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private DbCommand CreateCommand(IRelationalConnection connection)
        {
            var command = connection.DbConnection.CreateCommand();

            if (connection.CurrentTransaction != null)
            {
                command.Transaction = connection.CurrentTransaction.GetDbTransaction();
            }

            if (connection.CommandTimeout.HasValue)
            {
                command.CommandTimeout = connection.CommandTimeout.Value;
            }

            return command;
        }

        public bool TryGetEntity(object entity, IEntityType entityType, out EntityMaterializationInfo info)
        {
            info = default;

            var rootType = entityType.RootType();

            if (entityLookups.TryGetValue(rootType, out var lookups))
            {
                return lookups.EntityMap.TryGetValue(entity, out info);
            }

            return false;
        }

        public bool TryGetEntity(IEntityType entityType, object[] keyValues, out EntityMaterializationInfo info)
        {
            info = default;

            var rootType = entityType.RootType();

            if (entityLookups.TryGetValue(rootType, out var lookups))
            {
                return lookups.KeyMap.TryGetValue(keyValues, out info);
            }

            return false;
        }

        public void CacheEntity(IEntityType entityType, object[] keyValues, in EntityMaterializationInfo info)
        {
            var rootType = entityType.RootType();

            if (!entityLookups.TryGetValue(rootType, out var lookups))
            {
                IEqualityComparer<object[]> instance;

                switch (keyValues.Length)
                {
                    case 1:
                    {
                        instance = KeyValuesComparer1.Instance;
                        break;
                    }

                    case 2:
                    {
                        instance = KeyValuesComparer2.Instance;
                        break;
                    }

                    default:
                    {
                        instance = KeyValuesComparer.Create(keyValues.Length);
                        break;
                    }
                }

                entityLookups[rootType] = lookups = new EntityLookups
                {
                    KeyMap = new Dictionary<object[], EntityMaterializationInfo>(instance),
                    EntityMap = new Dictionary<object, EntityMaterializationInfo>(ReferenceEqualityComparer.Instance)
                };
            }

            lookups.KeyMap[keyValues] = info;
            lookups.EntityMap[info.Entity] = info;
        }

        private struct EntityLookups
        {
            public Dictionary<object[], EntityMaterializationInfo> KeyMap;
            public Dictionary<object, EntityMaterializationInfo> EntityMap;
        }

        private class KeyValuesComparer1 : IEqualityComparer<object[]>
        {
            public static KeyValuesComparer1 Instance = new KeyValuesComparer1();

            public bool Equals(object[] x, object[] y)
            {
                return x[0].Equals(y[0]);
            }

            public int GetHashCode(object[] obj)
            {
                return obj[0].GetHashCode();
            }
        }

        private class KeyValuesComparer2 : IEqualityComparer<object[]>
        {
            public static KeyValuesComparer2 Instance = new KeyValuesComparer2();

            public bool Equals(object[] x, object[] y)
            {
                return x[0].Equals(y[0]) && x[1].Equals(y[1]);
            }

            public int GetHashCode(object[] values)
            {
                unchecked
                {
                    var hash = KeyValuesComparer.InitialHashCode;

                    hash = (hash * 16777619) ^ values[0].GetHashCode();
                    hash = (hash * 16777619) ^ values[1].GetHashCode();

                    return hash;
                }
            }
        }

        private class KeyValuesComparer : IEqualityComparer<object[]>
        {
            public const int InitialHashCode = -2128831035;

            private readonly Func<object[], object[], bool> compiled;

            private KeyValuesComparer(Func<object[], object[], bool> compiled)
            {
                this.compiled = compiled;
            }

            public static KeyValuesComparer Create(int length)
            {
                var p1 = Expression.Parameter(typeof(object[]));
                var p2 = Expression.Parameter(typeof(object[]));
                var comparisons = new BinaryExpression[length];

                for (var i = 0; i < length; ++i)
                {
                    comparisons[i]
                        = Expression.Equal(
                            Expression.ArrayIndex(p1, Expression.Constant(i)),
                            Expression.ArrayIndex(p2, Expression.Constant(i)));
                }

                return new KeyValuesComparer(
                    (Func<object[], object[], bool>)Expression.Lambda(
                        comparisons.Aggregate(Expression.AndAlso),
                        new[] { p1, p2 }).Compile());
            }

            public bool Equals(object[] x, object[] y)
            {
                return compiled(x, y);
            }

            public int GetHashCode(object[] values)
            {
                unchecked
                {
                    var hash = InitialHashCode;

                    for (var i = 0; i < values.Length; i++)
                    {
                        hash = (hash * 16777619) ^ values[0].GetHashCode();
                        hash = (hash * 16777619) ^ values[1].GetHashCode();
                    }

                    return hash;
                }
            }
        }
    }
}
