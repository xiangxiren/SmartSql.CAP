﻿using System;
using System.Data;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using SmartSql.DbSession;
using SmartSql.DyRepository;

namespace SmartSql.CAP;

public static class DbTransactionExtensions
{
    #region IRepository

    public static ICapTransaction BeginCapTransaction(this IRepository repository, ICapPublisher publisher,
        bool autoCommit = false) =>
        repository.SqlMapper.BeginCapTransaction(publisher, autoCommit);

    public static ICapTransaction BeginCapTransaction(this IRepository repository, IsolationLevel isolationLevel,
        ICapPublisher publisher, bool autoCommit = false) =>
        repository.SqlMapper.BeginCapTransaction(isolationLevel, publisher, autoCommit);

    public static void CapTransactionWrap(this IRepository repository, ICapPublisher publisher,
        Action handler, bool autoCommit = false) =>
        repository.SqlMapper.CapTransactionWrap(publisher, handler, autoCommit);

    public static void CapTransactionWrap(this IRepository repository, IsolationLevel isolationLevel,
        ICapPublisher publisher, Action handler, bool autoCommit = false) =>
        repository.SqlMapper.CapTransactionWrap(isolationLevel, publisher, handler, autoCommit);

    public static async Task CapTransactionWrapAsync(this IRepository repository, ICapPublisher publisher,
        Func<Task> handler, bool autoCommit = false) =>
        await repository.SqlMapper.CapTransactionWrapAsync(publisher, handler, autoCommit);

    public static async Task CapTransactionWrapAsync(this IRepository repository, IsolationLevel isolationLevel,
        ICapPublisher publisher, Func<Task> handler, bool autoCommit = false) =>
        await repository.SqlMapper.CapTransactionWrapAsync(isolationLevel, publisher, handler, autoCommit);

    #endregion

    #region ISqlMapper

    /// <summary>
    /// Start the CAP transaction
    /// </summary>
    /// <param name="sqlMapper">The <see cref="ISqlMapper" />.</param>
    /// <param name="publisher">The <see cref="ICapPublisher" />.</param>
    /// <param name="autoCommit">Whether the transaction is automatically committed when the message is published</param>
    /// <returns>The <see cref="ICapTransaction" /> object.</returns>
    public static ICapTransaction BeginCapTransaction(this ISqlMapper sqlMapper, ICapPublisher publisher,
        bool autoCommit = false)
    {
        sqlMapper.BeginTransaction();
        if (publisher.Transaction == null)
        {
            var capTransaction = ActivatorUtilities.CreateInstance<SmartSqlCapTransaction>(publisher.ServiceProvider);

            capTransaction.DbTransaction = sqlMapper;
            capTransaction.AutoCommit = autoCommit;

            publisher.Transaction = capTransaction;
        }

        return publisher.Transaction;
    }

    /// <summary>
    /// Start the CAP transaction
    /// </summary>
    /// <param name="sqlMapper">The <see cref="ISqlMapper" />.</param>
    /// <param name="isolationLevel">The <see cref="IsolationLevel" />.</param>
    /// <param name="publisher">The <see cref="ICapPublisher" />.</param>
    /// <param name="autoCommit">Whether the transaction is automatically committed when the message is published</param>
    /// <returns>The <see cref="ICapTransaction" /> object.</returns>
    public static ICapTransaction BeginCapTransaction(this ISqlMapper sqlMapper, IsolationLevel isolationLevel,
        ICapPublisher publisher, bool autoCommit = false)
    {
        sqlMapper.BeginTransaction(isolationLevel);
        if (publisher.Transaction == null)
        {
            var capTransaction = ActivatorUtilities.CreateInstance<SmartSqlCapTransaction>(publisher.ServiceProvider);

            capTransaction.DbTransaction = sqlMapper;
            capTransaction.AutoCommit = autoCommit;

            publisher.Transaction = capTransaction;
        }

        return publisher.Transaction;
    }

    public static void CapTransactionWrap(this ISqlMapper sqlMapper, ICapPublisher publisher,
        Action handler, bool autoCommit = false)
    {
        using var trans = sqlMapper.BeginCapTransaction(publisher, autoCommit);
        handler();
        trans.Commit();
    }

    public static void CapTransactionWrap(this ISqlMapper sqlMapper, IsolationLevel isolationLevel,
        ICapPublisher publisher, Action handler, bool autoCommit = false)
    {
        using var trans = sqlMapper.BeginCapTransaction(isolationLevel, publisher, autoCommit);
        handler();
        trans.Commit();
    }

    public static async Task CapTransactionWrapAsync(this ISqlMapper sqlMapper, ICapPublisher publisher,
        Func<Task> handler, bool autoCommit = false)
    {
        using var trans = sqlMapper.BeginCapTransaction(publisher, autoCommit);
        await handler();
        await trans.CommitAsync();
    }

    public static async Task CapTransactionWrapAsync(this ISqlMapper sqlMapper, IsolationLevel isolationLevel,
        ICapPublisher publisher, Func<Task> handler, bool autoCommit = false)
    {
        using var trans = sqlMapper.BeginCapTransaction(isolationLevel, publisher, autoCommit);
        await handler();
        await trans.CommitAsync();
    }

    #endregion

    #region IDbSession

    /// <summary>
    /// Start the CAP transaction
    /// </summary>
    /// <param name="dbSession">The <see cref="IDbSession" />.</param>
    /// <param name="publisher">The <see cref="ICapPublisher" />.</param>
    /// <param name="autoCommit">Whether the transaction is automatically committed when the message is published</param>
    /// <returns>The <see cref="ICapTransaction" /> object.</returns>
    public static ICapTransaction BeginCapTransaction(this IDbSession dbSession, ICapPublisher publisher,
        bool autoCommit = false)
    {
        dbSession.BeginTransaction();
        if (publisher.Transaction == null)
        {
            var capTransaction = ActivatorUtilities.CreateInstance<SmartSqlCapTransaction>(publisher.ServiceProvider);

            capTransaction.DbTransaction = dbSession;
            capTransaction.AutoCommit = autoCommit;

            publisher.Transaction = capTransaction;
        }

        return publisher.Transaction;
    }

    /// <summary>
    /// Start the CAP transaction
    /// </summary>
    /// <param name="dbSession">The <see cref="IDbSession" />.</param>
    /// <param name="isolationLevel">The <see cref="IsolationLevel" />.</param>
    /// <param name="publisher">The <see cref="ICapPublisher" />.</param>
    /// <param name="autoCommit">Whether the transaction is automatically committed when the message is published</param>
    /// <returns>The <see cref="ICapTransaction" /> object.</returns>
    public static ICapTransaction BeginCapTransaction(this IDbSession dbSession, IsolationLevel isolationLevel,
        ICapPublisher publisher, bool autoCommit = false)
    {
        dbSession.BeginTransaction(isolationLevel);
        if (publisher.Transaction == null)
        {
            var capTransaction = ActivatorUtilities.CreateInstance<SmartSqlCapTransaction>(publisher.ServiceProvider);

            capTransaction.DbTransaction = dbSession;
            capTransaction.AutoCommit = autoCommit;

            publisher.Transaction = capTransaction;
        }

        return publisher.Transaction;
    }

    public static void CapTransactionWrap(this IDbSession dbSession, ICapPublisher publisher,
        Action handler, bool autoCommit = false)
    {
        using var trans = dbSession.BeginCapTransaction(publisher, autoCommit);
        handler();
        trans.Commit();
    }

    public static void CapTransactionWrap(this IDbSession dbSession, IsolationLevel isolationLevel,
        ICapPublisher publisher, Action handler, bool autoCommit = false)
    {
        using var trans = dbSession.BeginCapTransaction(isolationLevel, publisher, autoCommit);
        handler();
        trans.Commit();
    }

    public static async Task CapTransactionWrapAsync(this IDbSession dbSession, ICapPublisher publisher,
        Func<Task> handler, bool autoCommit = false)
    {
        using var trans = dbSession.BeginCapTransaction(publisher, autoCommit);
        await handler();
        await trans.CommitAsync();
    }

    public static async Task CapTransactionWrapAsync(this IDbSession dbSession, IsolationLevel isolationLevel,
        ICapPublisher publisher, Func<Task> handler, bool autoCommit = false)
    {
        using var trans = dbSession.BeginCapTransaction(isolationLevel, publisher, autoCommit);
        await handler();
        await trans.CommitAsync();
    }

    #endregion
}