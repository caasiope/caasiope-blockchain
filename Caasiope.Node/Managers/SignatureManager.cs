using System.Collections.Generic;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;
using Caasiope.Protocol.Validators.Transactions;

namespace Caasiope.Node.Managers
{
    public class TransactionRequiredValidationFactory : Protocol.Validators.TransactionRequiredValidationFactory
    {
        public override bool TryGetRequiredValidations(ILedgerState state, Address address, List<TxDeclaration> declarations, out TransactionRequiredValidation required)
        {
            return TryGetRequiredValidationsRecursive(state, address, declarations, out required, 0);
        }

        // TODO limit even more
        private const int MAX_DEPTH = 5;
        private const int MAX_CHILDREN = 20;
        // returns false if over the limit
        private bool TryGetRequiredValidationsRecursive(ILedgerState state, Address address, List<TxDeclaration> declarations, out TransactionRequiredValidation required, int depth)
        {
            // we check max depth to limit computation
            if(depth >= MAX_DEPTH)
            {
                required = null;
                return false;
            }

            // try get account
            switch (address.Type)
            {
                case AddressType.ECDSA:
                    required = new AddressRequiredSignature(address);
                    break;
                case AddressType.MultiSignatureECDSA:
                    if (!state.TryGetDeclaration<MultiSignature>(address, out var multi))
                    {
                        if (!TryGetMultiSigFromDeclarations(declarations, address, out multi))
                        {
                            required = NotDeclaredRequiredValidation.Instance;
                            break;
                        }
                    }

                    var list = new List<TransactionRequiredValidation>();
                    foreach (var signer in multi.Signers)
                    {
                        if (!TryGetRequiredValidationsRecursive(state, signer, declarations, out var required2, depth + 1))
                        {
                            required = null;
                            return false;
                        }
                        list.Add(required2);
                    }

                    required = new MultiAddressRequiredSignature(multi, list);
                    break;
                case AddressType.HashLock:
                    if (!state.TryGetDeclaration<HashLock>(address, out var hashlock))
                    {
                        if (!TryGetHashLockFromDeclarations(declarations, address, out hashlock))
                        {
                            required = NotDeclaredRequiredValidation.Instance;
                            break;
                        }
                    }
                    required = new HashLockRequiredSignature(hashlock);
                    break;
                case AddressType.TimeLock:
                    if (!state.TryGetDeclaration<TimeLock>(address, out var timelock))
                    {
                        if (!TryGetTimeLockFromDeclarations(declarations, address, out timelock))
                        {
                            required = NotDeclaredRequiredValidation.Instance;
                            break;
                        }
                    }

                    required = new TimeLockRequiredSignature(timelock);
                    //TODO
                    break;
                default:
                    required = null;
                    return false;
            }
            return true;
        }
        private bool TryGetTimeLockFromDeclarations(List<TxDeclaration> declarations, Address address, out TimeLock timelock)
        {
            foreach (var txDeclaration in declarations)
            {
                if (txDeclaration.Type == DeclarationType.TimeLock)
                {
                    var declaration = (TimeLock)txDeclaration;
                    if (declaration.Address == address)
                    {
                        timelock = declaration;
                        return true;
                    }
                }
            }
            timelock = null;
            return false;
        }

        private bool TryGetMultiSigFromDeclarations(List<TxDeclaration> declarations, Address address, out MultiSignature multisig)
        {
            foreach (var txDeclaration in declarations)
            {
                if (txDeclaration.Type == DeclarationType.MultiSignature)
                {
                    multisig = (MultiSignature)txDeclaration;
                    if (multisig.Address == address)
                    {
                        return true;
                    }
                }
            }
            multisig = null;
            return false;
        }

        private bool TryGetHashLockFromDeclarations(List<TxDeclaration> declarations, Address address, out HashLock hashLock)
        {
            foreach (var txDeclaration in declarations)
            {
                if (txDeclaration.Type == DeclarationType.HashLock)
                {
                    var declaration = (HashLock)txDeclaration;
                    if (declaration.Address == address)
                    {
                        hashLock = declaration;
                        return true;
                    }
                }
            }
            hashLock = null;
            return false;
        }
    }

    public class SignatureManager
    {
        public readonly TransactionRequiredValidationFactory TransactionRequiredValidationFactory= new TransactionRequiredValidationFactory();
    }
}
