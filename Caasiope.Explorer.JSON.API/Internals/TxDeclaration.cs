
using System.Diagnostics;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public abstract class TxDeclaration
    {
        public readonly byte Type;

        protected TxDeclaration(byte type)
        {
            Type = type;
        }
    }


    public abstract class Topic
    {
        public readonly string Type;

        protected Topic()
        {
            var name = GetType().Name;
            Debug.Assert(name.Substring(name.Length-5, 5) == "Topic");
            Type = name.Substring(0, name.Length - 5); // remove Topic
        }
    }

    public class AddressTopic : Topic
    {
        public string Address;
    }

    public class LedgerTopic : Topic
    {
    }

    public class FundsTopic : Topic
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