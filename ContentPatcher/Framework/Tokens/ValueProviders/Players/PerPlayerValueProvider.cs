using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens.ValueProviders.Players;

/// <summary>A value provider for a built-in condition whose value may change with the context and which c, and accepts flags to indicate whether to check the host or local player.</summary>
internal class PerPlayerValueProvider : BaseValueProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles reading info from the current save.</summary>
    private readonly TokenSaveReader SaveReader;

    /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
    private readonly IInvariantSet? AllowedRootValues;

    /// <summary>Get the current values for a player.</summary>
    private readonly Func<Farmer, IInvariantSet> FetchValues;

    /// <summary>The values as of the last context update.</summary>
    private readonly Dictionary<long, IInvariantSet> Values = [];

    /// <summary>The player ID for the host player.</summary>
    private long HostPlayerId;

    /// <summary>The player ID for the local player.</summary>
    private long LocalPlayerId;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="type">The condition type.</param>
    /// <param name="values">Get the current values.</param>
    /// <param name="saveReader">Handles reading info from the current save.</param>
    /// <param name="mayReturnMultipleValues">Whether the root may contain multiple values.</param>
    /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
    public PerPlayerValueProvider(ConditionType type, Func<Farmer, IEnumerable<string>> values, TokenSaveReader saveReader, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
        : base(type, mayReturnMultipleValues)
    {
        this.SaveReader = saveReader;
        this.AllowedRootValues = allowedValues != null ? InvariantSets.From(allowedValues) : null;
        this.FetchValues = player => InvariantSets.From(values(player));
        this.EnableInputArguments(required: false, mayReturnMultipleValues: mayReturnMultipleValues, maxPositionalArgs: null);
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="type">The condition type.</param>
    /// <param name="value">Get the current value.</param>
    /// <param name="saveReader">Handles reading info from the current save.</param>
    /// <param name="mayReturnMultipleValues">Whether the root may contain multiple values.</param>
    /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
    public PerPlayerValueProvider(ConditionType type, Func<Farmer, string?> value, TokenSaveReader saveReader, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
        : this(type, player => BaseValueProvider.WrapOptionalValue(value(player)), saveReader, mayReturnMultipleValues, allowedValues) { }

    /// <inheritdoc />
    public override bool UpdateContext(IContext context)
    {
        return this.IsChanged(() =>
        {
            bool changed = false;

            if (this.MarkReady(this.SaveReader.IsReady))
            {
                // update host/local player IDs
                long hostId = this.SaveReader.GetPlayer(PlayerType.HostPlayer)?.UniqueMultiplayerID ?? 0;
                long localId = this.SaveReader.GetPlayer(PlayerType.CurrentPlayer)?.UniqueMultiplayerID ?? 0;

                changed |= this.HostPlayerId != hostId || this.LocalPlayerId != localId;

                this.HostPlayerId = hostId;
                this.LocalPlayerId = localId;

                // update values by player ID
                HashSet<long> removeIds = [.. this.Values.Keys];
                foreach (Farmer player in this.SaveReader.GetAllPlayers())
                {
                    // get values
                    long id = player.UniqueMultiplayerID;
                    IInvariantSet newValues = this.FetchValues(player);
                    if (!this.Values.TryGetValue(id, out IInvariantSet? oldValues))
                        oldValues = null;

                    // track changes
                    removeIds.Remove(id);
                    changed |= oldValues == null || this.IsChanged(oldValues, newValues);

                    // update
                    this.Values[id] = newValues;
                }

                // remove players if needed
                foreach (long id in removeIds)
                {
                    this.Values.Remove(id);
                    changed = true;
                }
            }
            else
            {
                this.Values.Clear();
                this.HostPlayerId = 0;
                this.LocalPlayerId = 0;
            }

            return changed;
        });
    }

    /// <inheritdoc />
    public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
    {
        return
            base.TryValidateInput(input, out error)
            && (
                !input.HasPositionalArgs
                || this.TryParseInput(input, null, out error)
            );
    }

    /// <inheritdoc />
    public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
    {
        allowedValues = this.AllowedRootValues;
        return allowedValues != null;
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetValues(IInputArguments input)
    {
        this.AssertInput(input);

        if (!input.HasPositionalArgs)
            return this.GetValuesFor(null);

        var playerIds = new HashSet<long>();
        return this.TryParseInput(input, playerIds, out _)
            ? this.GetValuesFor(playerIds)
            : InvariantSets.Empty;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Parse the input arguments if valid.</summary>
    /// <param name="input">The input arguments.</param>
    /// <param name="playerIds">The parsed player IDs to populate, if any.</param>
    /// <param name="error">The error indicating why the input is invalid, if applicable.</param>
    /// <returns>Returns whether the input is valid.</returns>
    private bool TryParseInput(IInputArguments input, HashSet<long>? playerIds, [NotNullWhen(false)] out string? error)
    {
        foreach (string arg in input.PositionalArgs)
        {
            if (long.TryParse(arg, out long playerId))
            {
                playerIds?.Add(playerId);
            }
            else if (Enum.TryParse(arg, ignoreCase: true, out PlayerType type))
            {
                switch (type)
                {
                    case PlayerType.HostPlayer:
                        playerIds?.Add(this.HostPlayerId);
                        break;

                    case PlayerType.CurrentPlayer:
                        playerIds?.Add(this.LocalPlayerId);
                        break;

                    case PlayerType.AnyPlayer:
                        playerIds?.AddRange(this.Values.Keys);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown player type {type}.");
                }
            }
            else
            {
                error = $"invalid input arguments ({input.TokenString}) for {this.Name} token, expected any combination of '{string.Join("', '", Enum.GetNames(typeof(PlayerType)))}', or player IDs.";
                return false;
            }
        }

        error = null;
        return true;
    }

    /// <summary>Get the cached values for the given player IDs.</summary>
    /// <param name="playerIds">The player IDs.</param>
    private IEnumerable<string> GetValuesFor(HashSet<long>? playerIds)
    {
        // get single value (avoids copying the collection in most cases)
        if (playerIds is not { Count: > 1 })
            return this.GetValuesFor(playerIds?.FirstOrDefault() ?? this.LocalPlayerId);

        // get multiple values
        HashSet<string> values = [];
        foreach (long id in playerIds)
        {
            if (this.Values.TryGetValue(id, out IInvariantSet? set))
                values.AddRange(set);
        }
        return values;
    }

    /// <summary>Get the cached values for the given player ID.</summary>
    /// <param name="playerId">The player ID.</param>
    private IInvariantSet GetValuesFor(long playerId)
    {
        return this.Values.TryGetValue(playerId, out IInvariantSet? set)
            ? set
            : InvariantSets.Empty;
    }
}
