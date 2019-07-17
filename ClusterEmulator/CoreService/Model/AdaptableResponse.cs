using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoreService.Model
{
    public class AdaptableResponse
    {
        [JsonProperty("values")]
        public IEnumerable<string> Values { get; set; }
    }
}
