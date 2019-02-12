using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class ValidatorManager
    {
        public int Quorum { get; private set; }
        public int Total => validators.Count;

        private HashSet<PublicKey> validators;

        public void Initialize(List<PublicKey> list, int quorum)
        {
            Debug.Assert(!list.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).Any(), "Cannot have duplicates in validator list"); // No duplicates
            Debug.Assert(quorum > 0, "Quorum must be more than 0");
            Debug.Assert(quorum <= list.Count, "Quorum must be less or equal to the number of validators");

            validators = new HashSet<PublicKey>(list);
            Quorum = quorum;
        }

        public HashSet<PublicKey> GetValidators()
        {
            return validators;
        }

        public bool IsExist(PublicKey key)
        {
            return validators.Contains(key);
        }
    }
}