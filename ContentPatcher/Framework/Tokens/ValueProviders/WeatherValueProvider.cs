using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the weather.</summary>
    internal class WeatherValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether the basic save info is loaded.</summary>
        private readonly Func<bool> IsBasicInfoLoaded;

        /// <summary>The values as of the last context update.</summary>
        private readonly IDictionary<LocationContext, Weather> Values = CommonHelper
            .GetEnumValues<LocationContext>()
            .ToDictionary(p => p, _ => Weather.Sun);

        /// <summary>The input arguments recognized by this token.</summary>
        private readonly InvariantHashSet ValidInputKeys = new(Enum.GetNames(typeof(LocationContext)).Concat(new[] { "Current" }));

        /// <summary>The context for the current location.</summary>
        private LocationContext CurrentLocation;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isBasicInfoLoaded">Get whether the basic save info is loaded.</param>
        public WeatherValueProvider(Func<bool> isBasicInfoLoaded)
            : base(ConditionType.Weather, mayReturnMultipleValuesForRoot: false)
        {
            this.IsBasicInfoLoaded = isBasicInfoLoaded;
            this.EnableInputArguments(required: false, mayReturnMultipleValues: true, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                bool changed = false;

                if (this.MarkReady(this.IsBasicInfoLoaded()))
                {
                    // current location
                    var newLocation = ((LocationContext?)Game1.currentLocation?.GetLocationContext()) ?? LocationContext.Valley;
                    changed |= newLocation != this.CurrentLocation;
                    this.CurrentLocation = newLocation;

                    // weather values
                    foreach (LocationContext location in CommonHelper.GetEnumValues<LocationContext>())
                    {
                        Weather newWeather = this.GetWeather(location);

                        changed |= newWeather != this.Values[location];
                        this.Values[location] = newWeather;
                    }
                }

                return changed;
            });
        }

        /// <inheritdoc />
        public override InvariantHashSet GetValidPositionalArgs()
        {
            return this.ValidInputKeys;
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = new InvariantHashSet(Enum.GetNames(typeof(Weather)));
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return new InvariantHashSet(
                this.GetContextsFor(input).Select(p => this.Values[p].ToString())
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the contexts which apply for the given input arguments.</summary>
        /// <param name="input">The input arguments.</param>
        /// <returns>Returns the resulting inputs.</returns>
        private IEnumerable<LocationContext> GetContextsFor(IInputArguments input)
        {
            if (!input.HasPositionalArgs)
            {
                yield return this.CurrentLocation;
                yield break;
            }

            foreach (string arg in input.PositionalArgs)
            {
                if (arg.EqualsIgnoreCase("current"))
                    yield return this.CurrentLocation;

                else if (Enum.TryParse(arg, ignoreCase: true, out LocationContext context))
                    yield return context;
            }
        }

        /// <summary>Get the weather value for a location context.</summary>
        /// <param name="context">The location context.</param>
        private Weather GetWeather(LocationContext context)
        {
            // special case: day events override weather in the valley
            if (context == LocationContext.Valley)
            {
                if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || (SaveGame.loaded?.weddingToday ?? Game1.weddingToday))
                    return Weather.Sun;
            }

            // else get from game
            return this.GetWeather(
                Game1.netWorldState.Value.GetWeatherForLocation((GameLocation.LocationContext)context)
            );
        }

        /// <summary>Get the weather value for a per-location weather model.</summary>
        /// <param name="model">The location weather model.</param>
        private Weather GetWeather(LocationWeather model)
        {
            if (model.isSnowing.Value)
                return Weather.Snow;
            if (model.isRaining.Value)
                return model.isLightning.Value ? Weather.Storm : Weather.Rain;
            if (model.isDebrisWeather.Value)
                return Weather.Wind;

            return Weather.Sun;
        }
    }
}
