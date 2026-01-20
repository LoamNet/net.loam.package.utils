/// <summary> 
/// Loam implementation of the Whichmann-Hill random number generator algorithm as defined
/// in https://en.wikipedia.org/wiki/Wichmann%E2%80%93Hill. It's important to note that this
/// algorithm has a smaller range of valid seeds that can be user specified.
/// 
/// SEED RANGE: 0 to 29996 (inclusive)
/// 
/// Anything outside that range will be wrapped into that range. For example, providing seed 
/// values of 1 and 29998 would both yield the same internal seed value, in this case 2. This
/// internal seed is taken by applying mod of 29,997 then add 1.
/// 
/// Precision assumptions based on language references here (relevant if you're porting this elsewhere)
/// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
/// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
/// </summary>
namespace Loam
{
    public class WHRandom
    {
        /// <summary>
        /// The seed internally being used by WHRandom. This value isn't necessarily the same
        /// as the seed used with WHRandom initially, but it will yeild the same pseudo-random series.
        /// This value will always be in the range of 1 to 29,997, inclusive, and is calculated by
        /// taking the provided seed, applying mod of 29,997, then adding 1.
        /// </summary>
        public System.Int32 Seed { get; private set; }

        // Internal variables
        private System.Int32 seed1;
        private System.Int32 seed2;
        private System.Int32 seed3;



        /// <summary>
        /// Constructs WHRandom based on the system time in Milliseconds.
        /// </summary>
        public WHRandom() : this((System.Int32)System.DateTimeOffset.Now.ToUnixTimeMilliseconds()) { }

        /// <summary>
        /// Constructs WHRandom using the specified seed. If the seed value is outside of the
        /// range of 0 to 29,996 inclusive, then it will be wrapped to fit in that range. This means
        /// Seed values of 1 and 29998 will yield the same internal seed value, in this case 2. 
        /// 
        /// The internal seed can be checked with the Seed property.
        /// </summary>
        /// <param name="seed">The number used to generate the pseudo-random series</param>
        public WHRandom(System.Int32 seed)
        {
            InitState(seed);
        }

        /// <summary>
        /// Sets the internal state of WHRandom to begin with the specified seed.
        /// </summary>
        /// <param name="seed">The number used to generate the pseudo-random series.</param>
        public void InitState(System.Int32 seed)
        {
            // We want the provided seed in the range of 1 to 29,997 inclusive,
            // that way all three internal seeds are from 1 to 30,000 (inclusive).
            seed = System.Math.Abs(seed); // Ensure we're starting with a positive.
            Seed = seed % (29997);        // Make sure we have a value from 0 to 29,996 inclusive
            Seed += 1;                    // Make sure we have a value from 1 to 29,997 inclusive. 

            ResetSequence();
        }

        /// <summary>
        /// Restarts the random number sequence as if the constructor had just been run.
        /// </summary>
        public void ResetSequence()
        {
            seed1 = Seed;
            seed2 = Seed + 1;
            seed3 = Seed + 2;
        }

        /// <summary>
        /// Provide a 50/50 coinflip
        /// </summary>
        /// <returns>A random true or false value</returns>
        public bool FlipCoin()
        {
            return Next() >= 0.5;
        }
        
        /// <summary>
        /// Generates the next random number in the series.
        /// </summary>
        /// <returns>A pseudo-random number between 0 and 1</returns>
        public double Next()
        {
            // Apply primes
            seed1 = (171 * seed1) % 30269;
            seed2 = (172 * seed2) % 30307;
            seed3 = (170 * seed3) % 30323;

            // Aggregate seeds
            System.Double sum = (seed1 / 30269.0d) + (seed2 / 30307.0d) + (seed3 / 30323.0d);

            // Remove all but the decimal for aggregate double, and return that.
            return sum - System.Math.Truncate(sum);
        }
    }
}
