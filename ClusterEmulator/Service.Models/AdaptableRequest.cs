using Newtonsoft.Json;
using System.Collections.Generic;

namespace Service.Models
{
    /// <summary>
    /// An adaptable request object containing a list of values
    /// </summary>
    public class AdaptableRequest
    {
        /// <summary>
        /// The request payload
        /// </summary>
        [JsonProperty("payload", Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        [JsonRequired]
        public IEnumerable<string> Payload { get; set; }
    }
}
