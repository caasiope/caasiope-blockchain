using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TransactionHeader
    {
        public string Hash;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Fee;
        public bool HasDeclaration;

        public TransactionHeader(string hash, decimal? fee, bool hasDeclaration)
        {
            Hash = hash;
            Fee = fee;
            HasDeclaration = hasDeclaration;
        }
    }
}