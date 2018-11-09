using System;
using System.Collections.Generic;
using Helios.Common.Extensions;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Types;

namespace Caasiope.UnitTest
{
    public static class Extensions
    {
        public static Transaction CreateExchangeTransaction(this TestBase test, out PrivateKeyNotWallet account1, out PrivateKeyNotWallet account2)
        {
            account1 = test.CreateAccount();
            account2 = test.CreateAccount();

            var declarations = new List<TxDeclaration>();

            var inputs = new List<TxInput>()
            {
                new TxInput(account1, Currency.BTC, Amount.FromWholeValue(10)),
                new TxInput(account2, Currency.LTC, Amount.FromWholeValue(30)),
            };

            var outputs = new List<TxOutput>()
            {
                new TxOutput(account2, Currency.BTC, Amount.FromWholeValue(10)),
                new TxOutput(account1, Currency.LTC, Amount.FromWholeValue(30)),
            };

            return new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.Ticks);
        }

        public static bool CompareSigned<T, THash>(this Signed<T, THash> signed, Signed<T, THash> readed) where THash : Hash256
        {
            return SignaturesEqual(signed.Signatures, readed.Signatures) && signed.Hash.Equals(readed.Hash);
        }

        private static bool SignaturesEqual(IReadOnlyCollection<Signature> a, IReadOnlyCollection<Signature> b)
        {
            if (a.Count != b.Count)
                return false;

            if (a.Count == 0)
                return true;

            var isValid = false;
            foreach (var signatureA in a)
            {
                foreach (var signatureB in b)
                {
                    if (signatureA.PublicKey.CompareTo(signatureB.PublicKey) == 0 && signatureA.SignatureByte.Bytes.IsEqual(signatureB.SignatureByte.Bytes))
                    {
                        isValid = true;
                        break;
                    }
                    isValid = false;
                }
            }
            return isValid;
        }
    }
}