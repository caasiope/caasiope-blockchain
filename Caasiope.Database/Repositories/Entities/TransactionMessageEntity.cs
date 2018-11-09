using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class TransactionMessageEntity
    {
        public readonly TransactionHash Hash;
        public readonly TransactionMessage Message;

        public TransactionMessageEntity(TransactionHash hash, TransactionMessage message)
        {
            Hash = hash;
            Message = message;
        }
    }
}