using System;
using System.Data;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using SmartSql.DbSession;
using SmartSql.Exceptions;

namespace SmartSql.CAP
{
    public static class DbTransactionExtensions
    {
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
            if (sqlMapper.SessionStore.LocalSession != null)
            {
                throw new SmartSqlException(
                    "SmartSqlMapper could not invoke BeginCapTransaction(). A LocalSession is already existed.");
            }

            return sqlMapper.SessionStore.Open().BeginCapTransaction(publisher, autoCommit);
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
            if (sqlMapper.SessionStore.LocalSession != null)
            {
                throw new SmartSqlException(
                    "SmartSqlMapper could not invoke BeginCapTransaction(). A LocalSession is already existed.");
            }

            return sqlMapper.SessionStore.Open().BeginCapTransaction(isolationLevel, publisher, autoCommit);
        }

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
            if (publisher.Transaction.Value == null)
            {
                var capTransaction = publisher.ServiceProvider.GetService<ICapTransaction>();

                capTransaction.DbTransaction = dbSession;
                capTransaction.AutoCommit = autoCommit;

                publisher.Transaction.Value = capTransaction;
            }

            return publisher.Transaction.Value;
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
            if (publisher.Transaction.Value == null)
            {
                var capTransaction = publisher.ServiceProvider.GetService<ICapTransaction>();

                capTransaction.DbTransaction = dbSession;
                capTransaction.AutoCommit = autoCommit;

                publisher.Transaction.Value = capTransaction;
            }

            return publisher.Transaction.Value;
        }

        public static async Task CapTransactionWrapAsync(this IDbSession dbSession, ICapPublisher publisher,
            Func<Task> handler, bool autoCommit = false)
        {
            var trans = dbSession.BeginCapTransaction(publisher, autoCommit);
            try
            {
                await handler();
                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public static async Task CapTransactionWrapAsync(this IDbSession dbSession, IsolationLevel isolationLevel,
            ICapPublisher publisher, Func<Task> handler, bool autoCommit = false)
        {
            var trans = dbSession.BeginCapTransaction(isolationLevel, publisher, autoCommit);
            try
            {
                await handler();
                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }
    }
}