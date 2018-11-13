using System;
using System.Collections.Generic;
using Caasiope.Node;
using Caasiope.Protocol.Types;
using Caasiope.Wallet.Services;
using Caasiope.NBitcoin;

namespace Caasiope.Wallet.Managers
{
    public class MutableTransaction
    {
        public long Expire;
        public TxInput Fees;
        public List<TxDeclaration> Declarations = new List<TxDeclaration>();
        public List<TxInput> Inputs = new List<TxInput>();
        public List<TxOutput> Outputs = new List<TxOutput>();
        public TransactionMessage Message = TransactionMessage.Empty;

        public Transaction CreateTransaction()
        {
            return new Transaction(new List<TxDeclaration>(Declarations), new List<TxInput>(Inputs), new List<TxOutput>(Outputs), Message, Expire, Fees);
        }
    }

    public class PrepareTransactionManager
    {
        [Injected] public IWalletService WalletService;

        public MutableTransaction Transaction { get; private set; }

        public void InitializeTransaction()
        {
            Transaction = new MutableTransaction();
        }

        public bool FinalizeTransaction()
        {
            if (Transaction == null)
                return false;

            Transaction.Expire = Transaction.Expire == 0 ? DateTime.UtcNow.AddMinutes(1).ToUnixTimestamp() : Transaction.Expire;

            if (Transaction.Fees == null)
            {
                var active = WalletService.GetActiveKey();
                if (active != null)
                {
                    Transaction.Fees = WalletService.CreateFeesInput(WalletService.GetActiveKey().Data.Address);
                }
            }

            if (WalletService.SignAndSubmit(Transaction.CreateTransaction()))
            {
                Transaction = null;
                return true;
            }

            return false;
        }

        public void AddInputOutput(TxInputOutput io)
        {
            if(io.IsInput)
                Transaction.Inputs.Add((TxInput)io);
            else
                Transaction.Outputs.Add((TxOutput)io);
        }

        public void AddDeclaration(TxDeclaration declaration)
        {
            Transaction.Declarations.Add(declaration);
        }

        public void SetFees(Address address, Currency currency, Amount amount)
        {
            Transaction.Fees = new TxInput(address, currency, amount);
        }
    }
}
