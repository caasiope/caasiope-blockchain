using System;
using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators.Transactions
{
    public class VendingMachineRequiredSignature : TransactionRequiredValidation
    {
        public readonly VendingMachine Machine;

        public VendingMachineRequiredSignature(VendingMachine machine)
        {
            Machine = machine;
        }

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, Transaction transaction, long timestamp)
        {
            // check if it is a valid exchange
            if (IsExchange(transaction))
                return true;

            // require the signature from the owner
            foreach (var signature in signatures)
            {
                if (signature.CheckAddress(Machine.Owner))
                {
                    signature.Require();
                    return true;
                }
            }
            return false;
        }

        private bool IsExchange(Transaction transaction)
        {
            TxInput send = null;
            // what the machine sends
            foreach (var input in transaction.Inputs)
            {
                if (input.Address.Encoded == Machine.Address.Encoded)
                {
                    if (input.Currency == Machine.CurrencyOut)
                    {
                        send = input;
                    }
                    else
                    {
                        return false; // the machine only outputs the specified currency
                    }
                }
            }

            if (send == null)
                return false;

            TxOutput receive = null;
            // what the machine receives
            foreach (var output in transaction.Outputs)
            {
                if (output.Address.Encoded == Machine.Address.Encoded)
                {
                    if (output.Currency == Machine.CurrencyIn)
                    {
                        receive = output;
                    }
                }
            }

            if (receive == null)
                return false;

            // verify it matches
            try
            {
                var maximum = Amount.Multiply(send.Amount, Machine.Rate);
                return receive.Amount >= maximum;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}