using System;
using Caasiope.Database.Managers;
using Caasiope.Database.Repositories;
using Caasiope.Database.SQL;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.SqlTransactions
{
    public class SignedTransactionSqlTransaction : SqlTransaction<SignedTransaction>
    {
        public SignedTransactionSqlTransaction(SignedTransaction data) : base(data)
        {
        }
        protected override void Populate(RepositoryManager repositories, BlockchainEntities entities)
        {
            var hash = Data.Hash;

            var transactionRepository = repositories.GetRepository<TransactionRepository>();

            throw new NotImplementedException();
        }
    }
}
