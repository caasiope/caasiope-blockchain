using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Transformers
{
    internal class DeclarationTransformerService : DataTransformerService<TxDeclarationEntity, TransactionDeclarationRepository>
    {
        protected override IEnumerable<TxDeclarationEntity> Transform(DataTransformationContext context)
        {
            var list = new List<TxDeclarationEntity>();

            var timelocks = context.SignedLedgerState.State.TimeLocks;
            foreach (var locker in timelocks)
            {
                list.Add(new TxDeclarationEntity(locker.Address, GetRaw(locker)));
            }

            var hashlocks = context.SignedLedgerState.State.HashLocks;
            foreach (var locker in hashlocks)
            {
                list.Add(new TxDeclarationEntity(locker.Address, GetRaw(locker)));
            }

            var multiSignatures = context.SignedLedgerState.State.MultiSignatures;
            foreach (var locker in multiSignatures)
            {
                list.Add(new TxDeclarationEntity(locker.Address, GetRaw(locker)));
            }

            return list;
        }

        private byte[] GetRaw(TxDeclaration multiSignature)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(multiSignature);
                return stream.GetBytes();
            }
        }
    }
}