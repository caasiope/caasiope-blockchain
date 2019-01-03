using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Types
{
    public abstract class Topic
    {
    }

    public class AddressTopic : Topic
    {
        public readonly Address Address;

        public AddressTopic(Address address)
        {
            Address = address;
        }
    }

    public class LedgerTopic : Topic
    {
    }

    public class OrderBookTopic : Topic
    {
        public readonly string Symbol;

        public OrderBookTopic(string symbol)
        {
            Symbol = symbol;
        }
    }

    public class TransactionTopic : Topic
    {
        public readonly TransactionHash Hash;

        public TransactionTopic(TransactionHash hash)
        {
            Hash = hash;
        }
    }
}