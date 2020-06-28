using System;
using System.Data;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using SmartSql.DbSession;

// ReSharper disable once CheckNamespace
namespace SmartSql.AOP
{
    public static class DbTransactionExtensions
    {
        /// <summary>
        /// Start the CAP transaction
        /// </summary>
        /// <param name="dbTransaction">The <see cref="IDbTransaction" />.</param>
        /// <param name="publisher">The <see cref="ICapPublisher" />.</param>
        /// <param name="autoCommit">Whether the transaction is automatically committed when the message is published</param>
        /// <returns>The <see cref="ICapTransaction" /> object.</returns>
        public static IDbTransaction BeginCapTransaction(this IDbTransaction dbTransaction,
            ICapPublisher publisher, bool autoCommit = false)
        {
            if (publisher.Transaction.Value == null)
            {
                var capTransaction = publisher.ServiceProvider.GetService<ICapTransaction>();

                capTransaction.DbTransaction = dbTransaction;
                capTransaction.AutoCommit = autoCommit;

                publisher.Transaction.Value = capTransaction;
            }

            return (IDbTransaction)publisher.Transaction.Value.DbTransaction;
        }

        public static async Task TransactionWrapAsync(this IDbSession dbSession, ICapPublisher publisher,
            Func<Task> handler, bool autoCommit = false)
        {
            dbSession.BeginTransaction().BeginCapTransaction(publisher, autoCommit);
            try
            {
                await handler();
                dbSession.CommitTransaction();
            }
            catch (Exception)
            {
                dbSession.RollbackTransaction();
                throw;
            }
        }

        public static async Task TransactionWrapAsync(this IDbSession dbSession, IsolationLevel isolationLevel,
            ICapPublisher publisher, Func<Task> handler, bool autoCommit = false)
        {
            dbSession.BeginTransaction(isolationLevel).BeginCapTransaction(publisher, autoCommit);
            try
            {
                await handler();
                dbSession.CommitTransaction();
            }
            catch (Exception)
            {
                dbSession.RollbackTransaction();
                throw;
            }
        }
    }
}