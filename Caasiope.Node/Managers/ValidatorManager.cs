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

        private Dictionary<PublicKey, Validator> validators;

        public void Initialize(List<PublicKey> list, int quorum)
        {
            Debug.Assert(quorum > 0);
            Debug.Assert(quorum <= list.Count); // Quorum cannot be bigger than current number of validators, right?

            validators = list.ToDictionary(p => p, v => new Validator(v));
            Quorum = quorum;
        }

        public IEnumerable<Validator> GetValidators()
        {
            return validators.Values;
        }

        public bool IsExist(PublicKey key)
        {
            return validators.ContainsKey(key);
        }
    }
}