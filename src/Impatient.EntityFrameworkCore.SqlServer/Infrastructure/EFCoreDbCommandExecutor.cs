using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    // TODO: needs to be created via factory
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

                var reader = default(DbDataReader);

                try
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);

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

                CurrentDbContext.GetDependencies().StateManager.BeginTrackingQuery();

                while (reader.Read())
                {
                    yield return materializer(reader);
                }
            }
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

        public EntityMaterializationInfo GetMaterializationInfo(object entity, Type type)
        {
            if (entityLookups.TryGetValue(type, out var lookups))
            {
                if (lookups.EntityMap.TryGetValue(entity, out var info))
                {
                    return info;
                }
            }

            return default;
        }

        public bool TryGetEntity(Type type, object[] keyValues, ref object entity, List<INavigation> includes, out bool includesCached)
        {
            includesCached = false;

            if (entityLookups.TryGetValue(type, out var lookups))
            {
                if (lookups.KeyMap.TryGetValue(keyValues, out var cached))
                {
                    entity = cached.Entity;

                    if (cached.Includes.Contains(includes))
                    {
                        includesCached = true;
                    }
                    else
                    {
                        cached.Includes.Add(includes);
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
            if (!entityLookups.TryGetValue(entityType.ClrType, out var lookups))
            {
                lookups = new EntityLookups
                {
                    KeyMap = new Dictionary<object[], EntityMaterializationInfo>(KeyValuesComparer.Instance),
                    EntityMap = new ConditionalWeakTable<object, EntityMaterializationInfo>(),
                };

                entityLookups[entityType.ClrType] = lookups;                       
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
            };

            lookups.KeyMap[keyValues] = info;

            lookups.EntityMap.AddOrUpdate(entity, info);
        }

        private struct EntityLookups
        {
            public Dictionary<object[], EntityMaterializationInfo> KeyMap;
            public ConditionalWeakTable<object, EntityMaterializationInfo> EntityMap;
        }

        private readonly Dictionary<Type, EntityLookups> entityLookups
            = new Dictionary<Type, EntityLookups>();

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
    }
}
