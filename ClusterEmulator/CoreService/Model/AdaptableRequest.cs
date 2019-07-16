using Newtonsoft.Json;

namespace CoreService.Model
{
    public class AdaptableRequest
    {
       [JsonProperty("payload")]
       [JsonRequired]
       public string Payload { get; set; }
    }
}
