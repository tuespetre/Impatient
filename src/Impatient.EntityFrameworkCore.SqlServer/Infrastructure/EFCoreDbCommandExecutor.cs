using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EFCoreDbCommandExecutor : IDbCommandExecutor
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger;

        public EFCoreDbCommandExecutor(
            ICurrentDbContext currentDbContext,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            CurrentDbContext = currentDbContext;
            this.logger = logger;
        }

        public ICurrentDbContext CurrentDbContext { get; }

        public TResult ExecuteComplex<TResult>(Action<DbCommand> initializer, Func<DbDataReader, TResult> materializer)
        {
            var result = default(object);
            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            using (var command = CreateCommand(connection))
            {
                initializer(command);

                connection.Open();

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
            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            var command = CreateCommand(connection);

            initializer(command);

            connection.Open();

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
                    reader?.Close();
                    command.Dispose();
                    connection.Close();
                }
            }

            CurrentDbContext.GetDependencies().StateManager.BeginTrackingQuery();

            return new ReaderEnumerable<TElement>(
                command,
                connection,
                reader,
                materializer);
        }

        public TResult ExecuteScalar<TResult>(Action<DbCommand> initializer)
        {
            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            using (var command = CreateCommand(connection))
            {
                initializer(command);

                connection.Open();

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

        public EntityMaterializationInfo GetMaterializationInfo(object entity, IEntityType entityType)
        {
            if (entityLookups.TryGetValue(entityType, out var lookups))
            {
                if (lookups.EntityMap.TryGetValue(entity, out var info))
                {
                    return info;
                }
            }

            return default;
        }

        public bool TryGetEntity(IEntityType entityType, object[] keyValues, ref object entity, List<INavigation> includes, out bool cachedIncludes)
        {
            cachedIncludes = false;

            if (entityLookups.TryGetValue(entityType, out var lookups))
            {
                if (lookups.KeyMap.TryGetValue(keyValues, out var cached))
                {
                    entity = cached.Entity;

                    if (cached.Includes.Contains(includes))
                    {
                        cachedIncludes = true;
                    }
                    else
                    {
                        cached.Includes.Add(includes);

                        foreach (var include in includes)
                        {
                            cached.ForeignKeys.Add(include.ForeignKey);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public void CacheEntity(
            IEntityType entityType,
            IKey key,
            object[] keyValues,
            object entity,
            IProperty[] shadowProperties,
            object[] shadowPropertyValues,
            List<INavigation> includes)
        {
            if (!entityLookups.TryGetValue(entityType, out var lookups))
            {
                lookups = new EntityLookups
                {
                    KeyMap = new Dictionary<object[], EntityMaterializationInfo>(KeyValuesComparer.Instance),
                    EntityMap = new ConditionalWeakTable<object, EntityMaterializationInfo>(),
                };

                entityLookups[entityType] = lookups;
            }

            var info = new EntityMaterializationInfo
            {
                Entity = entity,
                KeyValues = keyValues,
                ShadowPropertyValues = shadowPropertyValues,
                EntityType = entityType,
                Key = key,
                ShadowProperties = shadowProperties,
                Includes = new List<List<INavigation>> { includes },
                ForeignKeys = new HashSet<IForeignKey>(includes.Select(i => i.ForeignKey))
            };

            lookups.KeyMap[keyValues] = info;

            lookups.EntityMap.AddOrUpdate(entity, info);
        }

        private struct EntityLookups
        {
            public Dictionary<object[], EntityMaterializationInfo> KeyMap;
            public ConditionalWeakTable<object, EntityMaterializationInfo> EntityMap;
        }

        private readonly Dictionary<IEntityType, EntityLookups> entityLookups
            = new Dictionary<IEntityType, EntityLookups>();

        private class KeyValuesComparer : IEqualityComparer<object[]>
        {
            private const int InitialHashCode = unchecked((int)2166136261);

            public static KeyValuesComparer Instance { get; } = new KeyValuesComparer();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int Combine(int a, int b) => unchecked((a * 16777619) ^ b);

            public bool Equals(object[] x, object[] y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(object[] obj)
            {
                var hash = InitialHashCode;

                for (var i = 0; i < obj.Length; i++)
                {
                    hash = Combine(hash, obj[i]?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }

        private class ReaderEnumerable<T> : IEnumerable<T>
        {
            private readonly DbCommand command;
            private readonly IRelationalConnection connection;
            private readonly DbDataReader reader;
            private readonly Func<DbDataReader, T> materializer;

            public ReaderEnumerable(
                DbCommand command,
                IRelationalConnection connection,
                DbDataReader reader,
                Func<DbDataReader, T> materializer)
            {
                this.command = command;
                this.connection = connection;
                this.reader = reader;
                this.materializer = materializer;
            }

            public IEnumerator<T> GetEnumerator()
                => new ReaderEnumerator<T>(
                    command,
                    connection,
                    reader,
                    materializer);

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        private class ReaderEnumerator<T> : IEnumerator<T>
        {
            private readonly DbCommand command;
            private readonly IRelationalConnection connection;
            private readonly DbDataReader reader;
            private readonly Func<DbDataReader, T> materializer;

            private bool finished;

            public ReaderEnumerator(
                DbCommand command,
                IRelationalConnection connection,
                DbDataReader reader,
                Func<DbDataReader, T> materializer)
            {
                this.command = command;
                this.connection = connection;
                this.reader = reader;
                this.materializer = materializer;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                reader.Close();
                command.Dispose();
                connection.Close();
            }

            public bool MoveNext()
            {
                if (finished)
                {
                    return false;
                }
                else if (reader.Read())
                {
                    Current = materializer(reader);

                    return true;
                }
                else
                {
                    finished = true;
                    Current = default;

                    return false;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}
