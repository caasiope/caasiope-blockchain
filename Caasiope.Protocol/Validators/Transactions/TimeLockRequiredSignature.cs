using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators.Transactions
{
    public class TimeLockRequiredSignature : TransactionRequiredValidation
    {
        private readonly TimeLock locker;

        public TimeLockRequiredSignature(TimeLock locker)
        {
            this.locker = locker;
        }

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, Transaction transaction, long timestamp)
        {
            return locker.Timestamp > 0 && timestamp >= locker.Timestamp;
        }
    }
}