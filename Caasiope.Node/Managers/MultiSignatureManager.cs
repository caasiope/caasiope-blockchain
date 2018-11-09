using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    // TODO get a post state version of it with shared interface
    public class MultiSignatureManager
    {
        private readonly Dictionary<string, MultiSignature> multis = new Dictionary<string, MultiSignature>();

        public void Initialize(List<MultiSignatureAddress> list)
        {
            Injector.Inject(this);

            foreach (var multi in list)
            {
                if(!TryAddAccount(new MultiSignature(multi.Signers, multi.Required)))
                    multis.Add(multi.Address.Encoded, new MultiSignature(multi.Signers, multi.Required));
            }
        }

        /// <summary>
        /// returns true if account is new
        /// </summary>
        /// <param name="multi"></param>
        /// <returns>true if account is new</returns>
        public bool TryAddAccount(MultiSignature multi)
        {
            var encoded = multi.Address.Encoded;

            if (!multis.ContainsKey(encoded))
            {
                multis.Add(encoded, multi);
                return true;
            }
            return false;
        }

        public bool TryGetAddress(Address address, out MultiSignatureAddress multi)
        {
            if (multis.TryGetValue(address.Encoded, out var declaration))
            {
                multi = MultiSignatureAddress.FromMultiSignature(declaration);
                return true;
            }

            multi = null;
            return false;
        }

        public IEnumerable<MultiSignature> GetMultiSignatures()
        {
            return multis.Values;
        }
    }
}