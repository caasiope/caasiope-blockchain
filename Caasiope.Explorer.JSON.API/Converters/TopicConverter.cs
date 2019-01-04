using System;
using Caasiope.Explorer.JSON.API.Internals;
using Newtonsoft.Json.Linq;

namespace Caasiope.Explorer.JSON.API.Converters
{
    public class TopicConverter : JsonCreationConverter<Topic>
    {
        protected override Topic Create(Type objectType, JObject jsonObject)
        {
            var typeName = jsonObject["Type"].ToString();
            switch (typeName)
            {
                case "Address":
                    return new AddressTopic();
                case "Ledger":
                    return new LedgerTopic();
                case "OrderBook":
                    return new OrderBookTopic();
                case "Transaction":
                    return new TransactionTopic();
                case "Funds":
                    return new FundsTopic();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}