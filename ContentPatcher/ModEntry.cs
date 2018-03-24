using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the file which contains patch metadata.</summary>
        private readonly string PatchFileName = "content.json";

        /// <summary>The name of the file which contains player settings.</summary>
        private readonly string ConfigFileName = "config.json";

        /// <summary>The supported format versions.</summary>
        private readonly string[] SupportedFormatVersions = { "1.0", "1.3" };

        /// <summary>Handles loading assets from content packs.</summary>
        private readonly AssetLoader AssetLoader = new AssetLoader();

        /// <summary>Handles constructing, permuting, and updating conditions.</summary>
        private readonly ConditionFactory ConditionFactory = new ConditionFactory();

        /// <summary>Handles the 'patch' console command.</summary>
        private CommandHandler CommandHandler;

        /// <summary>Manages loaded patches.</summary>
        private PatchManager PatchManager;

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The debug overlay (if enabled).</summary>
        private DebugOverlay DebugOverlay;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // init
            this.Config = helper.ReadConfig<ModConfig>();
            this.PatchManager = new PatchManager(this.Monitor, this.ConditionFactory, helper.Content.CurrentLocaleConstant, this.Config.VerboseLog, helper.Content.NormaliseAssetName);
            this.LoadContentPacks();

            // register patcher
            helper.Content.AssetLoaders.Add(this.PatchManager);
            helper.Content.AssetEditors.Add(this.PatchManager);
            this.PatchManager.UpdateContext(this.Helper.Content, this.Helper.Content.CurrentLocaleConstant, null, null);

            // set up events
            if (this.Config.EnableDebugFeatures)
                InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            SaveEvents.AfterReturnToTitle += this.SaveEvents_AfterReturnToTitle;
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;

            // set up commands
            this.CommandHandler = new CommandHandler(this.PatchManager, this.ConditionFactory, this.Monitor);
            helper.ConsoleCommands.Add(this.CommandHandler.CommandName, $"Starts a Content Patcher command. Type '{this.CommandHandler.CommandName} help' for details.", (name, args) => this.CommandHandler.Handle(args));
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (this.Config.EnableDebugFeatures)
            {
                // toggle overlay
                if (this.Config.Controls.ToggleDebug.Contains(e.Button))
                {
                    if (this.DebugOverlay == null)
                        this.DebugOverlay = new DebugOverlay(this.Helper.Content);
                    else
                    {
                        this.DebugOverlay.Dispose();
                        this.DebugOverlay = null;
                    }
                    return;
                }

                // cycle textures
                if (this.DebugOverlay != null)
                {
                    if (this.Config.Controls.DebugPrevTexture.Contains(e.Button))
                        this.DebugOverlay.PrevTexture();
                    if (this.Config.Controls.DebugNextTexture.Contains(e.Button))
                        this.DebugOverlay.NextTexture();
                }
            }
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            // get context
            IContentHelper contentHelper = this.Helper.Content;
            LocalizedContentManager.LanguageCode language = contentHelper.CurrentLocaleConstant;

            // update context
            this.VerboseLog($"Context: date=none, weather=none, locale={language}.");
            this.PatchManager.UpdateContext(this.Helper.Content, this.Helper.Content.CurrentLocaleConstant, null, null);
        }

        /// <summary>The method invoked when a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // get context
            IContentHelper contentHelper = this.Helper.Content;
            LocalizedContentManager.LanguageCode language = contentHelper.CurrentLocaleConstant;
            SDate date = SDate.Now();
            Weather weather = this.GetCurrentWeather();

            // update context
            this.VerboseLog($"Context: date={date.DayOfWeek} {date.Season} {date.Day}, weather={weather}, locale={language}.");
            this.PatchManager.UpdateContext(contentHelper, language, date, weather);
        }

        /****
        ** Methods
        ****/
        /// <summary>Load the patches from all registered content packs.</summary>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private void LoadContentPacks()
        {
            ConfigFileHandler configFileHandler = new ConfigFileHandler(this.ConfigFileName, this.PatchManager.ParseCommaDelimitedField, (pack, label, reason) => this.Monitor.Log($"Ignored {pack.Manifest.Name} > {label}: {reason}"));
            foreach (IContentPack pack in this.Helper.GetContentPacks())
            {
                this.VerboseLog($"Loading content pack '{pack.Manifest.Name}'...");

                try
                {
                    // read changes file
                    ContentConfig content = pack.ReadJsonFile<ContentConfig>(this.PatchFileName);
                    if (content == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it has no {this.PatchFileName} file.", LogLevel.Error);
                        continue;
                    }
                    if (content.Format == null || content.Changes == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it doesn't specify the required {nameof(ContentConfig.Format)} or {nameof(ContentConfig.Changes)} fields.", LogLevel.Error);
                        continue;
                    }

                    // validate version
                    if (!this.SupportedFormatVersions.Contains(content.Format.ToString()))
                    {
                        this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it uses unsupported format {content.Format} (supported version: {string.Join(", ", this.SupportedFormatVersions)}).", LogLevel.Error);
                        continue;
                    }

                    // load config.json
                    InvariantDictionary<ConfigField> config = configFileHandler.Read(pack, content.ConfigSchema);
                    configFileHandler.Save(pack, config, this.Helper);
                    if (config.Any())
                        this.VerboseLog($"   found config.json with {config.Count} fields...");

                    // validate features
                    if (content.Format.IsOlderThan("1.3"))
                    {
                        if (config.Any())
                        {
                            this.Monitor.Log($"Loading content pack '{pack.Manifest.Name}' failed. It specifies format version {content.Format}, but uses the {nameof(ContentConfig.ConfigSchema)} field added in 1.3.", LogLevel.Error);
                            continue;
                        }
                        if (content.Changes.Any(p => p.FromFile != null && p.FromFile.Contains("{{")))
                        {
                            this.Monitor.Log($"Loading content pack '{pack.Manifest.Name}' failed. It specifies format version {content.Format}, but uses the {{{{token}}}} feature added in 1.3.", LogLevel.Error);
                            continue;
                        }
                        if (content.Changes.Any(p => p.When != null && p.When.Any()))
                        {
                            this.Monitor.Log($"Loading content pack '{pack.Manifest.Name}' failed. It specifies format version {content.Format}, but uses the condition feature ({nameof(ContentConfig.Changes)}.{nameof(PatchConfig.When)} field) added in 1.3.", LogLevel.Error);
                            continue;
                        }
                    }

                    // load patches
                    this.NamePatches(pack, content.Changes);
                    foreach (PatchConfig patch in content.Changes)
                    {
                        this.VerboseLog($"   loading {patch.LogName}...");
                        this.LoadPatch(pack, patch, config, logSkip: reasonPhrase => this.Monitor.Log($"Ignored {patch.LogName}: {reasonPhrase}", LogLevel.Warn));
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading content pack '{pack.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                }
            }
        }

        /// <summary>Set a unique name for all patches in a content pack.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="patches">The patches to name.</param>
        private void NamePatches(IContentPack contentPack, PatchConfig[] patches)
        {
            // add default log names
            foreach (PatchConfig patch in patches)
            {
                if (string.IsNullOrWhiteSpace(patch.LogName))
                    patch.LogName = $"{patch.Action} {patch.Target}";
            }

            // detect duplicate names
            InvariantHashSet duplicateNames = new InvariantHashSet(
                from patch in patches
                group patch by patch.LogName into nameGroup
                where nameGroup.Count() > 1
                select nameGroup.Key
            );

            // make names unique
            int i = 0;
            foreach (PatchConfig patch in patches)
            {
                i++;

                if (duplicateNames.Contains(patch.LogName))
                    patch.LogName = $"entry #{i} ({patch.LogName})";

                patch.LogName = $"{contentPack.Manifest.Name} > {patch.LogName}";
            }
        }

        /// <summary>Load one patch from a content pack's <c>content.json</c> file.</summary>
        /// <param name="pack">The content pack being loaded.</param>
        /// <param name="entry">The change to load.</param>
        /// <param name="config">The content pack's config values.</param>
        /// <param name="logSkip">The callback to invoke with the error reason if loading it fails.</param>
        private bool LoadPatch(IContentPack pack, PatchConfig entry, InvariantDictionary<ConfigField> config, Action<string> logSkip)
        {
            bool TrackSkip(string reason, bool warn = true)
            {
                this.PatchManager.AddPermanentlyDisabled(new DisabledPatch(entry.LogName, entry.Action, entry.Target, pack, reason));
                if (warn)
                    logSkip(reason);
                return false;
            }

            try
            {
                // normalise patch fields
                if (entry.When == null)
                    entry.When = new InvariantDictionary<string>();

                // parse action
                if (!Enum.TryParse(entry.Action, true, out PatchType action))
                {
                    return TrackSkip(string.IsNullOrWhiteSpace(entry.Action)
                        ? $"must set the {nameof(PatchConfig.Action)} field."
                        : $"invalid {nameof(PatchConfig.Action)} value '{entry.Action}', expected one of: {string.Join(", ", Enum.GetNames(typeof(PatchType)))}."
                    );
                }

                // parse target asset
                TokenString assetName;
                {
                    if (string.IsNullOrWhiteSpace(entry.Target))
                        return TrackSkip($"must set the {nameof(PatchConfig.Target)} field.");
                    if (!this.TryParseTokenString(entry.Target, config, out string error, out TokenStringBuilder builder))
                        return TrackSkip($"the {nameof(PatchConfig.Target)} is invalid: {error}");
                    assetName = builder.Build();
                }

                // parse 'enabled'
                bool enabled = true;
                {
                    if (entry.Enabled != null && !this.TryParseBoolean(entry.Enabled, config, out string error, out enabled))
                        return TrackSkip($"invalid {nameof(PatchConfig.Enabled)} value '{entry.Enabled}': {error}");
                }

                // apply config
                foreach (string key in config.Keys)
                {
                    if (entry.When.TryGetValue(key, out string values))
                    {
                        InvariantHashSet expected = this.PatchManager.ParseCommaDelimitedField(values);
                        if (!expected.Intersect(config[key].Value).Any())
                            return TrackSkip($"disabled: config '{key}' value '{string.Join(", ", config[key].Value)}' doesn't condition '{string.Join(", ", expected)}'", warn: false);

                        entry.When.Remove(key);
                    }
                }

                // parse conditions
                ConditionDictionary conditions;
                {
                    if (!this.PatchManager.TryParseConditions(entry.When, out conditions, out string error))
                        return TrackSkip($"the {nameof(PatchConfig.When)} field is invalid: {error}.");
                }

                // get patch instance
                IPatch patch;
                switch (action)
                {
                    // load asset
                    case PatchType.Load:
                        {
                            // init patch
                            if (!this.TryPrepareLocalAsset(pack, entry.FromFile, config, conditions, out string error, out TokenString fromAsset, checkOnly: !enabled))
                                return TrackSkip(error);
                            patch = new LoadPatch(entry.LogName, this.AssetLoader, pack, assetName, conditions, fromAsset, this.Helper.Content.NormaliseAssetName);

                            // detect conflicting loaders
                            InvariantDictionary<IPatch> conflicts = this.PatchManager.GetConflictingLoaders(patch);
                            if (conflicts.Any())
                            {
                                IEnumerable<string> conflictNames = (from conflict in conflicts orderby conflict.Key select $"'{conflict.Value.LogName}' already loads {conflict.Key}");
                                return TrackSkip($"{nameof(entry.Target)} '{patch.TokenableAssetName.Raw}' conflicts with other load patches ({string.Join(", ", conflictNames)}). Each file can only be loaded by one patch, unless their conditions can never overlap.");
                            }
                        }
                        break;

                    // edit data
                    case PatchType.EditData:
                        {
                            // validate
                            if (entry.Entries == null && entry.Fields == null)
                                return TrackSkip($"either {nameof(PatchConfig.Entries)} or {nameof(PatchConfig.Fields)} must be specified for a '{action}' change.");
                            if (entry.Entries != null && entry.Entries.Any(p => string.IsNullOrWhiteSpace(p.Value)))
                                return TrackSkip($"the {nameof(PatchConfig.Entries)} can't contain empty values.");
                            if (entry.Fields != null && entry.Fields.Any(p => p.Value == null || p.Value.Any(n => n.Value == null)))
                                return TrackSkip($"the {nameof(PatchConfig.Fields)} can't contain empty values.");

                            // save
                            patch = new EditDataPatch(entry.LogName, this.AssetLoader, pack, assetName, conditions, entry.Entries, entry.Fields, this.Monitor, this.Helper.Content.NormaliseAssetName);
                        }
                        break;

                    // edit image
                    case PatchType.EditImage:
                        {
                            // read patch mode
                            PatchMode patchMode = PatchMode.Replace;
                            if (!string.IsNullOrWhiteSpace(entry.PatchMode) && !Enum.TryParse(entry.PatchMode, true, out patchMode))
                                return TrackSkip($"the {nameof(PatchConfig.PatchMode)} is invalid. Expected one of these values: [{string.Join(", ", Enum.GetNames(typeof(PatchMode)))}].");

                            // save
                            if (!this.TryPrepareLocalAsset(pack, entry.FromFile, config, conditions, out string error, out TokenString fromAsset, checkOnly: !enabled))
                                return TrackSkip(error);
                            patch = new EditImagePatch(entry.LogName, this.AssetLoader, pack, assetName, conditions, fromAsset, entry.FromArea, entry.ToArea, patchMode, this.Monitor, this.Helper.Content.NormaliseAssetName);
                        }
                        break;

                    default:
                        return TrackSkip($"unsupported patch type '{action}'.");
                }

                // only apply patch when its tokens are available
                HashSet<ConditionKey> tokensUsed = new HashSet<ConditionKey>(patch.GetTokensUsed());
                foreach (ConditionKey key in tokensUsed)
                {
                    if (!patch.Conditions.ContainsKey(key))
                        patch.Conditions.Add(key, patch.Conditions.GetValidValues(key));
                }

                // skip if not enabled
                // note: we process the patch even if it's disabled, so any errors are caught by the modder instead of only failing after the patch is enabled.
                if (!enabled)
                    return TrackSkip($"{nameof(PatchConfig.Enabled)} is false.");

                // save patch
                this.PatchManager.Add(patch);
                return true;
            }
            catch (Exception ex)
            {
                return TrackSkip($"error reading info. Technical details:\n{ex}");
            }
        }

        /// <summary>Parse a boolean value from a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="config">The player configuration.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseBoolean(string rawValue, InvariantDictionary<ConfigField> config, out string error, out bool parsed)
        {
            parsed = false;

            // parse tokens
            if (!this.TryParseTokenString(rawValue, config, out error, out TokenStringBuilder builder))
                return false;

            // validate tokens
            if (builder.HasAnyTokens)
            {
                // validate: no condition tokens allowed
                if (builder.ConditionTokens.Any())
                {
                    error = $"can't use condition tokens in a boolean field ({string.Join(", ", builder.ConditionTokens.OrderBy(p => p))}).";
                    return false;
                }

                // validate config tokens
                if (builder.ConfigTokens.Any())
                {
                    // max one tokem
                    if (builder.ConfigTokens.Count > 1)
                    {
                        error = "can't use multiple tokens.";
                        return false;
                    }

                    // field isn't boolean
                    string key = builder.ConfigTokens.First();
                    ConfigField field = config[key];
                    if (field.AllowValues.Except(new[] { "true", "false" }, StringComparer.InvariantCultureIgnoreCase).Any())
                    {
                        error = $"can't use {{{{{key}}}}} because that config field isn't restricted to 'true' or 'false'.";
                        return false;
                    }
                }
            }

            // parse value
            TokenString tokenString = builder.Build();
            if (!bool.TryParse(tokenString.Raw, out parsed))
            {
                error = $"can't parse {tokenString.Raw} as a true/false value.";
                return false;
            }
            return true;
        }

        /// <summary>Parse a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="config">The player configuration.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseTokenString(string rawValue, InvariantDictionary<ConfigField> config, out string error, out TokenStringBuilder parsed)
        {
            // parse
            TokenStringBuilder builder = new TokenStringBuilder(rawValue, config);

            // validate unknown tokens
            if (builder.InvalidTokens.Any())
            {
                parsed = null;
                error = $"found unknown tokens: {string.Join(", ", builder.InvalidTokens.OrderBy(p => p))}";
                return false;
            }

            // validate config tokens
            foreach (string key in builder.ConfigTokens)
            {
                ConfigField field = config[key];
                if (field.AllowMultiple)
                {
                    parsed = null;
                    error = $"token {{{{{key}}}}} can't be used because that config field allows multiple values.";
                    return false;
                }
            }

            // looks OK
            parsed = builder;
            error = null;
            return true;
        }


        /// <summary>Prepare a local asset file for a patch to use.</summary>
        /// <param name="pack">The content pack being loaded.</param>
        /// <param name="path">The asset path in the content patch.</param>
        /// <param name="config">The config values to apply.</param>
        /// <param name="conditions">The conditions to apply.</param>
        /// <param name="error">The error reason if preparing the asset fails.</param>
        /// <param name="tokenedPath">The parsed value.</param>
        /// <param name="checkOnly">Prepare the asset info and check if it's valid, but don't actually preload the asset.</param>
        /// <returns>Returns whether the local asset was successfully prepared.</returns>
        private bool TryPrepareLocalAsset(IContentPack pack, string path, InvariantDictionary<ConfigField> config, ConditionDictionary conditions, out string error, out TokenString tokenedPath, bool checkOnly)
        {
            // normalise raw value
            path = this.NormaliseLocalAssetPath(pack, path);
            if (path == null)
            {
                error = $"must set the {nameof(PatchConfig.FromFile)} field for this action type.";
                tokenedPath = null;
                return false;
            }

            // tokenise
            if (!this.TryParseTokenString(path, config, out string tokenError, out TokenStringBuilder builder))
            {
                error = $"the {nameof(PatchConfig.FromFile)} is invalid: {tokenError}";
                tokenedPath = null;
                return false;
            }
            tokenedPath = builder.Build();

            // validate all possible files exist
            // + preload PNG assets to avoid load-in-draw-loop error
            InvariantHashSet missingFiles = new InvariantHashSet();
            foreach (string localKey in this.ConditionFactory.GetPossibleStrings(tokenedPath, conditions))
            {
                // check-only mode
                if (checkOnly)
                {
                    if (!this.AssetLoader.FileExists(pack, localKey))
                        missingFiles.Add(localKey);
                    continue;
                }

                // else preload
                try
                {
                    if (this.AssetLoader.PreloadIfNeeded(pack, localKey))
                        this.VerboseLog($"      preloaded {localKey}.");
                }
                catch (FileNotFoundException)
                {
                    missingFiles.Add(localKey);
                }
            }
            if (missingFiles.Any())
            {
                error = tokenedPath.ConditionTokens.Any() || missingFiles.Count > 1
                    ? $"{nameof(PatchConfig.FromFile)} '{path}' matches files which don't exist ({string.Join(", ", missingFiles.OrderBy(p => p))})."
                    : $"{nameof(PatchConfig.FromFile)} '{path}' matches a file which doesn't exist.";
                tokenedPath = null;
                return false;
            }

            // looks OK
            error = null;
            return true;
        }

        /// <summary>Get a normalised file path relative to the content pack folder.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="path">The relative asset path.</param>
        private string NormaliseLocalAssetPath(IContentPack contentPack, string path)
        {
            // normalise asset name
            if (string.IsNullOrWhiteSpace(path))
                return null;
            string newPath = this.Helper.Content.NormaliseAssetName(path);

            // add .xnb extension if needed (it's stripped from asset names)
            string fullPath = Path.Combine(contentPack.DirectoryPath, newPath);
            if (!File.Exists(fullPath))
            {
                if (File.Exists($"{fullPath}.xnb") || Path.GetExtension(path) == ".xnb")
                    newPath += ".xnb";
            }

            return newPath;
        }

        /// <summary>Get the current weather from the game state.</summary>
        private Weather GetCurrentWeather()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || Game1.weddingToday)
                return Weather.Sun;

            if (Game1.isSnowing)
                return Weather.Snow;
            if (Game1.isRaining)
                return Game1.isLightning ? Weather.Storm : Weather.Rain;

            return Weather.Sun;
        }

        /// <summary>Log a message if <see cref="ModConfig.VerboseLog"/> is enabled.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log level.</param>
        private void VerboseLog(string message, LogLevel level = LogLevel.Trace)
        {
            if (this.Config.VerboseLog)
                this.Monitor.Log(message, level);
        }
    }
}
