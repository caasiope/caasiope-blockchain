using System.Diagnostics;
using Caasiope.Node.Services;
using Caasiope.Node.Types;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class SignedTransactionManager
    {
        [Injected] public ILiveService LiveService;

        public void Initialize()
        {
            Injector.Inject(this);
        }

        public void Execute(MutableLedgerState state, Transaction transaction)
        {
            // TODO declarations
            foreach (var declaration in transaction.Declarations)
                ApplyDeclaration(state, declaration);

            // update balances
            foreach (var input in transaction.Inputs)
                UpdateBalance(state, input.Address, input.Currency, -input.Amount);
            foreach (var input in transaction.Outputs)
                UpdateBalance(state, input.Address, input.Currency, input.Amount);

            if (transaction.Fees != null)
            {
                UpdateBalance(state, transaction.Fees.Address, transaction.Fees.Currency, -transaction.Fees.Amount);
            }
        }

        private void ApplyDeclaration(MutableLedgerState state, TxDeclaration declaration)
        {
            if (declaration.Type == DeclarationType.MultiSignature)
            {
                var multisig = (MultiSignature)declaration;
                state.TryAddAccount(multisig);
            }
            else if (declaration.Type == DeclarationType.HashLock)
            {
                var hashLock = (HashLock)declaration;
                state.TryAddAccount(hashLock);
            }
            else if(declaration.Type == DeclarationType.TimeLock)
            {
                var timeLock = (TimeLock)declaration;
                state.TryAddAccount(timeLock);
            }
        }

        private void UpdateBalance(MutableLedgerState state, Address address, Currency currency, Amount amount)
        {
            var account = state.GetOrCreateMutableAccount(address);
            var balance = account.GetBalance(currency);

            Debug.Assert(LiveService.IssuerManager.IsIssuer(currency, address) || balance + amount >= 0);
            state.SetBalance(account, currency, balance + amount);
        }
    }
}