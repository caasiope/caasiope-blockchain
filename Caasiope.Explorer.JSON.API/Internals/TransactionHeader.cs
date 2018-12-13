using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TransactionHeader
    {
        [JsonProperty(PropertyName = "h")]
        public string Hash;
        [JsonProperty(PropertyName = "f", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Fee;
        [JsonProperty(PropertyName = "d")]
        public bool HasDeclaration;

        public TransactionHeader(string hash, decimal? fee, bool hasDeclaration)
        {
            Hash = hash;
            Fee = fee;
            HasDeclaration = hasDeclaration;
        }
    }
}