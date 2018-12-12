using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin;

namespace Caasiope.Explorer.JSON.API
{
    public class LedgerConverter
    {
        public static Internals.Ledger GetLedger(SignedLedger ledger)
        {
            if (ledger == null)
                return null;

            var light = ledger.Ledger.LedgerLight;
            var transactions = ledger.Ledger.Block.Transactions.Select(_ => _.Hash.ToBase64()).ToList();
            return new Internals.Ledger(light.Height, ledger.Hash.ToBase64(), light.Timestamp, light.Lastledger.ToBase64(), light.Version.VersionNumber, transactions);
        }
    }

    public class TransactionConverter
    {
        public static SignedTransaction GetSignedTransaction(Internals.Transaction transaction, List<Internals.Signature> signatures)
        {
            if (transaction == null) return null;

            var inputs = transaction.Inputs.Select(CreateInput).ToList();
            var outputs = transaction.Outputs.Select(CreateOutput).ToList();
            var declarations = transaction.Declarations.Select(CreateDeclaration).ToList();
            var fees = transaction.Fees == null ? null : CreateInput(transaction.Fees);
            var message = transaction.Message == null ? null : new TransactionMessage(Convert.FromBase64String(transaction.Message));

            var signed = new SignedTransaction(new Transaction(declarations, inputs, outputs, message, transaction.Expire, fees), CreateSignatures(signatures).ToList());

            Debug.Assert(signed.Hash.Equals(new TransactionHash(Convert.FromBase64String(transaction.Hash))));

            return signed;
        }

        private static IEnumerable<Signature> CreateSignatures(List<Internals.Signature> signatures)
        {
            foreach (var signature in signatures)
            {
                var publicKey = new PublicKey(Convert.FromBase64String(signature.PublicKey));
                var signatureByte = new SignatureByte(Convert.FromBase64String(signature.SignatureByte));
                yield return new Signature(publicKey, signatureByte);
            }
        }

        private static TxInput CreateInput(Internals.TxInput input)
        {
            return new TxInput(new Address(input.Address), Currency.FromSymbol(input.Currency), Amount.FromWholeDecimal(input.Amount));
        }

        private static TxOutput CreateOutput(Internals.TxOutput output)
        {
            return new TxOutput(new Address(output.Address), Currency.FromSymbol(output.Currency), Amount.FromWholeDecimal(output.Amount));
        }

        private static TxDeclaration CreateDeclaration(Internals.TxDeclaration declaration)
        {
            switch ((DeclarationType)declaration.Type)
            {
                case DeclarationType.MultiSignature:
                    return CreateMultisignature((Internals.MultiSignature)declaration);
                case DeclarationType.HashLock:
                    return CreateHashLock((Internals.HashLock)declaration);
                case DeclarationType.Secret:
                    return CreateSecret((Internals.SecretRevelation)declaration);
                    case DeclarationType.TimeLock:
                    return CreateTimeLock((Internals.TimeLock)declaration);
                    case DeclarationType.VendingMachine:
                    return CreateVendingMachine((Internals.VendingMachine)declaration);
                default:
                    throw new NotImplementedException();
            }
        }

        private static TxDeclaration CreateMultisignature(Internals.MultiSignature declaration)
        {
            return new MultiSignature(declaration.Signers.Select(_ => new Address(_)), declaration.Required);
        }

        private static TxDeclaration CreateHashLock(Internals.HashLock declaration)
        {
            var secretHash = new SecretHash(declaration.SecretHash.Type, new Hash256(Convert.FromBase64String(declaration.SecretHash.Hash)));
            return new HashLock(secretHash);
        }

        private static TxDeclaration CreateSecret(Internals.SecretRevelation declaration)
        {
            var secret = new Secret(Convert.FromBase64String(declaration.Secret));
            return new SecretRevelation(secret);
        }

        private static TxDeclaration CreateTimeLock(Internals.TimeLock declaration)
        {
            return new TimeLock(declaration.Timestamp);
        }

        private static TxDeclaration CreateVendingMachine(Internals.VendingMachine declaration)
        {
            return new VendingMachine(new Address(declaration.Owner), Currency.FromSymbol(declaration.CurrencyIn), Currency.FromSymbol(declaration.CurrencyOut), Amount.FromWholeDecimal(declaration.Rate));
        }

        public static Internals.Transaction GetTransaction(Transaction transaction)
        {
            if (transaction == null) return null;

            var inputs = transaction.Inputs.Select(CreateInput).ToList();
            var outputs = transaction.Outputs.Select(CreateOutput).ToList();
            var declarations = transaction.Declarations.Select(CreateDeclaration).ToList();

            return new Internals.Transaction
            {
                Hash = transaction.GetHash().ToBase64(),
                Expire = transaction.Expire,
                Message = transaction.Message == null || transaction.Message.Equals(TransactionMessage.Empty) ? null : Convert.ToBase64String(transaction.Message.GetBytes()),
                Declarations = declarations,
                Inputs = inputs,
                Outputs = outputs,
                Fees = transaction.Fees == null ? null : CreateInput(transaction.Fees)
            };
        }
        
        public static Internals.HistoricalTransaction GetHistoricalTransaction(HistoricalTransaction historical)
        {
            var transaction = historical?.Transaction;
            if (transaction == null) return null;

            var inputs = transaction.Inputs.Select(CreateInput).ToList();
            var outputs = transaction.Outputs.Select(CreateOutput).ToList();
            var declarations = transaction.Declarations.Select(CreateDeclaration).ToList();

            return new Internals.HistoricalTransaction
            {
                Height = historical.LedgerHeight,
                LedgerTimestamp = historical.LedgerTimestamp,
                Transaction = new Internals.Transaction
                {
                    Hash = transaction.GetHash().ToBase64(),
                    Expire = transaction.Expire,
                    Message = transaction.Message == null || transaction.Message.Equals(TransactionMessage.Empty) ? null : Convert.ToBase64String(transaction.Message.GetBytes()),
                    Declarations = declarations,
                    Inputs = inputs,
                    Outputs = outputs,
                    Fees = transaction.Fees == null ? null : CreateInput(transaction.Fees)
                }
            };
        }

        private static Internals.TxInput CreateInput(TxInput input)
        {
            return new Internals.TxInput { Address = input.Address.Encoded, Currency = Currency.ToSymbol(input.Currency), Amount = Amount.ToWholeDecimal(input.Amount) };
        }

        private static Internals.TxOutput CreateOutput(TxOutput output)
        {
            return new Internals.TxOutput { Address = output.Address.Encoded, Currency = Currency.ToSymbol(output.Currency), Amount = Amount.ToWholeDecimal(output.Amount) };
        }

        private static Internals.TxDeclaration CreateDeclaration(TxDeclaration declaration)
        {
            switch (declaration.Type)
            {
                case DeclarationType.MultiSignature:
                    return CreateMultisignature((MultiSignature)declaration);
                case DeclarationType.HashLock:
                    return CreateHashLock((HashLock)declaration);
                case DeclarationType.Secret:
                    return CreateSecret((SecretRevelation)declaration);
                case DeclarationType.TimeLock:
                    return CreateTimeLock((TimeLock)declaration);
                case DeclarationType.VendingMachine:
                    return CreateVendingMachine((VendingMachine)declaration);
                default:
                    throw new NotImplementedException();
            }
        }

        private static Internals.TxDeclaration CreateMultisignature(MultiSignature declaration)
        {
            return new Internals.MultiSignature(declaration.Signers.Select(_ => _.Encoded).ToList(), declaration.Required);
        }

        private static Internals.TxDeclaration CreateHashLock(HashLock declaration)
        {
            var secretHash = new Internals.SecretHash(declaration.SecretHash.Type, Convert.ToBase64String(declaration.SecretHash.Hash.Bytes));
            return new Internals.HashLock(secretHash);
        }

        private static Internals.TxDeclaration CreateSecret(SecretRevelation declaration)
        {
            return new Internals.SecretRevelation(Convert.ToBase64String(declaration.Secret.Bytes));
        }

        private static Internals.TxDeclaration CreateTimeLock(TimeLock declaration)
        {
            return new Internals.TimeLock(declaration.Timestamp);
        }

        private static Internals.TxDeclaration CreateVendingMachine(VendingMachine declaration)
        {
            return new Internals.VendingMachine(declaration.Owner.Encoded, Currency.ToSymbol(declaration.CurrencyIn), Currency.ToSymbol(declaration.CurrencyOut), declaration.Rate);
        }

        public static IEnumerable<Internals.Signature> GetSignatures(List<Signature> signatures)
        {
            foreach (var signature in signatures)
            {
                yield return new Internals.Signature {PublicKey = Convert.ToBase64String(signature.PublicKey.GetBytes()), SignatureByte = Convert.ToBase64String(signature.SignatureByte.Bytes)};
            }
        }
    }
}