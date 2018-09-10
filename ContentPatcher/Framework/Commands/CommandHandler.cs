using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>Handles the 'patch' console command.</summary>
    internal class CommandHandler
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages loaded tokens.</summary>
        private readonly TokenManager TokenManager;

        /// <summary>Manages loaded patches.</summary>
        private readonly PatchManager PatchManager;

        /// <summary>A callback which immediately updates the current condition context.</summary>
        private readonly Action UpdateContext;

        /// <summary>The order in which condition types should be listed by <c>patch summary</c>.</summary>
        private readonly ConditionType[] DisplayOrder = {
            // general
            ConditionType.Year,
            ConditionType.Season,
            ConditionType.DayOfWeek,
            ConditionType.Day,
            ConditionType.DayEvent,
            ConditionType.Language,
            ConditionType.Spouse,
            ConditionType.Weather,

            // NPCs
            ConditionType.Hearts,
            ConditionType.Relationship,

            // lookups
            ConditionType.HasFlag,
            ConditionType.HasMod,
            ConditionType.HasSeenEvent
        };

        /// <summary>A regex pattern matching asset names which incorrectly include the Content folder.</summary>
        private readonly Regex AssetNameWithContentPattern = new Regex(@"^Content[/\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>A regex pattern matching asset names which incorrectly include the .xnb extension.</summary>
        private readonly Regex AssetNameWithXnbExtensionPattern = new Regex(@"\.xnb$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>A regex pattern matching asset names which incorrectly include the locale code.</summary>
        private readonly Regex AssetNameWithLocalePattern = new Regex(@"^\.(?:de-DE|es-ES|ja-JP|pt-BR|ru-RU|zh-CN)(?:\.xnb)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the root command.</summary>
        public string CommandName { get; } = "patch";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenManager">Manages loaded tokens.</param>
        /// <param name="patchManager">Manages loaded patches.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="updateContext">A callback which immediately updates the current condition context.</param>
        public CommandHandler(TokenManager tokenManager, PatchManager patchManager, IMonitor monitor, Action updateContext)
        {
            this.TokenManager = tokenManager;
            this.PatchManager = patchManager;
            this.Monitor = monitor;
            this.UpdateContext = updateContext;
        }

        /// <summary>Handle a console command.</summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        public bool Handle(string[] args)
        {
            string subcommand = args.FirstOrDefault();
            string[] subcommandArgs = args.Skip(1).ToArray();

            switch (subcommand?.ToLower())
            {
                case null:
                case "help":
                    return this.HandleHelp(subcommandArgs);

                case "summary":
                    return this.HandleSummary();

                case "update":
                    return this.HandleUpdate();

                default:
                    this.Monitor.Log($"The '{this.CommandName} {args[0]}' command isn't valid. Type '{this.CommandName} help' for a list of valid commands.");
                    return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Commands
        ****/
        /// <summary>Handle the 'patch help' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleHelp(string[] args)
        {
            // generate command info
            var helpEntries = new InvariantDictionary<string>
            {
                ["help"] = $"{this.CommandName} help\n   Usage: {this.CommandName} help\n   Lists all available {this.CommandName} commands.\n\n   Usage: {this.CommandName} help <cmd>\n   Provides information for a specific {this.CommandName} command.\n   - cmd: The {this.CommandName} command name.",
                ["summary"] = $"{this.CommandName} summary\n   Usage: {this.CommandName} summary\n   Shows a summary of the current conditions and loaded patches.",
                ["update"] = $"{this.CommandName} update\n   Usage: {this.CommandName} update\n   Imediately refreshes the condition context and rechecks all patches."
            };

            // build output
            StringBuilder help = new StringBuilder();
            if (!args.Any())
            {
                help.AppendLine(
                    $"The '{this.CommandName}' command is the entry point for Content Patcher commands. These are "
                    + "intended for troubleshooting and aren't intended for players. You use it by specifying a more "
                    + $"specific command (like 'help' in '{this.CommandName} help'). Here are the available commands:\n\n"
                );
                foreach (var entry in helpEntries.OrderBy(p => p.Key))
                {
                    help.AppendLine(entry.Value);
                    help.AppendLine();
                }
            }
            else if (helpEntries.TryGetValue(args[0], out string entry))
                help.AppendLine(entry);
            else
                help.AppendLine($"Unknown command '{this.CommandName} {args[0]}'. Type '{this.CommandName} help' for available commands.");

            // write output
            this.Monitor.Log(help.ToString());

            return true;
        }

        /// <summary>Handle the 'patch summary' command.</summary>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleSummary()
        {
            StringBuilder output = new StringBuilder();

            // add condition summary
            output.AppendLine();
            output.AppendLine("=====================");
            output.AppendLine("==  Global tokens  ==");
            output.AppendLine("=====================");
            foreach (IToken token in this.TokenManager.GetTokens().OrderBy(this.GetDisplayOrder))
            {
                if (token.RequiresSubkeys)
                {
                    foreach (string subkey in token.GetSubkeys().OrderBy(p => p))
                        output.AppendLine($"   {token.Name}:{subkey}: {string.Join(", ", token.GetValues(subkey))}");
                }
                else
                    output.AppendLine($"   {token.Name}: {string.Join(", ", token.GetValues())}");
            }
            output.AppendLine();

            // add patch summary
            var patches = this.GetAllPatches()
                .GroupBy(p => p.ContentPack.Manifest.Name)
                .OrderBy(p => p.Key);

            output.AppendLine(
                "========================\n"
                + "== Content patches ==\n"
                + "========================\n"
                + "The following patches were loaded. For each patch:\n"
                + "  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).\n"
                + "  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.\n"
                + "  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.\n"
                + "\n"
            );
            foreach (IGrouping<string, PatchInfo> patchGroup in patches)
            {
                LocalContext tokenContext = this.TokenManager.TrackLocalTokens(patchGroup.First().ContentPack.Pack);
                output.AppendLine($"{patchGroup.Key}:");
                output.AppendLine("========================");

                // print tokens
                {
                    IToken[] localTokens = tokenContext.GetTokens(localOnly: true).ToArray();
                    if (localTokens.Any())
                    {
                        output.AppendLine();
                        output.AppendLine("   Local tokens:");
                        foreach (IToken token in localTokens.OrderBy(p => p.Name))
                        {
                            if (token.RequiresSubkeys)
                            {
                                foreach (string subkey in token.GetSubkeys().OrderBy(p => p))
                                    output.AppendLine($"      {token.Name}:{subkey}: {string.Join(", ", token.GetValues(subkey))}");
                            }
                            else
                                output.AppendLine($"      {token.Name}: {string.Join(", ", token.GetValues())}");
                        }
                    }
                }

                // print patches
                output.AppendLine();
                output.AppendLine("   loaded  | conditions | applied | name + details");
                output.AppendLine("   ------- | ---------- | ------- | --------------");
                foreach (PatchInfo patch in patchGroup.OrderBy(p => p.ShortName))
                {
                    // log checkbox and patch name
                    output.Append($"   [{(patch.IsLoaded ? "X" : " ")}]     | [{(patch.MatchesContext ? "X" : " ")}]        | [{(patch.IsApplied ? "X" : " ")}]     | {patch.ShortName}");

                    // log raw target (if not in name)
                    if (!patch.ShortName.Contains($"{patch.Type} {patch.RawAssetName}"))
                        output.Append($" | {patch.Type} {patch.RawAssetName}");

                    // log parsed target if tokenised
                    if (patch.MatchesContext && patch.ParsedAssetName != null && patch.ParsedAssetName.Tokens.Any())
                        output.Append($" | => {patch.ParsedAssetName.Value}");

                    // log reason not applied
                    bool hasErrorReason = false;
                    if (!patch.IsLoaded)
                    {
                        output.Append($" | not loaded: {patch.ReasonDisabled}");
                        hasErrorReason = true;
                    }
                    else if (!patch.MatchesContext && patch.ParsedConditions != null)
                    {
                        string[] failedConditions = (
                            from condition in patch.ParsedConditions.Values
                            orderby condition.Key.ToString()
                            where !condition.IsMatch(tokenContext)
                            select $"{condition.Key} ({string.Join(", ", condition.Values)})"
                        ).ToArray();

                        if (failedConditions.Any())
                            output.Append($" | failed conditions: {string.Join(", ", failedConditions)}");
                        hasErrorReason = true;
                    }
                    else if (!patch.IsApplied)
                    {
                        IList<IToken> tokensOutOfContext =
                            patch.TokensUsed
                                .Where(token => !token.IsValidInContext)
                                .ToArray();

                        if (tokensOutOfContext.Any())
                        {
                            output.Append($" | disabled (uses tokens that aren't in context: {string.Join(", ", tokensOutOfContext.Select(p => p.Name).OrderBy(p => p))})");
                            hasErrorReason = true;
                        }
                        else
                        {
                            output.Append(" | disabled (reason unknown)");
                            hasErrorReason = true;
                        }
                    }

                    // log common issues
                    if (!hasErrorReason && patch.IsLoaded && !patch.IsApplied && patch.ParsedAssetName?.Value != null)
                    {
                        string assetName = patch.ParsedAssetName.Value;

                        List<string> issues = new List<string>();
                        if (this.AssetNameWithContentPattern.IsMatch(assetName))
                            issues.Add("shouldn't include 'Content/' prefix");
                        if (this.AssetNameWithXnbExtensionPattern.IsMatch(assetName))
                            issues.Add("shouldn't include '.xnb' extension");
                        if (this.AssetNameWithLocalePattern.IsMatch(assetName))
                            issues.Add("shouldn't include language code (use conditions instead)");

                        if (issues.Any())
                            output.Append($" | NOTE: asset name may be incorrect ({string.Join("; ", issues)}).");
                    }

                    // end line
                    output.AppendLine();
                }
                output.AppendLine(); // blank line between groups
            }

            this.Monitor.Log(output.ToString());
            return true;
        }

        /// <summary>Handle the 'patch update' command.</summary>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleUpdate()
        {
            this.UpdateContext();
            return true;
        }


        /****
        ** Helpers
        ****/
        /// <summary>Get basic info about all patches, including those which couldn't be loaded.</summary>
        public IEnumerable<PatchInfo> GetAllPatches()
        {
            foreach (IPatch patch in this.PatchManager.GetPatches())
                yield return new PatchInfo(patch);
            foreach (DisabledPatch patch in this.PatchManager.GetPermanentlyDisabledPatches())
                yield return new PatchInfo(patch);
        }

        /// <summary>Get the display order for a token in <c>patch summary</c> output.</summary>
        /// <param name="token">The token to check.</param>
        private int GetDisplayOrder(IToken token)
        {
            int index = Array.IndexOf(this.DisplayOrder, token.Name);
            return index != -1
                ? index
                : this.DisplayOrder.Length;
        }
    }
}
