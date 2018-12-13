using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TransactionHeader
    {
        [JsonProperty(PropertyName = "i")]
        public int Index;
        [JsonProperty(PropertyName = "h")]
        public string Hash;
        [JsonProperty(PropertyName = "f", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Fee;
        [JsonProperty(PropertyName = "d")]
        public bool HasDeclaration;

        public TransactionHeader(int index, string hash, decimal? fee, bool hasDeclaration)
        {
            Index = index;
            Hash = hash;
            Fee = fee;
            HasDeclaration = hasDeclaration;
        }
    }
}