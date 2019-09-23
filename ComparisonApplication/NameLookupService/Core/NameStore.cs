using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NameLookupService.Core
{
    /// <summary>
    /// Stores names based on an integer index
    /// </summary>
    public class NameStore : INameStore
    {
        private readonly IDictionary<int, string> m_names = new Dictionary<int, string>();

        private const int Entries = 2000;

        private const float HitRate = 0.3f;


        /// <summary>
        /// Initializes a new <see cref="NameStore"/>
        /// </summary>
        public NameStore()
        {
            var alphabet = new Span<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
            Random random = new Random();
            int id = 0;

            for (int count = 0; count < Entries; count++)
            {
                while (random.NextDouble() > HitRate)
                {
                    id++;
                }

                int nameLength = random.Next(3, 9);
                int nameIndex = random.Next(0, alphabet.Length - nameLength);
                Span<char> slice = alphabet.Slice(nameIndex, nameLength);
                m_names.Add(id, new string(slice));
                id++;
            }
        }


        /// <summary>
        /// Retrieves a name from the store.
        /// </summary>
        /// <param name="id">Id of the name to retrieve.</param>
        /// <returns>The name if found, false otehrwise.</returns>
        public async Task<string> GetNameAsync(int id)
        {
            if (!m_names.TryGetValue(id, out string value))
            {
                // TODO: log
            }

            return await Task.FromResult(value).ConfigureAwait(false);
        }
    }
}
