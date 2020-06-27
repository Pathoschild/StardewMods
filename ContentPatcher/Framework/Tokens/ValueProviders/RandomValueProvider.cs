using System;
using System.Collections.Generic;
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

        /// <summary>The random numbers assigned to pinned keys.</summary>
        private readonly InvariantDictionary<int> PinnedKeys = new InvariantDictionary<int>();

        /// <summary>The random numbers generated for input arguments that didn't specify one.</summary>
        private readonly IDictionary<ITokenString, int> GeneratedKeys = new Dictionary<ITokenString, int>(new ObjectReferenceComparer<ITokenString>());


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public RandomValueProvider()
            : base(ConditionType.Random, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
            this.ValidNamedArguments.Add("key");
            this.UpdateRandom();
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            this.PinnedKeys.Clear();
            this.GeneratedKeys.Clear();
            bool changed = this.UpdateRandom();

            return base.UpdateContext(context) || changed;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            // validate
            this.AssertInput(input);
            if (!input.HasPositionalArgs)
                yield break;

            // get pinned value
            int pinnedNumber = this.GetPinnedNumber(input);
            yield return this.Choose(input.PositionalArgs, pinnedNumber);
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = !input.IsMutable
                ? new InvariantHashSet(input.PositionalArgs)
                : null;

            return allowedValues != null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a value from the given input based on the selected random number.</summary>
        /// <param name="choices">The choices to pick from.</param>
        /// <param name="randomNumber">The random number to apply.</param>
        private string Choose(string[] choices, int randomNumber)
        {
            return choices[randomNumber % choices.Length];
        }

        /// <summary>Get the pinned random number from the input arguments.</summary>
        /// <param name="input">The input arguments.</param>
        private int GetPinnedNumber(IInputArguments input)
        {
            // from cache
            if (this.GeneratedKeys.TryGetValue(input.TokenString, out int cachedKey))
                return cachedKey;

            // from pinned key
            string pinnedKey = this.GetPinnedKey(input);
            if (pinnedKey != null)
            {
                if (!this.PinnedKeys.TryGetValue(pinnedKey, out int pinnedNumber))
                    this.PinnedKeys[pinnedKey] = pinnedNumber = this.Random.Next();
                return pinnedNumber;
            }

            // generate new number
            int generated = this.Random.Next();
            this.GeneratedKeys[input.TokenString] = generated;
            return generated;
        }

        /// <summary>Get the pinned key from the given input arguments, if any.</summary>
        /// <param name="input">The input arguments.</param>
        private string GetPinnedKey(IInputArguments input)
        {
            return input.NamedArgs.TryGetValue("key", out IInputArgumentValue value)
                ? value.Raw
                : null;
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
