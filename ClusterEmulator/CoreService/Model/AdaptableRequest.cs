using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoreService.Model
{
    public class AdaptableRequest
    {
       [JsonProperty("payload")]
       [JsonRequired]
       public IEnumerable<string> Payload { get; set; }
    }
}
