using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.NBitcoin;

namespace Caasiope.Protocol.Types
{
    public class Signed<TData, THash> where THash : Hash256
    {
        public readonly THash Hash;
        protected readonly TData Data; // the name is too ugly, I force the subclass to wrap it
        public readonly List<Signature> Signatures;
        
        public Signed(TData data, THash hash, List<Signature> signatures)
        {
            Data = data;
            Hash = hash;
            Signatures = signatures;
        }

        public virtual bool AddSignature(Signature signature)
        {
            if(Signatures.Any(s => s.PublicKey == signature.PublicKey))
                return false;
            Signatures.Add(signature);
            return true;
        }

        public override bool Equals(object obj)
        {
            throw  new NotImplementedException();
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
            return base.GetHashCode();
        }
    }
}