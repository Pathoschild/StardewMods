using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

/// <summary>A value provider for the weather.</summary>
internal class WeatherValueProvider : BaseValueProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles reading info from the current save.</summary>
    private readonly TokenSaveReader SaveReader;

    /// <summary>The values as of the last context update.</summary>
    private readonly Dictionary<string, string?> Values = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The context name for the current location.</summary>
    private string? CurrentLocationContextName;


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
                string newLocation = this.SaveReader.GetCurrentLocationContext(this.SaveReader.GetCurrentPlayer());
                changed |= newLocation != this.CurrentLocationContextName;
                this.CurrentLocationContextName = newLocation;

                // update weather values
                HashSet<string> currentContexts = this.SaveReader.GetContexts();
                foreach (string locationContext in currentContexts)
                {
                    string? newWeather = this.SaveReader.GetWeather(locationContext);

                    changed |= !this.Values.TryGetValue(locationContext, out string? oldWeather) || oldWeather != newWeather;

                    this.Values[locationContext] = newWeather;
                }

                // remove disappeared contexts
                foreach (string locationContext in this.Values.Keys.Where(p => !currentContexts.Contains(p)).ToArray())
                {
                    this.Values.Remove(locationContext);
                    changed = true;
                }
            }

            return changed;
        });
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetValues(IInputArguments input)
    {
        this.AssertInput(input);

        var values = this
            .GetContextsFor(input)
            .Select(context => context != null && this.Values.TryGetValue(context, out string? weather)
                ? weather
                : Game1.weather_sunny // the game treats an invalid context (e.g. MAX) as always sunny
            )
            .WhereNotNull();

        return InvariantSets.From(values);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the contexts which apply for the given input arguments.</summary>
    /// <param name="input">The input arguments.</param>
    /// <returns>Returns the resulting inputs.</returns>
    private IEnumerable<string?> GetContextsFor(IInputArguments input)
    {
        if (!input.HasPositionalArgs)
        {
            yield return this.CurrentLocationContextName;
            yield break;
        }

        foreach (string context in input.PositionalArgs)
        {
            if (context.EqualsIgnoreCase("current"))
                yield return this.CurrentLocationContextName;
            else
                yield return context;
        }
    }
}
