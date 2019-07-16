using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoreService.Model
{
    public class AdaptableResponse
    {
        // TODO: flesh out

        [JsonProperty("values")]
        public IEnumerable<string> Values { get; set; }
    }
}
