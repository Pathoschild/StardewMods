using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <summary>The base seed for the current save info.</summary>
        private int BaseSeed;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public RandomValueProvider()
            : base(ConditionType.Random, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
            this.ValidNamedArguments.Add("key");
            this.BaseSeed = this.GenerateBaseSeed();
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            int oldSeed = this.BaseSeed;
            this.BaseSeed = this.GenerateBaseSeed();
            return base.UpdateContext(context) || this.BaseSeed != oldSeed;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            // validate
            this.AssertInput(input);
            if (!input.HasPositionalArgs)
                yield break;

            // get random number for input
            string seedString = input.GetRawArgumentValue("key") ?? input.TokenString!.Path;
            int randomNumber = new Random(unchecked(this.BaseSeed + this.GetDeterministicHashCode(seedString))).Next();

            // choose value
            yield return input.PositionalArgs[randomNumber % input.PositionalArgs.Length];
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out InvariantHashSet? allowedValues)
        {
            allowedValues = !input.IsMutable
                ? new InvariantHashSet(input.PositionalArgs)
                : null;

            return allowedValues != null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the base seed for the current save info.</summary>
        private int GenerateBaseSeed()
        {
            // The seed is based on the current save info, but correctly generates a random one if
            // no save is loaded since Game1.uniqueIDForThisGame is randomized when returning to
            // title.

            int daysSinceStart = SDate.Now().DaysSinceStart;
            int uniqueId = (int)Game1.uniqueIDForThisGame;

            if (!Context.IsWorldReady && SaveGame.loaded != null)
            {
                SaveGame save = SaveGame.loaded;
                daysSinceStart = new SDate(save.dayOfMonth, save.currentSeason, save.year).DaysSinceStart;
                uniqueId = (int)save.uniqueIDForThisGame;
            }

            return unchecked(daysSinceStart + uniqueId);
        }

        /// <summary>Get a deterministic hash code for a given string.</summary>
        /// <param name="str">The string to hash.</param>
        /// <remarks>This ensures that the same hash code is generated across multiple players in multiplayer, so randomization is in sync. Derived from <a href="https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/#a-deterministic-gethashcode-implementation">code by Andrew Lock</a>.</remarks>
        private int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

    }
}
