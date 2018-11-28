using System;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class TransactionDeclarationEntity
    {
        public readonly TransactionHash TransactionHash;
        public readonly int Index;
        public readonly long DeclarationId;

        public TransactionDeclarationEntity(TransactionHash transactionHash, int index, long declarationId)
        {
            DeclarationId = declarationId;
            TransactionHash = transactionHash;
            Index = index;
        }
    }

    public class DeclarationEntity : IEquatable<DeclarationEntity>
    {
        public readonly long DeclarationId;
        public readonly DeclarationType DeclarationType;

        public DeclarationEntity(long declarationId, DeclarationType declarationType)
        {
            DeclarationId = declarationId;
            DeclarationType = declarationType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DeclarationEntity) obj);
        }

        public bool Equals(DeclarationEntity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DeclarationId == other.DeclarationId && DeclarationType == other.DeclarationType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DeclarationId.GetHashCode() * 397) ^ (int) DeclarationType;
            }
        }
    }
}