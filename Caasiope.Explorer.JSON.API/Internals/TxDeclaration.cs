namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TxDeclaration
    {
        public byte Type;
    }


    public class Topic
    {
    }

    public class AddressTopic : Topic
    {
        public string Address;
    }

    public class LedgerTopic : Topic
    {
    }

    public class OrderBookTopic : Topic
    {
        public string Symbol;
    }

    public class TransactionTopic : Topic
    {
        public string Hash;
    }
}