using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators.Transactions
{
    public class HashLockRequiredSignature : TransactionRequiredValidation
    {
        private readonly HashLock locker;

        public HashLockRequiredSignature(HashLock locker)
        {
            this.locker = locker;
        }

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, Transaction transaction, long timestamp)
        {
            // TODO super false, we want to be able to spend when unlocked
            foreach (var declaration in transaction.Declarations)
            {
                if (declaration.Type == DeclarationType.Secret)
                {
                    var secret = (SecretRevelation)declaration;
                    var hash = secret.Secret.ComputeSecretHash(locker.SecretHash.Type);
                    if (hash.Hash.Equals(locker.SecretHash.Hash))
                        return true;
                }
            }
            return false;
        }
    }
}