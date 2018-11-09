using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class Signature
    {
        [JsonProperty(PropertyName = "k")]
        public string PublicKey;
        [JsonProperty(PropertyName = "s")]
        public string SignatureByte;
    }
}