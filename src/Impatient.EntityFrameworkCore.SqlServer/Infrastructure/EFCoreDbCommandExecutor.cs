using Impatient.Query.Infrastructure;
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

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public class EFCoreDbCommandExecutor : IDbCommandExecutor
{
    private readonly IRelationalCommandDiagnosticsLogger logger;

    private IStateManager persistentStateManager;
    private IStateManager ephemeralStateManager;
    private BufferingDbDataReader unbufferedReader;

    public EFCoreDbCommandExecutor(
        ICurrentDbContext currentDbContext,
        IRelationalCommandDiagnosticsLogger logger)
    {
        CurrentDbContext = currentDbContext;
        this.logger = logger;
    }

    public ICurrentDbContext CurrentDbContext { get; }

    public IStateManager PersistentStateManager =>
        persistentStateManager ??= CurrentDbContext.GetDependencies().StateManager;

    public IStateManager EphemeralStateManager =>
        ephemeralStateManager ??= new StateManager(PersistentStateManager.Dependencies);

    public TResult ExecuteComplex<TResult>(Action<DbCommand> initializer, Func<DbDataReader, TResult> materializer)
    {
        if (unbufferedReader is not null)
        {
            unbufferedReader.Buffer();
            unbufferedReader = null;
        }

        var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

        connection.Open();

        using var command = CreateCommand(connection);

        initializer(command);

        var commandId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        logger.CommandReaderExecuting(
            connection,
            command,
            CurrentDbContext.Context,
            commandId,
            connection.ConnectionId,
            startTime,
            CommandSource.LinqQuery);

        try
        {
            using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);

            logger.CommandReaderExecuted(
                connection,
                command,
                CurrentDbContext.Context,
                commandId,
                connection.ConnectionId,
                reader,
                startTime,
                stopwatch.Elapsed,
                CommandSource.LinqQuery);

            reader.Read();

            return materializer(reader);
        }
        catch (Exception exception)
        {
            logger.CommandError(
                connection,
                command,
                CurrentDbContext.Context,
                DbCommandMethod.ExecuteReader,
                commandId,
                connection.ConnectionId,
                exception,
                startTime,
                stopwatch.Elapsed,
                CommandSource.LinqQuery);

            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public IEnumerable<TElement> ExecuteEnumerable<TElement>(Action<DbCommand> initializer, Func<DbDataReader, TElement> materializer)
    {
        if (unbufferedReader is not null)
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

        logger.CommandReaderExecuting(
            connection,
            command,
            CurrentDbContext.Context,
            commandId,
            connection.ConnectionId,
            startTime,
            CommandSource.LinqQuery);

        var reader = default(DbDataReader);

        var caughtException = false;

        try
        {
            reader = command.ExecuteReader();

            logger.CommandReaderExecuted(
                connection,
                command,
                CurrentDbContext.Context,
                commandId,
                connection.ConnectionId,
                reader,
                startTime,
                stopwatch.Elapsed,
                CommandSource.LinqQuery);
        }
        catch (Exception exception)
        {
            caughtException = true;

            logger.CommandError(
                connection,
                command,
                CurrentDbContext.Context,
                DbCommandMethod.ExecuteReader,
                commandId,
                connection.ConnectionId,
                exception,
                startTime,
                stopwatch.Elapsed,
                CommandSource.LinqQuery);

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

        try
        {
            // TODO: this
            /*
            if (!connection.IsMultipleActiveResultSetsEnabled)
            {
                var buffer = new BufferingDbDataReader(reader, ArrayPool<object>.Shared);

                unbufferedReader = buffer;

                reader = buffer;
            }
            */

            while (reader.Read())
            {
                var materialized = materializer(reader);

                yield return materialized;
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
        if (unbufferedReader is not null)
        {
            unbufferedReader.Buffer();
            unbufferedReader = null;
        }

        var connection = CurrentDbContext.Context.Database.GetService<IRelationalConnection>();

        connection.Open();

        using var command = CreateCommand(connection);

        initializer(command);

        var commandId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        logger.CommandScalarExecuting(
            connection,
            command,
            CurrentDbContext.Context,
            commandId,
            connection.ConnectionId,
            startTime,
            CommandSource.LinqQuery);

        try
        {
            var result = command.ExecuteScalar();

            logger.CommandScalarExecuted(
                connection,
                command,
                CurrentDbContext.Context,
                commandId,
                connection.ConnectionId,
                result,
                startTime,
                stopwatch.Elapsed,
                CommandSource.LinqQuery);

            return DBNull.Value.Equals(result) ? default : (TResult)result;
        }
        catch (Exception exception)
        {
            logger.CommandError(
                connection,
                command,
                CurrentDbContext.Context,
                DbCommandMethod.ExecuteScalar,
                commandId,
                connection.ConnectionId,
                exception,
                startTime,
                stopwatch.Elapsed,
                CommandSource.LinqQuery);

            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    private static DbCommand CreateCommand(IRelationalConnection connection)
    {
        var command = connection.DbConnection.CreateCommand();

        if (connection.CurrentTransaction is not null)
        {
            command.Transaction = connection.CurrentTransaction.GetDbTransaction();
        }

        if (connection.CommandTimeout.HasValue)
        {
            command.CommandTimeout = connection.CommandTimeout.Value;
        }

        return command;
    }
}
