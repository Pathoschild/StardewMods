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

        /// <summary>The random numbers assigned for each pinned key.</summary>
        private readonly InvariantDictionary<int> PinnedKeys = new InvariantDictionary<int>();


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
            this.PinnedKeys.Clear();
            this.CachedResults.Clear();
            bool changed = this.UpdateRandom();

            return base.UpdateContext(context) || changed;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (string.IsNullOrWhiteSpace(input.Value))
                yield break;

            // get for pinned key
            if (this.TryGetPinnedNumber(input, out string choices, out int randomNumber))
            {
                yield return this.Choose(choices, randomNumber);
                yield break;
            }

            // get for input string
            if (!this.CachedResults.TryGetValue(input, out string result))
            {
                string[] options = input.SplitValuesNonUnique().ToArray();
                this.CachedResults[input] = result = options[this.Random.Next(options.Length)];
            }
            yield return result;
        }

        /// <summary>Get the allowed values for an input argument (or <c>null</c> if any value is allowed).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override InvariantHashSet GetAllowedValues(ITokenString input)
        {
            if (input.IsMutable || !input.IsReady)
                return null;

            return this.TryGetPinnedNumber(input, out string choices, out _)
                ? new InvariantHashSet(choices.SplitValuesNonUnique())
                : new InvariantHashSet(input.SplitValuesNonUnique());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a value from the given input based on the selected random number.</summary>
        /// <param name="input">The comma-delimited input to parse.</param>
        /// <param name="randomNumber">The random number to apply.</param>
        private string Choose(string input, int randomNumber)
        {
            string[] options = input.SplitValuesNonUnique().ToArray();
            return options[randomNumber % options.Length];
        }

        /// <summary>Parse a token string to extract the underlying input and the number associated with the pinned key, if applicable.</summary>
        /// <param name="tokenStr">The token string to parse.</param>
        /// <param name="choices">The comma-delimited choices.</param>
        /// <param name="randomNumber">The random number associated with the pinned value.</param>
        private bool TryGetPinnedNumber(ITokenString tokenStr, out string choices, out int randomNumber)
        {
            choices = null;
            randomNumber = -1;
            if (!tokenStr.IsReady || string.IsNullOrWhiteSpace(tokenStr.Value))
                return false;

            // parse token
            string rawInput, pinnedKey;
            {
                string[] parts = tokenStr.Value.Split(new[] { '|' }, 2);
                if (parts.Length != 2)
                    return false;

                rawInput = parts[0].Trim();
                pinnedKey = parts[1].Trim();
            }
            if (string.IsNullOrWhiteSpace(pinnedKey))
                return false;

            // get value
            choices = rawInput;
            if (!this.PinnedKeys.TryGetValue(pinnedKey, out randomNumber))
                this.PinnedKeys[pinnedKey] = randomNumber = this.Random.Next();
            return true;
        }

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
