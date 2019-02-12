using System.Collections.Generic;
using Caasiope.Node.Services;
using Caasiope.Node.Types;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;

namespace Caasiope.Node
{
    public class PostStateHolder
    {
        [Injected] public ILiveService LiveService;
        [Injected] public ILedgerService LedgerService;

        private readonly List<TransactionHash> postStateTransactions = new List<TransactionHash>();
        
        private readonly LedgerPostState PostState;

        public PostStateHolder(LedgerStateFinal previous, long height)
        {
            PostState = new LedgerPostState(previous, height);
            Injector.Inject(this);
        }

        // try to apply a transaction to the post state
        public bool ProcessTransaction(SignedTransaction transaction)
        {
            // see if it was not already applied
            if (AlreadyIncluded(transaction.Hash))
                return false;

            // validate balance
            if (!LiveService.TransactionManager.TransactionValidator.ValidateBalance(PostState, transaction.Transaction.GetInputs()))
                return false;

            // include transacton in the post state
            IncludeTransaction(transaction);
            return true;
        }

        private void IncludeTransaction(SignedTransaction transaction)
        {
            LedgerService.SignedTransactionManager.Execute(PostState, transaction.Transaction);
            postStateTransactions.Add(transaction.Hash);
        }

        private bool AlreadyIncluded(TransactionHash hash)
        {
            return postStateTransactions.Contains(hash);
        }

        public IEnumerable<TransactionHash> GetTransactions() => postStateTransactions;

        public LedgerStateFinal Finalize(ProtocolVersion version)
        {
            return PostState.Finalize(HasherFactory.CreateHasher(version));
        }

        public int NbTransactions => postStateTransactions.Count;
    }
}