using Newtonsoft.Json;
using System;

namespace ClusterEmulator.Service.Models
{
    public class ErrorResponse
    {
        /// <summary>
        /// The success message, if any
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="ErrorResponse"/>.
        /// </summary>
        /// <param name="message">The message to set as the result.</param>
        public ErrorResponse(string message)
        {
            Error = message ??
                throw new ArgumentNullException($"{nameof(message)} cannot be null");
        }
    }
}
