using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using SmartSql.DbSession;

namespace SmartSql.CAP
{
    public class SmartSqlCapTransaction : CapTransactionBase
    {
        public SmartSqlCapTransaction(
            IDispatcher dispatcher) : base(dispatcher)
        {
        }

        public override void Commit()
        {
            Debug.Assert(DbTransaction != null);

            if (DbTransaction is ITransaction transaction)
            {
                transaction.CommitTransaction();
            }

            Flush();
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            Debug.Assert(DbTransaction != null);

            if (DbTransaction is ITransaction transaction)
            {
                transaction.CommitTransaction();
            }

            Flush();

            await Task.CompletedTask;
        }

        public override void Rollback()
        {
            Debug.Assert(DbTransaction != null);

            if (DbTransaction is ITransaction transaction)
            {
                transaction.RollbackTransaction();
            }
        }

        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            Debug.Assert(DbTransaction != null);

            if (DbTransaction is ITransaction transaction)
            {
                transaction.RollbackTransaction();
            }

            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            DbTransaction = null;
        }
    }
}