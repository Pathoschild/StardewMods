using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using ContentPatcher.Framework.Validators;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;

[assembly: InternalsVisibleTo("Pathoschild.Stardew.Tests.Mods")]
namespace ContentPatcher
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The name of the file which contains patch metadata.</summary>
        private readonly string PatchFileName = "content.json";

        /// <summary>The name of the file which contains player settings.</summary>
        private readonly string ConfigFileName = "config.json";

        /// <summary>The supported format versions.</summary>
        private readonly string[] SupportedFormatVersions = { "1.0", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8" };

        /// <summary>The format version migrations to apply.</summary>
        private readonly Func<IMigration[]> Migrations = () => new IMigration[]
        {
            new Migration_1_3(),
            new Migration_1_4(),
            new Migration_1_5(),
            new Migration_1_6(),
            new Migration_1_7(),
            new Migration_1_8(),
        };

        /// <summary>The special validation logic to apply to assets affected by patches.</summary>
        private readonly Func<IAssetValidator[]> AssetValidators = () => new IAssetValidator[]
        {
            new StardewValley_1_3_36_Validator()
        };

        /// <summary>Manages the available contextual tokens.</summary>
        private TokenManager TokenManager;

        /// <summary>Manages loaded patches.</summary>
        private PatchManager PatchManager;

        /// <summary>Handles the 'patch' console command.</summary>
        private CommandHandler CommandHandler;

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
            this.Config = helper.ReadConfig<ModConfig>();

            // init migrations
            IMigration[] migrations = this.Migrations();

            // fetch content packs
            RawContentPack[] contentPacks = this.GetContentPacks(migrations).ToArray();
            string[] installedMods =
                (contentPacks.Select(p => p.Manifest.UniqueID))
                .Concat(helper.ModRegistry.GetAll().Select(p => p.Manifest.UniqueID))
                .OrderByIgnoreCase(p => p)
                .ToArray();

            // load content packs and context
            this.TokenManager = new TokenManager(helper.Content, installedMods);
            this.PatchManager = new PatchManager(this.Monitor, this.TokenManager, this.AssetValidators());
            this.LoadContentPacks(contentPacks);
            this.TokenManager.UpdateContext();

            // register patcher
            helper.Content.AssetLoaders.Add(this.PatchManager);
            helper.Content.AssetEditors.Add(this.PatchManager);

            // set up events
            if (this.Config.EnableDebugFeatures)
                helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Specialised.LoadStageChanged += this.OnLoadStageChanged;

            // set up commands
            this.CommandHandler = new CommandHandler(this.TokenManager, this.PatchManager, this.Monitor, this.UpdateContext);
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
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.Config.EnableDebugFeatures)
            {
                // toggle overlay
                if (this.Config.Controls.ToggleDebug.Contains(e.Button))
                {
                    if (this.DebugOverlay == null)
                        this.DebugOverlay = new DebugOverlay(this.Helper.Events, this.Helper.Input, this.Helper.Content);
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

        /// <summary>Raised when the low-level stage in the game's loading process has changed. This is an advanced event for mods which need to run code at specific points in the loading process. The available stages or when they happen might change without warning in future versions (e.g. due to changes in the game's load process), so mods using this event are more likely to break or have bugs.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            switch (e.NewStage)
            {
                case LoadStage.CreatedBasicInfo:
                case LoadStage.SaveLoadedBasicInfo:
                    this.Monitor.VerboseLog($"Updating context: load stage changed to {e.NewStage}.");
                    this.TokenManager.IsBasicInfoLoaded = true;
                    this.UpdateContext();
                    break;
            }
        }

        /// <summary>The method invoked when a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: new day started.");
            this.TokenManager.IsBasicInfoLoaded = true;
            this.UpdateContext();
        }

        /// <summary>The method invoked when the player warps.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: player warped.");
            this.UpdateSpecificContext(new InvariantHashSet() { ConditionType.LocationName.ToString(), ConditionType.IsOutdoors.ToString() } );
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: returned to title.");
            this.TokenManager.IsBasicInfoLoaded = false;
            this.UpdateContext();
        }

        /****
        ** Methods
        ****/
        /// <summary>Update the current context.</summary>
        private void UpdateContext()
        {
            this.TokenManager.UpdateContext();
            this.PatchManager.UpdateContext(this.Helper.Content);
        }

        /// <summary>Update the current context for specific tokens.</summary>
        private void UpdateSpecificContext(InvariantHashSet tokens)
        {
            this.TokenManager.UpdateSpecificContext(tokens);
            this.PatchManager.UpdateSpecificContext(this.Helper.Content, tokens);
        }

        /// <summary>Load the registered content packs.</summary>
        /// <param name="migrations">The format version migrations to apply.</param>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private IEnumerable<RawContentPack> GetContentPacks(IMigration[] migrations)
        {
            this.Monitor.VerboseLog("Preloading content packs...");

            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                RawContentPack rawContentPack;
                try
                {
                    // validate content.json has required fields
                    ContentConfig content = contentPack.ReadJsonFile<ContentConfig>(this.PatchFileName);
                    if (content == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{contentPack.Manifest.Name}' because it has no {this.PatchFileName} file.", LogLevel.Error);
                        continue;
                    }
                    if (content.Format == null || content.Changes == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{contentPack.Manifest.Name}' because it doesn't specify the required {nameof(ContentConfig.Format)} or {nameof(ContentConfig.Changes)} fields.", LogLevel.Error);
                        continue;
                    }

                    // apply migrations
                    IMigration migrator = new AggregateMigration(content.Format, this.SupportedFormatVersions, migrations);
                    if (!migrator.TryMigrate(content, out string error))
                    {
                        this.Monitor.Log($"Loading content pack '{contentPack.Manifest.Name}' failed: {error}.", LogLevel.Error);
                        continue;
                    }

                    // init
                    rawContentPack = new RawContentPack(new ManagedContentPack(contentPack), content, migrator);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error preloading content pack '{contentPack.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }

                yield return rawContentPack;
            }
        }

        /// <summary>Load the patches from all registered content packs.</summary>
        /// <param name="contentPacks">The content packs to load.</param>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private void LoadContentPacks(IEnumerable<RawContentPack> contentPacks)
        {
            // load content packs
            ConfigFileHandler configFileHandler = new ConfigFileHandler(this.ConfigFileName, this.ParseCommaDelimitedField, (pack, label, reason) => this.Monitor.Log($"Ignored {pack.Manifest.Name} > {label}: {reason}", LogLevel.Warn));
            foreach (RawContentPack current in contentPacks)
            {
                this.Monitor.VerboseLog($"Loading content pack '{current.Manifest.Name}'...");

                try
                {
                    ContentConfig content = current.Content;

                    // load tokens
                    ModTokenContext tokenContext = this.TokenManager.TrackLocalTokens(current.ManagedPack.Pack);
                    {
                        // load config.json
                        InvariantDictionary<ConfigField> config = configFileHandler.Read(current.ManagedPack, content.ConfigSchema, current.Content.Format);
                        configFileHandler.Save(current.ManagedPack, config, this.Helper);
                        if (config.Any())
                            this.Monitor.VerboseLog($"   found config.json with {config.Count} fields...");

                        // load config tokens
                        foreach (KeyValuePair<string, ConfigField> pair in config)
                        {
                            ConfigField field = pair.Value;
                            tokenContext.Add(new ImmutableToken($"{current.Manifest.UniqueID}/{pair.Key}", field.Value, allowedValues: field.AllowValues, canHaveMultipleValues: field.AllowMultiple));
                        }

                        // load dynamic tokens
                        foreach (DynamicTokenConfig entry in content.DynamicTokens ?? new DynamicTokenConfig[0])
                        {
                            void LogSkip(string reason) => this.Monitor.Log($"Ignored {current.Manifest.Name} > dynamic token '{entry.Name}': {reason}", LogLevel.Warn);

                            // validate token key
                            if (string.IsNullOrWhiteSpace(entry.Name))
                            {
                                LogSkip("the token name can't be empty.");
                                continue;
                            }
                            if (entry.Name.Contains(InternalConstants.InputArgSeparator))
                            {
                                LogSkip($"the token name can't have an input argument ({InternalConstants.InputArgSeparator} character).");
                                continue;
                            }
                            if (Enum.TryParse<ConditionType>(entry.Name, true, out _))
                            {
                                LogSkip("the token name is already used by a global token.");
                                continue;
                            }
                            if (config.ContainsKey(entry.Name))
                            {
                                LogSkip("the token name is already used by a config token.");
                                continue;
                            }

                            // parse values
                            ITokenString values;
                            if (!string.IsNullOrWhiteSpace(entry.Value))
                            {
                                if (!this.TryParseStringTokens(current.Manifest.UniqueID, entry.Value, tokenContext, current.Migrator, out string valueError, out values))
                                {
                                    LogSkip($"the token value is invalid: {valueError}");
                                    continue;
                                }
                            }
                            else
                                values = new LiteralString("");

                            // parse conditions
                            IList<Condition> conditions;
                            {
                                if (!this.TryParseConditions(current.Manifest.UniqueID, entry.When, tokenContext, current.Migrator, out conditions, out string conditionError))
                                {
                                    this.Monitor.Log($"Ignored {current.Manifest.Name} > '{entry.Name}' token: its {nameof(DynamicTokenConfig.When)} field is invalid: {conditionError}.", LogLevel.Warn);
                                    continue;
                                }
                            }

                            // add token
                            tokenContext.Add(new DynamicTokenValue($"{current.Manifest.UniqueID}/{entry.Name}", values, conditions));
                        }
                    }

                    // load patches
                    content.Changes = this.SplitPatches(content.Changes).ToArray();
                    this.NamePatches(current.ManagedPack, content.Changes);
                    foreach (PatchConfig patch in content.Changes)
                    {
                        this.Monitor.VerboseLog($"   loading {patch.LogName}...");
                        this.LoadPatch(current.ManagedPack, patch, tokenContext, current.Migrator, logSkip: reasonPhrase => this.Monitor.Log($"Ignored {patch.LogName}: {reasonPhrase}", LogLevel.Warn));
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading content pack '{current.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }
            }
        }

        /// <summary>Split patches with multiple target values.</summary>
        /// <param name="patches">The patches to split.</param>
        private IEnumerable<PatchConfig> SplitPatches(IEnumerable<PatchConfig> patches)
        {
            foreach (PatchConfig patch in patches)
            {
                if (string.IsNullOrWhiteSpace(patch.Target) || !patch.Target.Contains(","))
                {
                    yield return patch;
                    continue;
                }

                int i = 0;
                foreach (string target in patch.Target.Split(','))
                {
                    i++;
                    yield return new PatchConfig(patch)
                    {
                        LogName = !string.IsNullOrWhiteSpace(patch.LogName) ? $"{patch.LogName} {"".PadRight(i, 'I')}" : "",
                        Target = target.Trim()
                    };
                }
            }
        }

        /// <summary>Set a unique name for all patches in a content pack.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="patches">The patches to name.</param>
        private void NamePatches(ManagedContentPack contentPack, PatchConfig[] patches)
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
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="logSkip">The callback to invoke with the error reason if loading it fails.</param>
        private bool LoadPatch(ManagedContentPack pack, PatchConfig entry, IContext tokenContext, IMigration migrator, Action<string> logSkip)
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
                ITokenString assetName;
                {
                    if (string.IsNullOrWhiteSpace(entry.Target))
                        return TrackSkip($"must set the {nameof(PatchConfig.Target)} field.");
                    if (!this.TryParseStringTokens(pack.Manifest.UniqueID, entry.Target, tokenContext, migrator, out string error, out assetName))
                        return TrackSkip($"the {nameof(PatchConfig.Target)} is invalid: {error}");
                }

                // parse 'enabled'
                bool enabled = true;
                {
                    if (entry.Enabled != null && !this.TryParseEnabled(pack.Manifest.UniqueID, entry.Enabled, tokenContext, migrator, out string error, out enabled))
                        return TrackSkip($"invalid {nameof(PatchConfig.Enabled)} value '{entry.Enabled}': {error}");
                }

                // parse conditions
                IList<Condition> conditions;
                {
                    if (!this.TryParseConditions(pack.Manifest.UniqueID, entry.When, tokenContext, migrator, out conditions, out string error))
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
                            if (!this.TryPrepareLocalAsset(pack, entry.FromFile, tokenContext, migrator, out string error, out ITokenString fromAsset))
                                return TrackSkip(error);
                            patch = new LoadPatch(entry.LogName, pack, assetName, conditions, fromAsset, this.Helper.Content.NormaliseAssetName);
                        }
                        break;

                    // edit data
                    case PatchType.EditData:
                        {
                            // validate
                            if (entry.Entries == null && entry.Fields == null)
                                return TrackSkip($"either {nameof(PatchConfig.Entries)} or {nameof(PatchConfig.Fields)} must be specified for a '{action}' change.");

                            // parse entries
                            List<EditDataPatchRecord> entries = new List<EditDataPatchRecord>();
                            if (entry.Entries != null)
                            {
                                foreach (KeyValuePair<string, JToken> pair in entry.Entries)
                                {
                                    if (!this.TryParseStringTokens(pack.Manifest.UniqueID, pair.Key, tokenContext, migrator, out string keyError, out ITokenString key))
                                        return TrackSkip($"{nameof(PatchConfig.Entries)} > '{key}' key is invalid: {keyError}.");
                                    if (!this.TryParseJsonTokens(pack.Manifest.UniqueID, pair.Value, tokenContext, migrator, out string error, out TokenisableJToken value))
                                        return TrackSkip($"{nameof(PatchConfig.Entries)} > '{key}' value is invalid: {error}.");

                                    entries.Add(new EditDataPatchRecord(key, value));
                                }
                            }

                            // parse fields
                            List<EditDataPatchField> fields = new List<EditDataPatchField>();
                            if (entry.Fields != null)
                            {
                                foreach (KeyValuePair<string, IDictionary<string, JToken>> recordPair in entry.Fields)
                                {
                                    // parse entry key
                                    if (!this.TryParseStringTokens(pack.Manifest.UniqueID, recordPair.Key, tokenContext, migrator, out string keyError, out ITokenString key))
                                        return TrackSkip($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} is invalid: {keyError}.");

                                    // parse fields
                                    foreach (var fieldPair in recordPair.Value)
                                    {
                                        // parse field key
                                        if (!this.TryParseStringTokens(pack.Manifest.UniqueID, fieldPair.Key, tokenContext, migrator, out string fieldError, out ITokenString fieldKey))
                                            return TrackSkip($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} > field {fieldPair.Key} key is invalid: {fieldError}.");

                                        // parse value
                                        if (!this.TryParseJsonTokens(pack.Manifest.UniqueID, fieldPair.Value, tokenContext, migrator, out string valueError, out TokenisableJToken value))
                                            return TrackSkip($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} > field {fieldKey} is invalid: {valueError}.");
                                        if (value?.Value is JValue jValue && jValue.Value<string>()?.Contains("/") == true)
                                            return TrackSkip($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} > field {fieldKey} is invalid: value can't contain field delimiter character '/'.");

                                        fields.Add(new EditDataPatchField(key, fieldKey, value));
                                    }
                                }
                            }

                            // save
                            patch = new EditDataPatch(entry.LogName, pack, assetName, conditions, entries, fields, this.Monitor, this.Helper.Content.NormaliseAssetName);
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
                            if (!this.TryPrepareLocalAsset(pack, entry.FromFile, tokenContext, migrator, out string error, out ITokenString fromAsset))
                                return TrackSkip(error);
                            patch = new EditImagePatch(entry.LogName, pack, assetName, conditions, fromAsset, entry.FromArea, entry.ToArea, patchMode, this.Monitor, this.Helper.Content.NormaliseAssetName);
                        }
                        break;

                    default:
                        return TrackSkip($"unsupported patch type '{action}'.");
                }

                // skip if not enabled
                // note: we process the patch even if it's disabled, so any errors are caught by the modder instead of only failing after the patch is enabled.
                if (!enabled)
                    return TrackSkip($"{nameof(PatchConfig.Enabled)} is false.", warn: false);

                // save patch
                this.PatchManager.Add(patch);
                return true;
            }
            catch (Exception ex)
            {
                return TrackSkip($"error reading info. Technical details:\n{ex}");
            }
        }

        /// <summary>Normalise and parse the given condition values.</summary>
        /// <param name="modId">The mod ID of the current pack.</param>
        /// <param name="raw">The raw condition values to normalise.</param>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="conditions">The normalised conditions.</param>
        /// <param name="error">An error message indicating why normalisation failed.</param>
        private bool TryParseConditions(string modId, InvariantDictionary<string> raw, IContext tokenContext, IMigration migrator, out IList<Condition> conditions, out string error)
        {
            conditions = new List<Condition>();

            // no conditions
            if (raw == null || !raw.Any())
            {
                error = null;
                return true;
            }

            // parse conditions
            Lexer lexer = new Lexer();
            foreach (KeyValuePair<string, string> pair in raw)
            {
                // get lexical tokens
                ILexToken[] lexTokens = lexer.ParseBits(pair.Key, impliedBraces: true).ToArray();
                for (int i = 0; i < lexTokens.Length; i++)
                {
                    if (!migrator.TryMigrate(ref lexTokens[0], out error))
                    {
                        conditions = null;
                        return false;
                    }
                }

                // parse condition key
                if (lexTokens.Length != 1 || !(lexTokens[0] is LexTokenToken lexToken))
                {
                    error = $"'{pair.Key}' isn't a valid token name";
                    conditions = null;
                    return false;
                }
                ITokenString input = new TokenString(lexToken.InputArg, tokenContext, modId);

                // get token
                IToken token = tokenContext.GetToken(lexToken.Name, enforceContext: false);
                if (token == null)
                {
                    token = tokenContext.GetToken($"{modId}/{lexToken.Name}", enforceContext: false);
                    if (token == null)
                    {
                        error = $"'{pair.Key}' isn't a valid condition; must be one of {string.Join(", ", tokenContext.GetTokens(enforceContext: false).Select(p => p.Name).OrderBy(p => p))}";
                        conditions = null;
                        return false;
                    }
                }

                // validate input
                if (!token.TryValidateInput(input, out error))
                {
                    conditions = null;
                    return false;
                }

                // parse values
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    error = $"can't parse condition {pair.Key}: value can't be empty";
                    conditions = null;
                    return false;
                }
                if (!this.TryParseStringTokens(modId, pair.Value, tokenContext, migrator, out error, out ITokenString values))
                {
                    error = $"can't parse condition {pair.Key}: {error}";
                    return false;
                }

                // validate token keys & values
                if (!values.IsMutable && !token.TryValidateValues(input, values.SplitValues(), tokenContext, out string customError))
                {
                    error = $"invalid {lexToken.Name} condition: {customError}";
                    conditions = null;
                    return false;
                }

                // create condition
                conditions.Add(new Condition(name: token.Name, input: input, values: values));
            }

            // return parsed conditions
            error = null;
            return true;
        }

        /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
        /// <param name="field">The field value to parse.</param>
        public InvariantHashSet ParseCommaDelimitedField(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return new InvariantHashSet();

            IEnumerable<string> values = (
                from value in field.Split(',')
                where !string.IsNullOrWhiteSpace(value)
                select value.Trim()
            );
            return new InvariantHashSet(values);
        }

        /// <summary>Parse a boolean <see cref="PatchConfig.Enabled"/> value from a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="modId">The mod ID of the current pack.</param>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseEnabled(string modId, string rawValue, IContext tokenContext, IMigration migrator, out string error, out bool parsed)
        {
            parsed = false;

            // analyse string
            if (!this.TryParseStringTokens(modId, rawValue, tokenContext, migrator, out error, out ITokenString tokenString))
                return false;

            // validate & extract tokens
            string text = rawValue;
            if (tokenString.HasAnyTokens)
            {
                // only one token allowed
                if (!tokenString.IsSingleTokenOnly)
                {
                    error = "can't be treated as a true/false value because it contains multiple tokens.";
                    return false;
                }

                // parse token
                LexTokenToken lexToken = tokenString.GetTokenPlaceholders(recursive: false).Single();
                IToken token = tokenContext.GetToken(lexToken.Name, enforceContext: false);
                if (token == null)
                    token = tokenContext.GetToken($"{modId}/{lexToken.Name}", enforceContext: false);
                ITokenString input = new TokenString(lexToken.InputArg, tokenContext, modId);

                // check token options
                InvariantHashSet allowedValues = token?.GetAllowedValues(input);
                if (token == null || token.IsMutable || !token.IsReady)
                {
                    error = $"can only use static tokens in this field, consider using a {nameof(PatchConfig.When)} condition instead.";
                    return false;
                }
                if (allowedValues == null || !allowedValues.All(p => bool.TryParse(p, out _)))
                {
                    error = "that token isn't restricted to 'true' or 'false'.";
                    return false;
                }
                if (token.CanHaveMultipleValues(input))
                {
                    error = "can't be treated as a true/false value because that token can have multiple values.";
                    return false;
                }

                text = token.GetValues(input).First();
            }

            // parse text
            if (!bool.TryParse(text, out parsed))
            {
                error = $"can't parse {tokenString.Raw} as a true/false value.";
                return false;
            }
            return true;
        }

        /// <summary>Parse a JSON structure which can contain tokens, and validate that it's valid.</summary>
        /// <param name="modId">The mod ID of the current pack.</param>
        /// <param name="rawJson">The raw JSON structure which may contain tokens.</param>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value, which may be legitimately <c>null</c> even if successful.</param>
        private bool TryParseJsonTokens(string modId, JToken rawJson, IContext tokenContext, IMigration migrator, out string error, out TokenisableJToken parsed)
        {
            if (rawJson == null)
            {
                error = null;
                parsed = null;
                return true;
            }

            // parse
            parsed = new TokenisableJToken(rawJson, tokenContext, modId);
            if (!migrator.TryMigrate(parsed, out error))
                return false;

            // validate tokens
            ITokenString[] tokenStrings = parsed.GetTokenStrings().ToArray();
            if (tokenStrings.Any())
            {
                // validate unknown tokens
                string[] unknownTokens = tokenStrings.SelectMany(p => p.InvalidTokens).OrderBy(p => p).ToArray();
                if (unknownTokens.Any())
                {
                    error = $"found unknown tokens ({string.Join(", ", unknownTokens)})";
                    parsed = null;
                    return false;
                }

                // validate tokens
                foreach (LexTokenToken lexToken in tokenStrings.SelectMany(p => p.GetTokenPlaceholders(recursive: false)).Distinct())
                {
                    IToken token = tokenContext.GetToken(lexToken.Name, enforceContext: false);
                    if (token == null)
                    {
                        token = tokenContext.GetToken($"{modId}/{lexToken.Name}", enforceContext: false);
                        if (token == null)
                        {
                            error = $"'{lexToken}' can't be used as a token because that token could not be found."; // should never happen
                            parsed = null;
                            return false;
                        }
                    }
                }
            }

            // looks OK
            if (parsed.Value.Type == JTokenType.Null)
                parsed = null;
            error = null;
            return true;
        }

        /// <summary>Parse a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="modId">The mod ID of the current pack.</param>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseStringTokens(string modId, string rawValue, IContext tokenContext, IMigration migrator, out string error, out ITokenString parsed)
        {
            // parse
            parsed = new TokenString(rawValue, tokenContext, modId);
            if (!migrator.TryMigrate(parsed, out error))
                return false;

            // validate unknown tokens
            if (parsed.InvalidTokens.Any())
            {
                error = $"found unknown tokens ({string.Join(", ", parsed.InvalidTokens.OrderBy(p => p))})";
                parsed = null;
                return false;
            }

            // validate tokens
            foreach (LexTokenToken lexToken in parsed.GetTokenPlaceholders(recursive: false))
            {
                IToken token = tokenContext.GetToken(lexToken.Name, enforceContext: false);
                if (token == null)
                {
                    token = tokenContext.GetToken($"{modId}/{lexToken.Name}", enforceContext: false);
                    if (token == null)
                    {
                        error = $"'{lexToken}' can't be used as a token because that token could not be found."; // should never happen
                        parsed = null;
                        return false;
                    }
                }
            }

            // looks OK
            error = null;
            return true;
        }

        /// <summary>Prepare a local asset file for a patch to use.</summary>
        /// <param name="pack">The content pack being loaded.</param>
        /// <param name="path">The asset path in the content patch.</param>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="error">The error reason if preparing the asset fails.</param>
        /// <param name="tokenedPath">The parsed value.</param>
        /// <returns>Returns whether the local asset was successfully prepared.</returns>
        private bool TryPrepareLocalAsset(ManagedContentPack pack, string path, IContext tokenContext, IMigration migrator, out string error, out ITokenString tokenedPath)
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
            if (!this.TryParseStringTokens(pack.Manifest.UniqueID, path, tokenContext, migrator, out string tokenError, out tokenedPath))
            {
                error = $"the {nameof(PatchConfig.FromFile)} is invalid: {tokenError}";
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
        private string NormaliseLocalAssetPath(ManagedContentPack contentPack, string path)
        {
            // normalise asset name
            if (string.IsNullOrWhiteSpace(path))
                return null;
            string newPath = this.Helper.Content.NormaliseAssetName(path);

            // add .xnb extension if needed (it's stripped from asset names)
            string fullPath = contentPack.GetFullPath(newPath);
            if (!File.Exists(fullPath))
            {
                if (File.Exists($"{fullPath}.xnb") || Path.GetExtension(path) == ".xnb")
                    newPath += ".xnb";
            }

            return newPath;
        }
    }
}
