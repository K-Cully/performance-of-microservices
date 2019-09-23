using System;

namespace RandomGeneratorService.Core
{
    /// <summary>
    /// Generates primes
    /// </summary>
    /// <remarks>Note currently thread safe but suffecient for random seed generation puroses</remarks>
    public class PrimeGenerator : ISeedGenerator
    {
        private static uint LastPrime = 2;


        private readonly TimeSpan m_allowedTime = TimeSpan.FromSeconds(1.0d);


        /// <summary>
        /// Returns the highest prime number that can be computed in a set interval
        /// </summary>
        /// <returns>The generated prime number</returns>
        public uint Generate()
        {
            uint last = LastPrime;
            DateTime start = DateTime.Now;
            uint currentValue = LastPrime;
            uint currentPrime = LastPrime;

            while (m_allowedTime < DateTime.Now - start)
            {
                currentValue++;
                if (IsPrime(currentValue))
                {
                    currentPrime = currentValue;
                }
            }

            if (currentPrime == last)
            {
                currentPrime = 0;
            }

            LastPrime = currentPrime;
            return currentValue;
        }


        /// <summary>
        /// Checks if a given value is prime
        /// </summary>
        /// <param name="value">The value to examine</param>
        /// <returns>True if the value is prime, false otherwise</returns>
        private bool IsPrime(uint value)
        {
            if (value <= 1) return false;
            if (value == 2) return true;
            if (value % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(value));

            for (int i = 3; i <= boundary; i += 2)
                if (value % i == 0)
                    return false;

            return true;
        }
    }
}
