using Newtonsoft.Json;
using System;

namespace CoreService.Model
{
    public class SuccessResponse
    {
        /// <summary>
        /// The success message, if any
        /// </summary>
        [JsonProperty("result")]
        public string Result { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="SuccessResponse"/>.
        /// </summary>
        /// <param name="message">The message to set as the result.</param>
        public SuccessResponse(string message)
        {
            Result = message ??
                throw new ArgumentNullException($"{nameof(message)} cannot be null");
        }
    }
}
