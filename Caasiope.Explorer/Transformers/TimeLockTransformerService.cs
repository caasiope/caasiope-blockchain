using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Transformers
{
    internal class TimeLockTransformerService : DataTransformerService<TimeLockEntity, TimeLockRepository>
    {
        protected override IEnumerable<TimeLockEntity> Transform(DataTransformationContext context)
        {
            var timelocks = context.SignedLedgerState.State.TimeLocks;
            var list = new List<TimeLockEntity>();

            var declarations = context.GetDeclarations();
            foreach (var locker in timelocks)
            {
                list.Add(new TimeLockEntity(GetDeclarationId(declarations.AddressDeclarations, locker.Address), new TimeLockAccount(locker.Address, locker.Timestamp)));
            }
            return list;
        }

        private long GetDeclarationId(Dictionary<Address, TransactionDeclarationEntity> declarations, Address address)
        {
            return declarations[address].DeclarationId;
        }
    }
}