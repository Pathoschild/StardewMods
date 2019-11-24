using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which selects a random value once per day.</summary>
    internal class RandomValueProvider : BaseValueProvider
    {
        /*********
        ** Private methods
        *********/
        /// <summary>The seed to use when selecting a random number.</summary>
        private int Seed = -1;

        /// <summary>The underlying random number generator.</summary>
        private Random Random;

        /// <summary>The cached results by token string instance.</summary>
        private readonly IDictionary<ITokenString, string> CachedResults = new Dictionary<ITokenString, string>(new ObjectReferenceComparer<ITokenString>());


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public RandomValueProvider()
            : base(ConditionType.Random, canHaveMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, canHaveMultipleValues: false);
            this.UpdateRandom();
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            bool changed = this.UpdateRandom();
            if (changed)
                this.CachedResults.Clear();

            return base.UpdateContext(context) || changed;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (!this.CachedResults.TryGetValue(input, out string result))
            {
                string[] options = input.SplitValuesNonUnique().ToArray();
                this.CachedResults[input] = result = options[this.Random.Next(options.Length)];
            }
            yield return result;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Update the random number generator if needed.</summary>
        /// <returns>Returns whether the RNG changed.</returns>
        private bool UpdateRandom()
        {
            int seed = this.GetSeed();

            if (!this.IsReady || seed != this.Seed)
            {
                this.Seed = seed;
                this.Random = new Random(seed);
                this.MarkReady(true);
                return true;
            }

            return false;
        }

        /// <summary>Get the base seed based on the in-game date.</summary>
        private int GetSeed()
        {
            // save loaded
            if (Context.IsWorldReady)
                return SDate.Now().DaysSinceStart + (int)Game1.uniqueIDForThisGame / 2;

            // save loading
            if (SaveGame.loaded != null)
            {
                SaveGame save = SaveGame.loaded;
                return new SDate(save.dayOfMonth, save.currentSeason, save.year).DaysSinceStart + (int)save.uniqueIDForThisGame / 2;
            }

            // no save selected
            return DateTime.UtcNow.Minute;
        }
    }
}
