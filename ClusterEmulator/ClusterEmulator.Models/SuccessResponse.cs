using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.Models
{
    public class SuccessResponse
    {
        private const int ChunkSize = 64;


        /// <summary>
        /// The success message, if any
        /// </summary>
        [JsonProperty("result")]
        public IEnumerable<string> Result { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="SuccessResponse"/>.
        /// </summary>
        /// <param name="length">The length of the success response in characters.</param>
        public SuccessResponse(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException($"{nameof(length)} cannot be negative", nameof(length));
            }

            var list = new List<string>();
            while (length > 0)
            {
                int chars = length < ChunkSize ? length : ChunkSize;
                list.Add(new string('1', chars));
                length -= chars;
            }

            Result = list;
        }
    }
}
