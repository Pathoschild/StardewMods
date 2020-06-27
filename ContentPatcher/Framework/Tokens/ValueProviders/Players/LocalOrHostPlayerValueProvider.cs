using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders.Players
{
    /// <summary>A value provider for a built-in condition whose value may change with the context, and accepts flags to indicate whether to check the host or local player.</summary>
    internal class LocalOrHostPlayerValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>Get the current values for a player.</summary>
        private readonly Func<Farmer, InvariantHashSet> FetchValues;

        /// <summary>Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</summary>
        private readonly Func<bool> IsPlayerLoaded;

        /// <summary>The values as of the last context update.</summary>
        private readonly IDictionary<PlayerType, InvariantHashSet> Values = new Dictionary<PlayerType, InvariantHashSet>
        {
            [PlayerType.CurrentPlayer] = new InvariantHashSet(),
            [PlayerType.HostPlayer] = new InvariantHashSet()
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="values">Get the current values.</param>
        /// <param name="isPlayerLoaded">Get whether the player is loaded and ready.</param>
        /// <param name="mayReturnMultipleValues">Whether the root may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public LocalOrHostPlayerValueProvider(ConditionType type, Func<Farmer, IEnumerable<string>> values, Func<bool> isPlayerLoaded, bool mayReturnMultipleValues = false, IEnumerable<string> allowedValues = null)
            : base(type, mayReturnMultipleValues)
        {
            this.IsPlayerLoaded = isPlayerLoaded;
            this.AllowedRootValues = allowedValues != null ? new InvariantHashSet(allowedValues) : null;
            this.FetchValues = player => new InvariantHashSet(values(player));
            this.EnableInputArguments(required: false, mayReturnMultipleValues: true, maxPositionalArgs: null);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="value">Get the current value.</param>
        /// <param name="isPlayerLoaded">Get whether the player is loaded and ready.</param>
        /// <param name="mayReturnMultipleValues">Whether the root may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public LocalOrHostPlayerValueProvider(ConditionType type, Func<Farmer, string> value, Func<bool> isPlayerLoaded, bool mayReturnMultipleValues = false, IEnumerable<string> allowedValues = null)
            : this(type, player => new[] { value(player) }, isPlayerLoaded, mayReturnMultipleValues, allowedValues) { }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                bool changed = false;

                foreach (var pair in this.Values)
                {
                    PlayerType type = pair.Key;
                    InvariantHashSet values = pair.Value;
                    InvariantHashSet oldValues = new InvariantHashSet(values);

                    values.Clear();

                    if (this.MarkReady(this.IsPlayerLoaded()))
                    {
                        Farmer player = type == PlayerType.HostPlayer
                            ? Game1.MasterPlayer
                            : Game1.player;

                        if (player != null)
                        {
                            foreach (string value in this.FetchValues(player))
                                values.Add(value);
                        }
                    }

                    changed |= this.IsChanged(oldValues, values);
                }

                return changed;
            });
        }

        /// <inheritdoc />
        public override InvariantHashSet GetValidPositionalArgs()
        {
            return new InvariantHashSet(this.Values.Keys.Select(p => p.ToString()));
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = this.AllowedRootValues;
            return allowedValues != null;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return this.TryParseInput(input, out ISet<PlayerType> playerTypes, out _)
                ? this.GetValuesFor(playerTypes)
                : Enumerable.Empty<string>();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse the input arguments if valid.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="playerTypes">The parsed player types.</param>
        /// <param name="error">The error indicating why the input is invalid, if applicable.</param>
        /// <returns>Returns whether the input is valid.</returns>
        private bool TryParseInput(IInputArguments input, out ISet<PlayerType> playerTypes, out string error)
        {
            playerTypes = new HashSet<PlayerType>();
            foreach (string arg in input.PositionalArgs)
            {
                if (!Enum.TryParse(arg, ignoreCase: true, out PlayerType type))
                {
                    error = $"invalid input arguments ({input.TokenString.Value}) for {this.Name} token, expected any of {string.Join(", ", this.Values.Keys.Select(p => p.ToString()))}.";
                    return false;
                }

                playerTypes.Add(type);
            }

            error = null;
            return true;
        }

        /// <summary>Get the cached values for the given player types.</summary>
        /// <param name="playerTypes">The player types.</param>
        private IEnumerable<string> GetValuesFor(ISet<PlayerType> playerTypes)
        {
            // current player
            if (playerTypes.Count == 0 || Context.IsMainPlayer)
                return this.Values[PlayerType.CurrentPlayer];

            // one player type
            if (playerTypes.Count == 1)
                return this.Values[playerTypes.First()];

            // multiple player types
            return new InvariantHashSet(playerTypes.SelectMany(type => this.Values[type]));
        }
    }
}
