using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the weather.</summary>
    internal class WeatherValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles reading info from the current save.</summary>
        private readonly TokenSaveReader SaveReader;

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
        /// <param name="saveReader">Handles reading info from the current save.</param>
        public WeatherValueProvider(TokenSaveReader saveReader)
            : base(ConditionType.Weather, mayReturnMultipleValuesForRoot: false)
        {
            this.SaveReader = saveReader;
            this.EnableInputArguments(required: false, mayReturnMultipleValues: true, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                bool changed = false;

                if (this.MarkReady(this.SaveReader.IsReady))
                {
                    // current location
                    var newLocation = this.SaveReader.GetCurrentLocationContext(this.SaveReader.GetCurrentPlayer()) ?? LocationContext.Valley;
                    changed |= newLocation != this.CurrentLocation;
                    this.CurrentLocation = newLocation;

                    // weather values
                    foreach (LocationContext location in CommonHelper.GetEnumValues<LocationContext>())
                    {
                        Weather newWeather = this.SaveReader.GetWeather(location);

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

            var values = this
                .GetContextsFor(input)
                .Select(context => this.Values.TryGetValue(context, out Weather weather)
                    ? weather.ToString()
                    : Weather.Sun.ToString() // the game treats an invalid context (e.g. MAX) as always sunny
                );

            return new InvariantHashSet(values);
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
    }
}
