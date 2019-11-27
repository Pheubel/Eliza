using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElizaBot.Services
{
    public class RNGService : Random
    {
        public RNGService() : base(Guid.NewGuid().GetHashCode()) { }
        public RNGService(int seed) : base(seed) { }

        /// <summary> Generates a unique set of numbers between a given range.</summary>
        /// <param name="count"> The amount of numbers to generate.</param>
        /// <param name="minValue"> The inclusive lower limit of the generated numbers.</param>
        /// <param name="maxValue"> The exclusive upper limit of the generated numbers.</param>
        public Task<IEnumerable<int>> GenerateUniqueNumbersAsync(int count, int minValue = 0, int maxValue = int.MaxValue)
        {
            if (count < 0)
                throw new ArgumentException($"{nameof(count)} can not be negative", nameof(count));

            if (maxValue < minValue)
                throw new ArgumentException($"{nameof(maxValue)} must be higher than {nameof(minValue)}", nameof(maxValue));

            if (count < (uint)(maxValue - minValue))
                throw new ArgumentException($"{nameof(count)} can not exceed the possible range of generated numbers of {nameof(minValue)} and {nameof(maxValue)} ({(uint)(maxValue - minValue)})");

            HashSet<int> numbers = new HashSet<int>();

            while (numbers.Count < count)
                numbers.Add(Next(minValue, maxValue));

            return Task.FromResult<IEnumerable<int>>(numbers);
        }
    }
}