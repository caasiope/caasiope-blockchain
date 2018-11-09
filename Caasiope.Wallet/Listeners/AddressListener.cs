using System;
using System.Collections.Generic;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Wallet.Listeners
{
    public class AddressListener
    {
        private readonly Dictionary<string, Address> addresses = new Dictionary<string, Address>();

        private Action<TxInputOutput> WalletUpdated;

        public void RegisterWalletUpdated(Action<TxInputOutput> callback)
        {
            WalletUpdated += callback;
        }

        internal void OnLedgerUpdated(SignedLedger signed)
        {
            Console.WriteLine($"Ledger Updated ! Height {signed.Ledger.LedgerLight.Height}");
            foreach (var transaction in signed.Ledger.Block.Transactions)
            {
                foreach (var input in transaction.Transaction.Inputs)
                {
                    if (addresses.ContainsKey(input.Address.Encoded))
                        WalletUpdated.Call(input);
                }

                foreach (var output in transaction.Transaction.Outputs)
                {
                    if (addresses.ContainsKey(output.Address.Encoded))
                        WalletUpdated.Call(output);
                }
            }
        }

        public void Listen(Address address)
        {
            addresses.Add(address.Encoded, address);
        }
    }
}
