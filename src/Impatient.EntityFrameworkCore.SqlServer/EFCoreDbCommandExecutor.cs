using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

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

            using (var command = connection.DbConnection.CreateCommand())
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

            using (var command = connection.DbConnection.CreateCommand())
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
                
                while (reader.Read())
                {
                    yield return materializer(reader);
                }
            }
        }

        public TResult ExecuteScalar<TResult>(Action<DbCommand> initializer)
        {
            var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

            using (var command = connection.DbConnection.CreateCommand())
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

                    return (TResult)result;
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
    }
}
