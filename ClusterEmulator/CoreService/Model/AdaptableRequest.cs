using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoreService.Model
{
    /// <summary>
    /// An adaptable request object containing a list of values
    /// </summary>
    [Serializable]
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
