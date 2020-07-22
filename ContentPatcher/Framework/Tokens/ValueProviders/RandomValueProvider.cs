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
            string seedString = input.GetRawArgumentValue("key") ?? input.TokenString.Path;
            int randomNumber = new Random(unchecked(this.BaseSeed + seedString.GetHashCode())).Next();

            // choose value
            yield return input.PositionalArgs[randomNumber % input.PositionalArgs.Length];
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
    }
}
