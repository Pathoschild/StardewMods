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
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages loaded tokens.</summary>
        private readonly TokenManager TokenManager;

        /// <summary>Manages loaded patches.</summary>
        private readonly PatchManager PatchManager;

        /// <summary>A callback which immediately updates the current condition context.</summary>
        private readonly Action UpdateContext;

        /// <summary>A regex pattern matching asset names which incorrectly include the Content folder.</summary>
        private readonly Regex AssetNameWithContentPattern = new Regex(@"^Content[/\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>A regex pattern matching asset names which incorrectly include an extension.</summary>
        private readonly Regex AssetNameWithExtensionPattern = new Regex(@"(\.\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                foreach (var entry in helpEntries.OrderByIgnoreCase(p => p.Key))
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
            {
                // get data
                IToken[] tokens =
                    (
                        from token in this.TokenManager.GetTokens(enforceContext: false)
                        let inputArgs = token.GetAllowedInputArguments().ToArray()
                        let rootValues = !token.RequiresInput ? token.GetValues(null).ToArray() : new string[0]
                        let isMultiValue =
                            inputArgs.Length > 1
                            || rootValues.Length > 1
                            || (inputArgs.Length == 1 && token.GetValues(new LiteralString(inputArgs[0])).Count() > 1)
                        orderby isMultiValue, token.Name // single-value tokens first, then alphabetically
                        select token
                    )
                    .ToArray();
                int labelWidth = tokens.Max(p => p.Name.Length);

                // print table header
                output.AppendLine($"   {"token name".PadRight(labelWidth)} | value");
                output.AppendLine($"   {"".PadRight(labelWidth, '-')} | -----");

                // print tokens
                foreach (IToken token in tokens)
                {
                    output.Append($"   {token.Name.PadRight(labelWidth)} | ");

                    if (!token.IsReady)
                        output.AppendLine("[ ] n/a");
                    else if (token.RequiresInput)
                    {
                        bool isFirst = true;
                        foreach (string input in token.GetAllowedInputArguments().OrderByIgnoreCase(input => input))
                        {
                            if (isFirst)
                            {
                                output.Append("[X] ");
                                isFirst = false;
                            }
                            else
                                output.Append($"   {"".PadRight(labelWidth, ' ')} |     ");
                            output.AppendLine($":{input}: {string.Join(", ", token.GetValues(new LiteralString(input)))}");
                        }
                    }
                    else
                        output.AppendLine("[X] " + string.Join(", ", token.GetValues(null).OrderByIgnoreCase(p => p)));
                }
            }
            output.AppendLine();

            // add patch summary
            var patches = this.GetAllPatches()
                .GroupByIgnoreCase(p => p.ContentPack.Manifest.Name)
                .OrderByIgnoreCase(p => p.Key);

            output.AppendLine(
                "=====================\n"
                + "== Content patches ==\n"
                + "=====================\n"
                + "The following patches were loaded. For each patch:\n"
                + "  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).\n"
                + "  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.\n"
                + "  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.\n"
                + "\n"
            );
            foreach (IGrouping<string, PatchInfo> patchGroup in patches)
            {
                ModTokenContext tokenContext = this.TokenManager.TrackLocalTokens(patchGroup.First().ContentPack.Pack);
                output.AppendLine($"{patchGroup.Key}:");
                output.AppendLine("".PadRight(patchGroup.Key.Length + 1, '-'));

                // print tokens
                {
                    var tokens =
                        (
                            // get non-global tokens
                            from IToken token in tokenContext.GetTokens(enforceContext: false)
                            where token.Scope != null && token.Name != ConditionType.HasFile.ToString()

                            // get input arguments
                            let validInputs = token.IsReady && token.RequiresInput
                                ? token.GetAllowedInputArguments().Select(p => new LiteralString(p)).AsEnumerable<ITokenString>()
                                : new ITokenString[] { null }
                            from ITokenString input in validInputs

                            // select display data
                            let result = new
                            {
                                Name = token.RequiresInput ? $"{token.Name}:{input}" : token.Name,
                                Value = token.IsReady ? string.Join(", ", token.GetValues(input)) : "",
                                IsReady = token.IsReady
                            }
                            orderby result.Name
                            select result
                        )
                        .ToArray();
                    if (tokens.Any())
                    {
                        int maxNameWidth = tokens.Max(p => p.Name.Length);

                        output.AppendLine();
                        output.AppendLine("   Local tokens:");

                        output.AppendLine($"      {"token name".PadRight(maxNameWidth)} | value");
                        output.AppendLine($"      {"----------".PadRight(maxNameWidth, '-')} | -----");

                        foreach (var token in tokens)
                            output.AppendLine($"      {token.Name.PadRight(maxNameWidth)} | [{(token.IsReady ? "X" : " ")}] {token.Value}");
                    }
                }

                // print patches
                output.AppendLine();
                output.AppendLine("   loaded  | conditions | applied | name + details");
                output.AppendLine("   ------- | ---------- | ------- | --------------");
                foreach (PatchInfo patch in patchGroup.OrderByIgnoreCase(p => p.ShortName))
                {
                    // log checkbox and patch name
                    output.Append($"   [{(patch.IsLoaded ? "X" : " ")}]     | [{(patch.MatchesContext ? "X" : " ")}]        | [{(patch.IsApplied ? "X" : " ")}]     | {patch.ShortName}");

                    // log raw target (if not in name)
                    if (!patch.ShortName.Contains($"{patch.Type} {patch.RawTargetAsset}"))
                        output.Append($" | {patch.Type} {patch.RawTargetAsset}");

                    // log parsed target if tokenised
                    if (patch.MatchesContext && patch.ParsedTargetAsset != null && patch.ParsedTargetAsset.HasAnyTokens)
                        output.Append($" | => {patch.ParsedTargetAsset.Value}");

                    // log reason not applied
                    string errorReason = this.GetReasonNotLoaded(patch);
                    if (errorReason != null)
                        output.Append($"  // {errorReason}");

                    // log common issues
                    if (errorReason == null && patch.IsLoaded && !patch.IsApplied && patch.ParsedTargetAsset.IsMeaningful())
                    {
                        string assetName = patch.ParsedTargetAsset.Value;

                        List<string> issues = new List<string>();
                        if (this.AssetNameWithContentPattern.IsMatch(assetName))
                            issues.Add("shouldn't include 'Content/' prefix");
                        if (this.AssetNameWithExtensionPattern.IsMatch(assetName))
                        {
                            var match = this.AssetNameWithExtensionPattern.Match(assetName);
                            issues.Add($"shouldn't include '{match.Captures[0]}' extension");
                        }
                        if (this.AssetNameWithLocalePattern.IsMatch(assetName))
                            issues.Add("shouldn't include language code (use conditions instead)");

                        if (issues.Any())
                            output.Append($" | hint: asset name may be incorrect ({string.Join("; ", issues)}).");
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

        /// <summary>Get a human-readable reason that the patch isn't applied.</summary>
        /// <param name="patch">The patch to check.</param>
        private string GetReasonNotLoaded(PatchInfo patch)
        {
            if (patch.IsApplied)
                return null;

            IContext tokenContext = patch.PatchContext;

            // load error
            if (!patch.IsLoaded)
                return $"not loaded: {patch.ReasonDisabled}";

            // uses tokens not available in the current context
            {
                string[] tokensOutOfContext = patch
                    .TokensUsed
                    .Union(patch.ParsedConditions.Select(p => p.Name))
                    .Distinct()
                    .Where(name => !tokenContext.GetToken(name, enforceContext: false).IsReady)
                    .OrderByIgnoreCase(name => name)
                    .ToArray();
                if (tokensOutOfContext.Any())
                    return $"uses tokens not available right now: {string.Join(", ", tokensOutOfContext)}";
            }

            // conditions not matched
            if (!patch.MatchesContext && patch.ParsedConditions != null)
            {
                string[] failedConditions = (
                    from condition in patch.ParsedConditions
                    let displayText = !string.IsNullOrWhiteSpace(condition.Input?.Raw)
                        ? $"{condition.Name}:{condition.Input.Raw}"
                        : condition.Name
                    let displayValue = condition.Values.HasAnyTokens
                        ? $"{condition.Values.Raw} => {string.Join(", ", condition.CurrentValues)}"
                        : $"{string.Join(", ", condition.CurrentValues)}"
                    orderby displayText
                    where !condition.IsMatch(tokenContext)
                    select $"{displayText} (expected one of {displayValue})"
                ).ToArray();

                if (failedConditions.Any())
                    return $"conditions don't match: {string.Join(", ", failedConditions)}";
            }

            return null;
        }
    }
}
