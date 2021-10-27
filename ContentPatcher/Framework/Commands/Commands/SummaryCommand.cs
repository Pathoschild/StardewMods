using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Locations;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which shows a summary of the current conditions and loaded patches.</summary>
    internal class SummaryCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages loaded patches.</summary>
        private readonly Func<PatchManager> GetPatchManager;

        /// <summary>Manages loaded tokens.</summary>
        private readonly Func<TokenManager> GetTokenManager;

        /// <summary>Handles loading custom location data and adding it to the game.</summary>
        private readonly Func<CustomLocationManager> GetCustomLocationLoader;

        /// <summary>A regex pattern matching asset names which incorrectly include the Content folder.</summary>
        private readonly Regex AssetNameWithContentPattern = new(@"^Content[/\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>A regex pattern matching asset names which incorrectly include an extension.</summary>
        private readonly Regex AssetNameWithExtensionPattern = new(@"(\.\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>A regex pattern matching asset names which incorrectly include the locale code.</summary>
        private readonly Regex AssetNameWithLocalePattern = new(@"^\.(?:de-DE|es-ES|ja-JP|pt-BR|ru-RU|zh-CN)(?:\.xnb)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>The tokens to sort manually for display.</summary>
        /// <remarks>This avoids the performance impact of sorting the actual token each time the context is updated. Note that we shouldn't sort all tokens here, since some have a natural order that affects the <see cref="InputArguments.ValueAtKey"/> input argument.</remarks>
        private readonly HashSet<ConditionType> SortTokens = new()
        {
            ConditionType.HasActiveQuest,
            ConditionType.HasCaughtFish,
            ConditionType.HasCookingRecipe,
            ConditionType.HasCraftingRecipe,
            ConditionType.HasConversationTopic,
            ConditionType.HasDialogueAnswer,
            ConditionType.HasFlag,
            ConditionType.HasProfession,
            ConditionType.HasReadLetter,
            ConditionType.HasSeenEvent,
            ConditionType.SkillLevel
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="getPatchManager">Manages loaded patches.</param>
        /// <param name="getTokenManager">Manages loading and unloading patches.</param>
        /// <param name="getCustomLocationLoader">Handles loading custom location data and adding it to the game.</param>
        public SummaryCommand(IMonitor monitor, Func<PatchManager> getPatchManager, Func<TokenManager> getTokenManager, Func<CustomLocationManager> getCustomLocationLoader)
            : base(monitor, "summary")
        {
            this.GetPatchManager = getPatchManager;
            this.GetTokenManager = getTokenManager;
            this.GetCustomLocationLoader = getCustomLocationLoader;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch summary
                   Usage: patch summary
                   Shows a summary of the current conditions and loaded patches.

                   Usage: patch summary ""<content pack ID>""
                   Show a summary of the current conditions, and loaded patches for the given content pack.

                   You can also specify any number of optional flags (e.g. `patch summary full unsorted`):
                      - full: don't truncate very long token values.
                      - unsorted: don't sort the values for display. This is mainly useful for checking the real order for `valueAt`.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            var patchManager = this.GetPatchManager();
            var tokenManager = this.GetTokenManager();

            StringBuilder output = new();
            LogPathBuilder path = new("console command");

            // parse arguments
            bool showFull = false;
            bool sort = true;
            var forModIds = new InvariantHashSet();
            foreach (string arg in args)
            {
                // flags
                switch (arg.ToLower())
                {
                    case "full":
                        showFull = true;
                        continue;

                    case "unsorted":
                        sort = false;
                        continue;
                }

                // for mod ID
                forModIds.Add(arg);
            }

            // truncate token values if needed
            string GetTruncatedTokenValues(IEnumerable<string> values)
            {
                const int maxLength = 200;
                const string truncatedSuffix = "... (use `patch summary full` to see other values)";

                string valueStr = string.Join(", ", values);
                return showFull || valueStr.Length <= maxLength
                    ? valueStr
                    : $"{valueStr.Substring(0, maxLength - truncatedSuffix.Length)}{truncatedSuffix}";
            }

            // add condition summary
            output.AppendLine();
            output.AppendLine("=====================");
            output.AppendLine("==  Global tokens  ==");
            output.AppendLine("=====================");
            {
                // get data
                var tokensByProvider =
                    (
                        from token in tokenManager.GetTokens(enforceContext: false).OrderByHuman(p => p.Name)
                        let inputArgs = token.GetAllowedInputArguments().ToArray()
                        let rootValues = !token.RequiresInput ? this.GetValues(token, InputArguments.Empty, sort).ToArray() : new string[0]
                        let isMultiValue =
                            inputArgs.Length > 1
                            || rootValues.Length > 1
                            || (inputArgs.Length == 1 && this.GetValues(token, new InputArguments(new LiteralString(inputArgs[0], path.With(token.Name, "input"))), sort).Count() > 1)
                        let mod = (token as ModProvidedToken)?.Mod
                        orderby isMultiValue // single-value tokens first, then alphabetically
                        select new { Mod = mod, Token = token }
                    )
                    .GroupBy(p => p.Mod?.Name?.Trim())
                    .OrderByHuman(p => p.Key) // default tokens (key is null), then tokens added by other mods
                    .ToArray();
                int labelWidth = Math.Max(tokensByProvider.Max(group => group.Max(p => p.Token.Name.Length)), "token name".Length);

                // group by provider mod (if any)
                foreach (var tokenGroup in tokensByProvider)
                {
                    if (tokenGroup.Key != null && forModIds.Any() && !forModIds.Contains(tokenGroup.First().Mod.UniqueID))
                        continue;

                    // print mod name
                    output.AppendLine($"   {tokenGroup.Key ?? "Content Patcher"}:");
                    output.AppendLine();

                    // print table header
                    output.AppendLine($"      {"token name".PadRight(labelWidth)} | value");
                    output.AppendLine($"      {"".PadRight(labelWidth, '-')} | -----");

                    // print tokens
                    foreach (IToken token in tokenGroup.Select(p => p.Token))
                    {
                        output.Append($"      {token.Name.PadRight(labelWidth)} | ");

                        if (!token.IsReady)
                            output.AppendLine("[ ] n/a");
                        else if (token.RequiresInput)
                        {
                            InvariantHashSet allowedInputs = token.GetAllowedInputArguments();
                            if (allowedInputs.Any())
                            {
                                bool isFirst = true;
                                foreach (string input in allowedInputs.OrderByHuman())
                                {
                                    if (isFirst)
                                    {
                                        output.Append("[X] ");
                                        isFirst = false;
                                    }
                                    else
                                        output.Append($"      {"".PadRight(labelWidth, ' ')} |     ");

                                    output.AppendLine($":{input}: {GetTruncatedTokenValues(this.GetValues(token, new InputArguments(new LiteralString(input, path.With(token.Name, "input"))), sort))}");
                                }
                            }
                            else
                                output.AppendLine("[X] (token returns a dynamic value)");
                        }
                        else
                            output.AppendLine("[X] " + GetTruncatedTokenValues(this.GetValues(token, InputArguments.Empty, sort)));
                    }

                    output.AppendLine();
                }
            }

            // list custom locations
            {
                var locations = this.GetCustomLocationLoader().GetCustomLocationData()
                    .Where(p => !forModIds.Any() || forModIds.Contains(p.ModId))
                    .GroupByIgnoreCase(p => p.ModName)
                    .OrderByHuman(p => p.Key)
                    .ToArray();

                if (locations.Any())
                {
                    output.AppendLine(
                        "======================\n"
                        + "== Custom locations ==\n"
                        + "======================\n"
                        + "The following custom locations were created by content packs.\n"
                        + (forModIds.Any() ? $"\n(Filtered to content pack ID{(forModIds.Count > 1 ? "s" : "")}: {string.Join(", ", forModIds.OrderByHuman())}.)\n" : "")
                    );

                    foreach (var locationGroup in locations)
                    {
                        int nameWidth = Math.Max("location name".Length, locationGroup.Max(p => p.Name.Length));

                        output.AppendLine($"{locationGroup.Key}:");
                        output.AppendLine("".PadRight(locationGroup.Key.Length + 1, '-'));
                        output.AppendLine();
                        output.AppendLine($"   {"location name".PadRight(nameWidth)}  | status");
                        output.AppendLine($"   {"".PadRight(nameWidth, '-')}  | ------");
                        foreach (CustomLocationData location in locationGroup.OrderByHuman(p => p.Name))
                            output.AppendLine($"   {location.Name.PadRight(nameWidth)}  | {(location.IsEnabled ? "[X] ok" : $"[ ] error: {location.Error}")}");
                        output.AppendLine();
                    }
                }

                output.AppendLine();
            }


            // list patches
            {
                var patches = this.GetAllPatches(patchManager)
                    .Where(p => !forModIds.Any() || forModIds.Contains(p.ContentPack.Manifest.UniqueID))
                    .GroupByIgnoreCase(p => p.ContentPack.Manifest.Name)
                    .OrderByHuman(p => p.Key)
                    .ToArray();

                output.AppendLine(
                    "=====================\n"
                    + "== Content patches ==\n"
                    + "=====================\n"
                    + "The following patches were loaded. For each patch:\n"
                    + "  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).\n"
                    + "  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.\n"
                    + "  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.\n"
                    + (forModIds.Any() ? $"\n(Filtered to content pack ID{(forModIds.Count > 1 ? "s" : "")}: {string.Join(", ", forModIds.OrderByHuman())}.)\n" : "")
                    + "\n"
                );
                foreach (var patchGroup in patches)
                {
                    ModTokenContext tokenContext = tokenManager.TrackLocalTokens(patchGroup.First().ContentPack);
                    output.AppendLine($"{patchGroup.Key}:");
                    output.AppendLine("".PadRight(patchGroup.Key.Length + 1, '-'));

                    // print tokens
                    {
                        var tokens =
                            (
                                // get non-global tokens
                                from IToken token in tokenContext.GetTokens(enforceContext: false)
                                where token.Scope != null

                                // get input arguments
                                let validInputs = token.IsReady && token.RequiresInput
                                    ? token.GetAllowedInputArguments().Select(p => new LiteralString(p, path.With(patchGroup.Key, token.Name, $"input '{p}'"))).AsEnumerable<ITokenString>()
                                    : new ITokenString[] { null }
                                from ITokenString input in validInputs

                                where !token.RequiresInput || validInputs.Any() // don't show tokens which can't be represented

                                // select display data
                                let result = new
                                {
                                    Name = token.RequiresInput ? $"{token.Name}:{input}" : token.Name,
                                    Values = token.IsReady ? this.GetValues(token, new InputArguments(input), sort).ToArray() : new string[0],
                                    IsReady = token.IsReady
                                }
                                orderby result.Name
                                select result
                            )
                            .ToArray();
                        if (tokens.Any())
                        {
                            int labelWidth = Math.Max(tokens.Max(p => p.Name.Length), "token name".Length);

                            output.AppendLine();
                            output.AppendLine("   Local tokens:");

                            output.AppendLine($"      {"token name".PadRight(labelWidth)} | value");
                            output.AppendLine($"      {"".PadRight(labelWidth, '-')} | -----");

                            foreach (var token in tokens)
                                output.AppendLine($"      {token.Name.PadRight(labelWidth)} | [{(token.IsReady ? "X" : " ")}] {GetTruncatedTokenValues(token.Values)}");
                        }
                    }

                    // print patches
                    output.AppendLine();
                    output.AppendLine("   Patches:");
                    output.AppendLine("      loaded  | conditions | applied | name + details");
                    output.AppendLine("      ------- | ---------- | ------- | --------------");
                    foreach (PatchInfo patch in patchGroup.OrderBy(p => p, new PatchDisplaySortComparer()))
                    {
                        // log checkbox and patch name
                        output.Append($"      [{(patch.IsLoaded ? "X" : " ")}]     | [{(patch.MatchesContext ? "X" : " ")}]        | [{(patch.IsApplied ? "X" : " ")}]     | {patch.PathWithoutContentPackPrefix}");

                        // log target value if different from name
                        {
                            // get patch values
                            string rawIdentifyingPath = PathUtilities.NormalizeAssetName(patch.ParsedType == PatchType.Include
                                ? patch.RawFromAsset
                                : patch.RawTargetAsset
                            );
                            ITokenString parsedIdentifyingPath = patch.ParsedType == PatchType.Include
                                ? patch.ParsedFromAsset
                                : patch.ParsedTargetAsset;

                            // get raw name if different
                            // (ignore differences in whitespace, capitalization, and path separators)
                            string rawValue = !PathUtilities.NormalizeAssetName(patch.PathWithoutContentPackPrefix.ToString().Replace(" ", "")).ContainsIgnoreCase(rawIdentifyingPath?.Replace(" ", ""))
                                ? $"{patch.ParsedType?.ToString() ?? patch.RawType} {rawIdentifyingPath}"
                                : null;

                            // get parsed value
                            string parsedValue = patch.MatchesContext && parsedIdentifyingPath?.HasAnyTokens == true
                                ? PathUtilities.NormalizeAssetName(parsedIdentifyingPath.Value)
                                : null;

                            // format
                            if (rawValue != null || parsedValue != null)
                            {
                                output.Append(" (");
                                if (rawValue != null)
                                {
                                    output.Append(rawValue);
                                    if (parsedValue != null)
                                        output.Append(" ");
                                }
                                if (parsedValue != null)
                                    output.Append($"=> {parsedValue}");
                                output.Append(")");
                            }
                        }

                        // log reason not applied
                        string errorReason = patch.GetReasonNotLoaded();
                        if (errorReason != null)
                            output.Append($"  // {errorReason}");

                        // log common issues if not applied
                        if (errorReason == null && patch.IsLoaded && !patch.IsApplied && patch.ParsedTargetAsset.IsMeaningful())
                        {
                            string assetName = patch.ParsedTargetAsset.Value;

                            List<string> issues = new List<string>();
                            if (this.AssetNameWithContentPattern.IsMatch(assetName))
                                issues.Add("shouldn't include 'Content/' prefix");
                            if (this.AssetNameWithExtensionPattern.IsMatch(assetName))
                            {
                                Match match = this.AssetNameWithExtensionPattern.Match(assetName);
                                issues.Add($"shouldn't include '{match.Captures[0]}' extension");
                            }
                            if (this.AssetNameWithLocalePattern.IsMatch(assetName))
                                issues.Add("shouldn't include language code (use conditions instead)");

                            if (issues.Any())
                                output.Append($" // hint: asset name may be incorrect ({string.Join("; ", issues)}).");
                        }

                        // log update rate issues
                        if (patch.Patch != null)
                        {
                            foreach (var pair in tokenManager.TokensWithSpecialUpdateRates)
                            {
                                UpdateRate rate = pair.Item1;
                                string label = pair.Item2;
                                InvariantHashSet tokenNames = pair.Item3;

                                if (!patch.Patch.UpdateRate.HasFlag(rate))
                                {
                                    var tokensUsed = new InvariantHashSet(patch.Patch.GetTokensUsed());

                                    string[] locationTokensUsed = tokenNames.Where(p => tokensUsed.Contains(p)).ToArray();
                                    if (locationTokensUsed.Any())
                                        output.Append($" // hint: patch uses {label}, but doesn't set \"{nameof(PatchConfig.Update)}\": \"{rate}\".");
                                }
                            }
                        }

                        // end line
                        output.AppendLine();
                    }

                    // print patch effects
                    {
                        IDictionary<string, InvariantHashSet> effectsByPatch = new Dictionary<string, InvariantHashSet>(StringComparer.OrdinalIgnoreCase);
                        foreach (PatchInfo patch in patchGroup)
                        {
                            if (!patch.IsApplied || patch.Patch == null)
                                continue;

                            string[] changeLabels = patch.GetChangeLabels().ToArray();
                            if (!changeLabels.Any())
                                continue;

                            if (!effectsByPatch.TryGetValue(patch.ParsedTargetAsset.Value, out InvariantHashSet effects))
                                effectsByPatch[patch.ParsedTargetAsset.Value] = effects = new InvariantHashSet();

                            effects.AddMany(patch.GetChangeLabels());
                        }

                        output.AppendLine();
                        if (effectsByPatch.Any())
                        {
                            int maxAssetNameWidth = Math.Max("asset name".Length, effectsByPatch.Max(p => p.Key.Length));

                            output.AppendLine("   Current changes:");
                            output.AppendLine($"      asset name{"".PadRight(maxAssetNameWidth - "asset name".Length)} | changes");
                            output.AppendLine($"      ----------{"".PadRight(maxAssetNameWidth - "----------".Length, '-')} | -------");

                            foreach (var pair in effectsByPatch.OrderByHuman(p => p.Key))
                                output.AppendLine($"      {pair.Key}{"".PadRight(maxAssetNameWidth - pair.Key.Length)} | {string.Join("; ", pair.Value.OrderByHuman())}");
                        }
                        else
                            output.AppendLine("   No current changes.");
                    }

                    // add blank line between groups
                    output.AppendLine();
                }
            }

            this.Monitor.Log(output.ToString(), LogLevel.Debug);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get basic info about all patches, including those which couldn't be loaded.</summary>
        /// <param name="patchManager">Manages loaded patches.</param>
        private IEnumerable<PatchInfo> GetAllPatches(PatchManager patchManager)
        {
            foreach (IPatch patch in patchManager.GetPatches())
                yield return new PatchInfo(patch);
            foreach (DisabledPatch patch in patchManager.GetPermanentlyDisabledPatches())
                yield return new PatchInfo(patch);
        }

        /// <summary>Get the values for a token in display order.</summary>
        /// <param name="token">The token whose values to get.</param>
        /// <param name="input">The input arguments for the token.</param>
        /// <param name="sort">Whether to sort the values for display.</param>
        private IEnumerable<string> GetValues(IToken token, IInputArguments input, bool sort)
        {
            IEnumerable<string> values = token.GetValues(input);

            if (sort && Enum.TryParse(token.Name, ignoreCase: true, out ConditionType type) && this.SortTokens.Contains(type))
                values = values.OrderByHuman();

            return values;
        }
    }
}
