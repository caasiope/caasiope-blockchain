using System.Collections.Generic;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;
using Caasiope.Protocol.Validators.Transactions;

namespace Caasiope.Node.Managers
{
    public class TransactionRequiredValidationFactory : Protocol.Validators.TransactionRequiredValidationFactory
    {
        // todo use interface to discriminate current and post state
        private readonly MultiSignatureManager multis;
        private readonly HashLockManager hashlocks;
        private readonly TimeLockManager timeLocks;

        public TransactionRequiredValidationFactory(MultiSignatureManager multis, HashLockManager hashlocks, TimeLockManager timeLocks)
        {
            this.multis = multis;
            this.hashlocks = hashlocks;
            this.timeLocks = timeLocks;
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

        private bool TryGetMultiSigFromDeclarations(List<TxDeclaration> declarations, Address address, out MultiSignatureAddress multiSignatureAddress)
        {
            foreach (var txDeclaration in declarations)
            {
                if (txDeclaration.Type == DeclarationType.MultiSignature)
                {
                    var multisig = (MultiSignature) txDeclaration;
                    if (multisig.Address == address)
                    {
                        multiSignatureAddress = MultiSignatureAddress.FromMultiSignature(multisig);
                        return true;
                    }
                }
            }
            multiSignatureAddress = null;
            return false;
        }

        private bool TryGetHashLockFromDeclarations(List<TxDeclaration> declarations, Address address, out HashLock hashLock)
        {
            foreach (var txDeclaration in declarations)
            {
                if (txDeclaration.Type == DeclarationType.HashLock)
                {
                    var declaration = (HashLock) txDeclaration;
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

        public override bool TryGetRequiredValidations(Address address, List<TxDeclaration> declarations, out TransactionRequiredValidation required)
        {
            return TryGetRequiredValidationsRecursive(address, declarations, out required, 0);
        }

        // TODO limit even more
        private const int MAX_DEPTH = 5;
        private const int MAX_CHILDREN = 20;
        // returns false if over the limit
        private bool TryGetRequiredValidationsRecursive(Address address, List<TxDeclaration> declarations, out TransactionRequiredValidation required, int depth)
        {
            // we check max depth to limit computation
            if(depth >= MAX_DEPTH)
            {
                required = null;
                return false;
            }

            switch (address.Type)
            {
                case AddressType.ECDSA:
                    required = new AddressRequiredSignature(address);
                    break;
                case AddressType.MultiSignatureECDSA:
                    if (!multis.TryGetAddress(address, out var multi))
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
                        if (!TryGetRequiredValidationsRecursive(signer, declarations, out var required2, depth + 1))
                        {
                            required = null;
                            return false;
                        }
                        list.Add(required2);
                    }

                    required = new MultiAddressRequiredSignature(multi, list);
                    break;
                case AddressType.HashLock:
                    if (!hashlocks.TryGetAddress(address, out var hashlock))
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
                    if (!timeLocks.TryGetAddress(address, out var timelock))
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
    }

    public class SignatureManager
    {
        [Injected] public ILiveService LiveService;
        
        public TransactionRequiredValidationFactory TransactionRequiredValidationFactory { get; private set; }

        public void Initialize()
        {
            Injector.Inject(this);
            TransactionRequiredValidationFactory = new TransactionRequiredValidationFactory(LiveService.MultiSignatureManager, LiveService.HashLockManager, LiveService.TimeLockManager);
        }
    }
}
