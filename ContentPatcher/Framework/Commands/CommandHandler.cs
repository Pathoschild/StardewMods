using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Utilities;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Locations;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

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

        /// <summary>Manages state for each screen.</summary>
        private readonly PerScreen<ScreenManager> ScreenManager;

        /// <summary>Manages loaded tokens.</summary>
        private TokenManager TokenManager => this.ScreenManager.Value.TokenManager;

        /// <summary>Manages loaded patches.</summary>
        private PatchManager PatchManager => this.ScreenManager.Value.PatchManager;

        /// <summary>Manages loading and unloading patches.</summary>
        private PatchLoader PatchLoader => this.ScreenManager.Value.PatchLoader;

        /// <summary>Handles loading custom location data and adding it to the game.</summary>
        private CustomLocationManager CustomLocationLoader => this.ScreenManager.Value.CustomLocationManager;

        /// <summary>The loaded content packs.</summary>
        private readonly LoadedContentPack[] ContentPacks;

        /// <summary>Get the current token context for a given mod ID, or the global context if given a null mod ID.</summary>
        private readonly Func<string, IContext> GetContext;

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
        /// <param name="screenManager">Manages state for each screen.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="contentPacks">The loaded content packs.</param>
        /// <param name="getContext">Get the current token context.</param>
        /// <param name="updateContext">A callback which immediately updates the current condition context.</param>
        public CommandHandler(PerScreen<ScreenManager> screenManager, IMonitor monitor, LoadedContentPack[] contentPacks, Func<string, IContext> getContext, Action updateContext)
        {
            this.ScreenManager = screenManager;
            this.Monitor = monitor;
            this.ContentPacks = contentPacks;
            this.GetContext = getContext;
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

                case "dump":
                    return this.HandleDump(subcommandArgs);

                case "export":
                    return this.HandleExport(subcommandArgs);

                case "parse":
                    return this.HandleParse(subcommandArgs);

                case "reload":
                    return this.HandleReload(subcommandArgs);

                case "summary":
                    return this.HandleSummary(subcommandArgs);

                case "update":
                    return this.HandleUpdate();

                default:
                    this.Monitor.Log($"The '{this.CommandName} {args[0]}' command isn't valid. Type '{this.CommandName} help' for a list of valid commands.", LogLevel.Debug);
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
                ["dump"] = $"{this.CommandName} dump\n   Usage: {this.CommandName} dump order\n   Lists every loaded patch in their apply order. When two patches edit the same asset, they'll be applied in the apply order.",
                ["export"] = $"{this.CommandName} export\n   Usage: {this.CommandName} export \"<asset name>\"\n   Saves a copy of an asset (including any changes from mods like Content Patcher) to the game folder. The asset name should be the target without the locale or extension, like \"Characters/Abigail\" if you want to export the value of 'Content/Characters/Abigail.xnb'.",
                ["parse"] = $"{this.CommandName} parse\n   usage: {this.CommandName} parse \"value\"\n   Parses the given token string and shows the result. For example, `{this.CommandName} parse \"assets/{{{{Season}}}}.png\" will show a value like \"assets/Spring.png\".\n\n{this.CommandName} parse \"value\" \"content-pack.id\"\n   Parses the given token string and shows the result, using tokens available to the specified content pack (using the ID from the content pack's manifest.json). For example, `{this.CommandName} parse \"assets/{{{{CustomToken}}}}.png\" \"Pathoschild.ExampleContentPack\".",
                ["reload"] = $"{this.CommandName} reload\n   Usage: {this.CommandName} reload \"<content pack ID>\"\n   Reloads the patches of the content.json of a content pack. Config schema changes and dynamic token changes are unsupported.",
                ["summary"] = $"{this.CommandName} summary\n   Usage: {this.CommandName} summary\n   Shows a summary of the current conditions and loaded patches.\n\n  Usage: {this.CommandName} summary \"<content pack ID>\"\n Show a summary of the current conditions, and loaded patches for the given content pack.",
                ["update"] = $"{this.CommandName} update\n   Usage: {this.CommandName} update\n   Immediately refreshes the condition context and rechecks all patches."
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
                    help.AppendLine();
                }
            }
            else if (helpEntries.TryGetValue(args[0], out string entry))
                help.AppendLine(entry);
            else
                help.AppendLine($"Unknown command '{this.CommandName} {args[0]}'. Type '{this.CommandName} help' for available commands.");

            // write output
            this.Monitor.Log(help.ToString().Trim(), LogLevel.Debug);

            return true;
        }

        /// <summary>Handle the 'patch dump' command.</summary>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleDump(string[] args)
        {
            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch dump' command requires an argument which indicates what to dump. See 'patch help dump' for more info.", LogLevel.Error);
                return true;
            }

            switch (args[0]?.ToLower())
            {
                case "applied":
                    {
                        StringBuilder str = new();
                        str.AppendLine("Here are the active patches grouped by their current target value. Within each group, patches are listed in the expected apply order and the checkbox indicates whether each patch is currently applied. See `patch summary` for more info about each patch, including reasons it may not be applied.");

                        foreach (var group in this.PatchManager.GetPatchesByTarget().OrderBy(p => p.Key, HumanSortComparer.DefaultIgnoreCase))
                        {
                            str.AppendLine();
                            str.AppendLine(group.Key);
                            str.AppendLine("".PadRight(group.Key.Length, '-'));

                            var patches = group.Value
                                .OrderByDescending(p => p.Type == PatchType.Load)
                                .ThenBy(p => p, PatchIndexComparer.Instance);

                            foreach (var patch in patches)
                                str.AppendLine($"   [{(patch.IsApplied ? "X" : " ")}] {patch.Type} {patch.Path}");
                        }

                        this.Monitor.Log(str.ToString(), LogLevel.Info);
                    }
                    return true;

                case "order":
                    {
                        var patches = this.PatchManager.GetPatches()
                            .Select((patch, globalIndex) => new
                            {
                                globalPosition = (globalIndex + 1).ToString(CultureInfo.InvariantCulture),
                                indexPath = string.Join(" > ", patch.IndexPath),
                                name = patch.Path.ToString()
                            })
                            .ToArray();

                        int indexLen = Math.Max("order".Length, patches.Max(p => p.globalPosition.Length));
                        int pathLen = Math.Max("index path".Length, patches.Max(p => p.indexPath.Length));

                        StringBuilder str = new();
                        str.AppendLine("Here's the global patch definition order across all loaded content packs, which affects the order that patches are applied. The 'order' column is the patch's global position in the order; the 'index path' column is Content Patcher's internal hierarchical definition order.");
                        str.AppendLine();
                        str.AppendLine($"   {"order".PadRight(indexLen, ' ')}   {"index path".PadRight(pathLen, ' ')}   patch");
                        str.AppendLine($"   {"".PadRight(indexLen, '-')}   {"".PadRight(pathLen, '-')}   -----");

                        foreach (var patch in patches)
                            str.AppendLine($"   {patch.globalPosition.PadRight(indexLen, ' ')}   {patch.indexPath.PadRight(pathLen, ' ')}   {patch.name}");

                        this.Monitor.Log(str.ToString(), LogLevel.Info);
                    }
                    return true;

                default:
                    this.Monitor.Log("Invalid 'patch dump' argument. See 'patch help dump' for more info.", LogLevel.Error);
                    return true;
            }
        }

        /// <summary>Handle the 'patch export' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleExport(string[] args)
        {
            // get asset name
            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch export' command expects a single argument containing the target asset name. See 'patch help' for more info.", LogLevel.Error);
                return true;
            }
            string assetName = args[0];

            // load asset
            object asset;
            try
            {
                asset = Game1.content.Load<object>(assetName);
            }
            catch (ContentLoadException ex)
            {
                this.Monitor.Log($"Can't load asset '{assetName}': {ex.Message}", LogLevel.Error);
                return true;
            }

            // init export path
            string fullTargetPath = Path.Combine(StardewModdingAPI.Constants.ExecutionPath, "patch export", string.Join("_", assetName.Split(Path.GetInvalidFileNameChars())));
            Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath));

            // export
            if (asset is Texture2D texture)
            {
                fullTargetPath += ".png";

                texture = this.UnpremultiplyTransparency(texture);
                using (Stream stream = File.Create(fullTargetPath))
                    texture.SaveAsPng(stream, texture.Width, texture.Height);

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else if (this.IsDataAsset(asset))
            {
                fullTargetPath += ".json";

                File.WriteAllText(fullTargetPath, JsonConvert.SerializeObject(asset, Formatting.Indented));

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else
                this.Monitor.Log($"Can't export asset '{assetName}' of type {asset?.GetType().FullName ?? "null"}, expected image or data.", LogLevel.Error);

            return true;
        }

        /// <summary>Handle the 'patch parse' command.</summary>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleParse(string[] args)
        {
            // get token string
            if (args.Length < 1 || args.Length > 2)
            {
                this.Monitor.Log("The 'patch parse' command expects one to two arguments. See 'patch help parse' for more info.", LogLevel.Error);
                return true;
            }
            string raw = args[0];
            string modID = args.Length >= 2 ? args[1] : null;

            // get context
            IContext context;
            try
            {
                context = this.GetContext(modID);
            }
            catch (KeyNotFoundException ex)
            {
                this.Monitor.Log(ex.Message, LogLevel.Error);
                return true;
            }
            catch (Exception ex)
            {
                this.Monitor.Log(ex.ToString(), LogLevel.Error);
                return true;
            }

            // parse value
            TokenString tokenStr;
            try
            {
                tokenStr = new TokenString(raw, context, new LogPathBuilder("console command"));
            }
            catch (LexFormatException ex)
            {
                this.Monitor.Log($"Can't parse that token value: {ex.Message}", LogLevel.Error);
                return true;
            }
            catch (InvalidOperationException outerEx) when (outerEx.InnerException is LexFormatException ex)
            {
                this.Monitor.Log($"Can't parse that token value: {ex.Message}", LogLevel.Error);
                return true;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Can't parse that token value: {ex}", LogLevel.Error);
                return true;
            }
            IContextualState state = tokenStr.GetDiagnosticState();

            // show result
            StringBuilder output = new StringBuilder();
            output.AppendLine();
            output.AppendLine("Metadata");
            output.AppendLine("----------------");
            output.AppendLine($"   raw value:   {raw}");
            output.AppendLine($"   ready:       {tokenStr.IsReady}");
            output.AppendLine($"   mutable:     {tokenStr.IsMutable}");
            output.AppendLine($"   has tokens:  {tokenStr.HasAnyTokens}");
            if (tokenStr.HasAnyTokens)
                output.AppendLine($"   tokens used: {string.Join(", ", tokenStr.GetTokensUsed().Distinct().OrderBy(p => p, StringComparer.OrdinalIgnoreCase))}");
            output.AppendLine();

            output.AppendLine("Diagnostic state");
            output.AppendLine("----------------");
            output.AppendLine($"   valid:    {state.IsValid}");
            output.AppendLine($"   in scope: {state.IsValid}");
            output.AppendLine($"   ready:    {state.IsReady}");
            if (state.Errors.Any())
                output.AppendLine($"   errors:         {string.Join(", ", state.Errors)}");
            if (state.InvalidTokens.Any())
                output.AppendLine($"   invalid tokens: {string.Join(", ", state.InvalidTokens)}");
            if (state.UnreadyTokens.Any())
                output.AppendLine($"   unready tokens: {string.Join(", ", state.UnreadyTokens)}");
            if (state.UnavailableModTokens.Any())
                output.AppendLine($"   unavailable mod tokens: {string.Join(", ", state.UnavailableModTokens)}");
            output.AppendLine();

            output.AppendLine("Result");
            output.AppendLine("----------------");
            output.AppendLine(!tokenStr.IsReady
                ? "The token string is invalid or unready."
                : $"   The token string is valid and ready. Parsed value: \"{tokenStr}\""
            );

            this.Monitor.Log(output.ToString(), LogLevel.Debug);
            return true;
        }

        /// <summary>Handle the 'patch reload' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleReload(string[] args)
        {
            // get pack ID
            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch reload' command expects a single arguments containing the target content pack ID. See 'patch help' for more info.", LogLevel.Error);
                return true;
            }
            string packId = args[0];

            // get pack
            RawContentPack pack = this.ContentPacks.SingleOrDefault(p => p.Manifest.UniqueID == packId);
            if (pack == null)
            {
                this.Monitor.Log($"No Content Patcher content pack with the unique ID \"{packId}\".", LogLevel.Error);
                return true;
            }

            // unload patches
            this.PatchLoader.UnloadPatchesLoadedBy(pack, false);

            // load pack patches
            if (!pack.TryReloadContent(out string loadContentError))
            {
                this.Monitor.Log($"Failed to reload content pack '{pack.Manifest.Name}' for configuration changes: {loadContentError}. The content pack may not be in a valid state.", LogLevel.Error); // should never happen
                return true;
            }

            // reload patches
            this.PatchLoader.LoadPatches(
                contentPack: pack,
                rawPatches: pack.Content.Changes,
                rootIndexPath: new[] { pack.Index },
                path: new LogPathBuilder(pack.Manifest.Name),
                reindex: true,
                parentPatch: null
            );

            // make the changes apply
            this.UpdateContext();

            this.Monitor.Log("Content pack reloaded.", LogLevel.Info);
            return true;
        }

        /// <summary>Handle the 'patch summary' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleSummary(string[] args)
        {
            StringBuilder output = new StringBuilder();
            LogPathBuilder path = new LogPathBuilder("console command");

            // parse arguments
            bool showFull = false;
            var forModIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string arg in args)
            {
                // flags
                if (arg.Equals("full", StringComparison.OrdinalIgnoreCase))
                {
                    showFull = true;
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
                        from token in this.TokenManager.GetTokens(enforceContext: false)
                        let inputArgs = token.GetAllowedInputArguments().ToArray()
                        let rootValues = !token.RequiresInput ? token.GetValues(InputArguments.Empty).ToArray() : new string[0]
                        let isMultiValue =
                            inputArgs.Length > 1
                            || rootValues.Length > 1
                            || (inputArgs.Length == 1 && token.GetValues(new InputArguments(new LiteralString(inputArgs[0], path.With(token.Name, "input")))).Count() > 1)
                        let mod = (token as ModProvidedToken)?.Mod
                        orderby isMultiValue, token.Name // single-value tokens first, then alphabetically
                        select new { Mod = mod, Token = token }
                    )
                    .GroupBy(p => p.Mod?.Name?.Trim())
                    .OrderBy(p => p.Key) // default tokens (key is null), then tokens added by other mods
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
                                foreach (string input in allowedInputs.OrderByIgnoreCase(input => input))
                                {
                                    if (isFirst)
                                    {
                                        output.Append("[X] ");
                                        isFirst = false;
                                    }
                                    else
                                        output.Append($"      {"".PadRight(labelWidth, ' ')} |     ");

                                    output.AppendLine($":{input}: {GetTruncatedTokenValues(token.GetValues(new InputArguments(new LiteralString(input, path.With(token.Name, "input")))))}");
                                }
                            }
                            else
                                output.AppendLine("[X] (token returns a dynamic value)");
                        }
                        else
                            output.AppendLine("[X] " + GetTruncatedTokenValues(token.GetValues(InputArguments.Empty).OrderByIgnoreCase(p => p)));
                    }

                    output.AppendLine();
                }
            }

            // list custom locations
            {
                var locations = this.CustomLocationLoader.GetCustomLocationData()
                    .Where(p => !forModIds.Any() || forModIds.Contains(p.ContentPack.Manifest.UniqueID))
                    .GroupByIgnoreCase(p => p.ContentPack.Manifest.Name)
                    .OrderByIgnoreCase(p => p.Key)
                    .ToArray();

                if (locations.Any())
                {
                    output.AppendLine(
                        "======================\n"
                        + "== Custom locations ==\n"
                        + "======================\n"
                        + "The following custom locations were created by content packs.\n"
                        + (forModIds.Any() ? $"\n(Filtered to content pack ID{(forModIds.Count > 1 ? "s" : "")}: {string.Join(", ", forModIds.OrderByIgnoreCase(p => p))}.)\n" : "")
                    );

                    foreach (var locationGroup in locations)
                    {
                        int nameWidth = Math.Max("location name".Length, locationGroup.Max(p => p.Name.Length));

                        output.AppendLine($"{locationGroup.Key}:");
                        output.AppendLine("".PadRight(locationGroup.Key.Length + 1, '-'));
                        output.AppendLine();
                        output.AppendLine($"   {"location name".PadRight(nameWidth)}  | status");
                        output.AppendLine($"   {"".PadRight(nameWidth, '-')}  | ------");
                        foreach (CustomLocationData location in locationGroup.OrderByIgnoreCase(p => p.Name))
                            output.AppendLine($"   {location.Name.PadRight(nameWidth)}  | {(location.IsEnabled ? "[X] ok" : $"[ ] error: {location.Error}")}");
                        output.AppendLine();
                    }
                }

                output.AppendLine();
            }


            // list patches
            {
                var patches = this.GetAllPatches()
                    .Where(p => !forModIds.Any() || forModIds.Contains(p.ContentPack.Manifest.UniqueID))
                    .GroupByIgnoreCase(p => p.ContentPack.Manifest.Name)
                    .OrderByIgnoreCase(p => p.Key)
                    .ToArray();

                output.AppendLine(
                    "=====================\n"
                    + "== Content patches ==\n"
                    + "=====================\n"
                    + "The following patches were loaded. For each patch:\n"
                    + "  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).\n"
                    + "  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.\n"
                    + "  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.\n"
                    + (forModIds.Any() ? $"\n(Filtered to content pack ID{(forModIds.Count > 1 ? "s" : "")}: {string.Join(", ", forModIds.OrderByIgnoreCase(p => p))}.)\n" : "")
                    + "\n"
                );
                foreach (var patchGroup in patches)
                {
                    ModTokenContext tokenContext = this.TokenManager.TrackLocalTokens(patchGroup.First().ContentPack);
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
                                    Values = token.IsReady ? token.GetValues(new InputArguments(input)).ToArray() : new string[0],
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
                            string rawIdentifyingPath = PathUtilities.NormalizePath(patch.ParsedType == PatchType.Include
                                ? patch.RawFromAsset
                                : patch.RawTargetAsset
                            );
                            ITokenString parsedIdentifyingPath = patch.ParsedType == PatchType.Include
                                ? patch.ParsedFromAsset
                                : patch.ParsedTargetAsset;

                            // get raw name if different
                            // (ignore differences in whitespace, capitalization, and path separators)
                            string rawValue = !PathUtilities.NormalizePath(patch.PathWithoutContentPackPrefix.ToString().Replace(" ", "")).ContainsIgnoreCase(rawIdentifyingPath?.Replace(" ", ""))
                                ? $"{patch.ParsedType?.ToString() ?? patch.RawType} {rawIdentifyingPath}"
                                : null;

                            // get parsed value
                            string parsedValue = patch.MatchesContext && parsedIdentifyingPath?.HasAnyTokens == true
                                ? PathUtilities.NormalizePath(parsedIdentifyingPath.Value)
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
                        string errorReason = this.GetReasonNotLoaded(patch);
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
                            foreach (var pair in this.TokenManager.TokensWithSpecialUpdateRates)
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

                            foreach (var pair in effectsByPatch.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                                output.AppendLine($"      {pair.Key}{"".PadRight(maxAssetNameWidth - pair.Key.Length)} | {string.Join("; ", pair.Value.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))}");
                        }
                        else
                            output.AppendLine("   No current changes.");
                    }

                    // add blank line between groups
                    output.AppendLine();
                }
            }

            this.Monitor.Log(output.ToString(), LogLevel.Debug);
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

            IContextualState state = patch.State;

            // state error
            if (state.InvalidTokens.Any())
                return $"invalid tokens: {string.Join(", ", state.InvalidTokens.OrderByIgnoreCase(p => p))}";
            if (state.UnreadyTokens.Any())
                return $"tokens not ready: {string.Join(", ", state.UnreadyTokens.OrderByIgnoreCase(p => p))}";
            if (state.Errors.Any())
                return string.Join("; ", state.Errors);

            // conditions not matched
            if (!patch.MatchesContext && patch.ParsedConditions != null)
            {
                string[] failedConditions = (
                    from condition in patch.ParsedConditions
                    let displayText = !condition.Is(ConditionType.HasFile) && !string.IsNullOrWhiteSpace(condition.Input.TokenString?.Raw)
                        ? $"{condition.Name}:{condition.Input.TokenString.Raw}"
                        : condition.Name
                    orderby displayText
                    where !condition.IsMatch
                    select $"{displayText}"
                ).ToArray();

                if (failedConditions.Any())
                    return $"conditions don't match: {string.Join(", ", failedConditions)}";
            }

            // fallback to unavailable tokens (should never happen due to HasMod check)
            if (state.UnavailableModTokens.Any())
                return $"tokens provided by an unavailable mod: {string.Join(", ", state.UnavailableModTokens.OrderByIgnoreCase(p => p))}";

            // non-matching for an unknown reason
            if (!patch.MatchesContext)
                return "doesn't match context (unknown reason)";

            // seems fine, just not applied yet
            return null;
        }

        /// <summary>Reverse premultiplication applied to an image asset by the XNA content pipeline.</summary>
        /// <param name="texture">The texture to adjust.</param>
        private Texture2D UnpremultiplyTransparency(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                Color pixel = data[i];
                if (pixel.A == 0)
                    continue;

                data[i] = new Color(
                    (byte)((pixel.R * 255) / pixel.A),
                    (byte)((pixel.G * 255) / pixel.A),
                    (byte)((pixel.B * 255) / pixel.A),
                    pixel.A
                ); // don't use named parameters, which are inconsistent between MonoGame (e.g. 'alpha') and XNA (e.g. 'a')
            }

            Texture2D result = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            result.SetData(data);
            return result;
        }

        /// <summary>Get whether an asset can be saved to JSON.</summary>
        /// <param name="asset">The asset to check.</param>
        private bool IsDataAsset(object asset)
        {
            if (asset == null)
                return false;

            Type type = asset.GetType();
            type = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            return type == typeof(Dictionary<,>) || type == typeof(List<>);
        }
    }
}
