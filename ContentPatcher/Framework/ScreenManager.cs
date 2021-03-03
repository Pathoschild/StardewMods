using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Locations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Validators;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewValley;

namespace ContentPatcher.Framework
{
    /// <summary>Manages the content assets for a screen.</summary>
    /// <remarks>There's only one instance in single-player, but multiple instances in split-screen mode.</remarks>
    internal class ScreenManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>The public SMAPI APIs.</summary>
        private readonly IModHelper Helper;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The content packs whose configuration changed.</summary>
        private readonly HashSet<LoadedContentPack> QueuedForConfigUpdates = new();

        /// <summary>Whether the next tick is the first one for the current screen.</summary>
        private bool IsFirstTick = true;


        /*********
        ** Accessors
        *********/
        /// <summary>Handles loading and unloading patches.</summary>
        public PatchLoader PatchLoader { get; }

        /// <summary>Manages the available contextual tokens.</summary>
        public TokenManager TokenManager { get; }

        /// <summary>Manages loaded patches.</summary>
        public PatchManager PatchManager { get; }

        /// <summary>Handles loading custom location data and adding it to the game.</summary>
        public CustomLocationManager CustomLocationManager { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="helper">The public SMAPI APIs.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        /// <param name="modTokens">The custom tokens provided by mods.</param>
        /// <param name="assetValidators">Handle special validation logic on loaded or edited assets.</param>
        /// <param name="contentPacks">The content packs to load.</param>
        public ScreenManager(IModHelper helper, IMonitor monitor, InvariantHashSet installedMods, ModProvidedToken[] modTokens, IAssetValidator[] assetValidators, LoadedContentPack[] contentPacks)
        {
            this.Helper = helper;
            this.Monitor = monitor;
            this.TokenManager = new TokenManager(helper.Content, installedMods, modTokens, helper.Reflection);
            this.PatchManager = new PatchManager(this.Monitor, this.TokenManager, assetValidators);
            this.PatchLoader = new PatchLoader(this.PatchManager, this.TokenManager, this.Monitor, helper.Reflection, installedMods, helper.Content.NormalizeAssetName);
            this.CustomLocationManager = new CustomLocationManager(this.Monitor);

            this.Initialize(contentPacks, installedMods);
        }

        /// <summary>Raised when the low-level stage in the game's loading process has changed. This is an advanced event for mods which need to run code at specific points in the loading process. The available stages or when they happen might change without warning in future versions (e.g. due to changes in the game's load process), so mods using this event are more likely to break or have bugs.</summary>
        /// <param name="oldStage">The previous load stage.</param>
        /// <param name="newStage">The new load stage.</param>
        public void OnLoadStageChanged(LoadStage oldStage, LoadStage newStage)
        {
            // add locations
            if (newStage == LoadStage.CreatedInitialLocations || newStage == LoadStage.SaveAddedLocations)
                this.CustomLocationManager.Apply(saveLocations: SaveGame.loaded.locations, gameLocations: Game1.locations);

            // update context
            switch (newStage)
            {
                case LoadStage.CreatedBasicInfo:
                case LoadStage.SaveLoadedBasicInfo:
                case LoadStage.Loaded when Game1.dayOfMonth == 0: // handled by OnDayStarted if we're not creating a new save
                    this.Monitor.VerboseLog($"Updating context: load stage changed to {newStage}.");
                    this.TokenManager.IsBasicInfoLoaded = true;
                    this.UpdateContext(ContextUpdateType.All);
                    break;
            }
        }

        /// <summary>The method invoked when a new day starts.</summary>
        public void OnDayStarted()
        {
            this.Monitor.VerboseLog("Updating context: new day started.");
            this.TokenManager.IsBasicInfoLoaded = true;
            this.UpdateContext(ContextUpdateType.All);
        }

        /// <summary>The method invoked when the in-game clock changes.</summary>
        public void OnTimeChanged()
        {
            this.Monitor.VerboseLog("Updating context: clock changed.");
            this.UpdateContext(ContextUpdateType.OnTimeChange);
        }

        /// <summary>The method invoked when the player warps.</summary>
        public void OnWarped()
        {
            this.Monitor.VerboseLog("Updating context: player warped.");
            this.UpdateContext(ContextUpdateType.OnLocationChange);
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        public void OnReturnedToTitle()
        {
            this.Monitor.VerboseLog("Updating context: returned to title.");
            this.TokenManager.IsBasicInfoLoaded = false;
            this.UpdateContext(ContextUpdateType.All);
        }

        /// <summary>Raised after the game performs its overall update tick (≈60 times per second).</summary>
        public void OnUpdateTicked()
        {
            this.IsFirstTick = false;

            if (this.QueuedForConfigUpdates.Any())
            {
                foreach (var contentPack in this.QueuedForConfigUpdates)
                    this.ApplyNewConfig(contentPack);

                this.QueuedForConfigUpdates.Clear();
            }
        }

        /// <summary>Raised after the game language is changed, and after SMAPI handles the change.</summary>
        public void OnLocaleChanged()
        {
            // update if locale changes after initialization
            if (!this.IsFirstTick)
                this.UpdateContext(ContextUpdateType.All);
        }

        /// <summary>Raised after a content pack's configuration changed.</summary>
        /// <param name="contentPack">The content pack instance.</param>
        public void OnContentPackConfigChanged(LoadedContentPack contentPack)
        {
            this.QueuedForConfigUpdates.Add(contentPack);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="updateType">The context update type.</param>
        public void UpdateContext(ContextUpdateType updateType)
        {
            this.TokenManager.UpdateContext(out InvariantHashSet changedGlobalTokens);
            this.PatchManager.UpdateContext(this.Helper.Content, changedGlobalTokens, updateType);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialize the mod and content packs.</summary>
        /// <param name="contentPacks">The content packs to load.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        private void Initialize(LoadedContentPack[] contentPacks, InvariantHashSet installedMods)
        {
            // set initial context before loading any custom mod tokens
            this.UpdateContext(ContextUpdateType.All);

            // load context
            this.LoadContentPacks(contentPacks, installedMods);

            // set initial context once patches + dynamic tokens + custom tokens are loaded
            this.UpdateContext(ContextUpdateType.All);
        }

        /// <summary>Load the patches from all registered content packs.</summary>
        /// <param name="contentPacks">The content packs to load.</param>
        /// <param name="installedMods">The mod IDs which are currently installed.</param>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private void LoadContentPacks(IEnumerable<LoadedContentPack> contentPacks, InvariantHashSet installedMods)
        {
            // load content packs

            foreach (LoadedContentPack current in contentPacks)
            {
                this.Monitor.VerboseLog($"Loading content pack '{current.Manifest.Name}'...");

                try
                {
                    ContentConfig content = current.Content;

                    // load tokens
                    ModTokenContext modContext = this.TokenManager.TrackLocalTokens(current.ContentPack);
                    TokenParser tokenParser = new TokenParser(modContext, current.Manifest, current.Migrator, installedMods);
                    {
                        // load config.json
                        var config = current.Config;
                        if (config.Any())
                            this.Monitor.VerboseLog($"   found config.json with {config.Count} fields...");

                        // load config tokens
                        foreach (KeyValuePair<string, ConfigField> pair in config)
                            this.AddConfigToken(pair.Key, pair.Value, modContext, current);

                        // load dynamic tokens
                        IDictionary<string, int> dynamicTokenCountByName = new InvariantDictionary<int>();
                        foreach (DynamicTokenConfig entry in content.DynamicTokens)
                        {
                            void LogSkip(string reason) => this.Monitor.Log($"Ignored {current.Manifest.Name} > dynamic token '{entry.Name}': {reason}", LogLevel.Warn);

                            // get path
                            LogPathBuilder localPath = current.LogPath.With(nameof(content.DynamicTokens));
                            {
                                if (!dynamicTokenCountByName.ContainsKey(entry.Name))
                                    dynamicTokenCountByName[entry.Name] = -1;
                                int discriminator = ++dynamicTokenCountByName[entry.Name];
                                localPath = localPath.With($"{entry.Name} {discriminator}");
                            }

                            // validate token key
                            if (string.IsNullOrWhiteSpace(entry.Name))
                            {
                                LogSkip("the token name can't be empty.");
                                continue;
                            }
                            if (entry.Name.Contains(InternalConstants.PositionalInputArgSeparator))
                            {
                                LogSkip($"the token name can't have positional input arguments ({InternalConstants.PositionalInputArgSeparator} character).");
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

                            // parse conditions
                            IList<Condition> conditions;
                            InvariantHashSet immutableRequiredModIDs;
                            {
                                if (!this.PatchLoader.TryParseConditions(entry.When, tokenParser, localPath.With(nameof(entry.When)), out conditions, out immutableRequiredModIDs, out string conditionError))
                                {
                                    this.Monitor.Log($"Ignored {current.Manifest.Name} > '{entry.Name}' token: its {nameof(DynamicTokenConfig.When)} field is invalid: {conditionError}.", LogLevel.Warn);
                                    continue;
                                }
                            }

                            // parse values
                            IManagedTokenString values;
                            if (!string.IsNullOrWhiteSpace(entry.Value))
                            {
                                if (!tokenParser.TryParseString(entry.Value, immutableRequiredModIDs, localPath.With(nameof(entry.Value)), out string valueError, out values))
                                {
                                    LogSkip($"the token value is invalid: {valueError}");
                                    continue;
                                }
                            }
                            else
                                values = new LiteralString("", localPath.With(nameof(entry.Value)));

                            // add token
                            modContext.AddDynamicToken(entry.Name, values, conditions);
                        }
                    }

                    // load patches
                    this.PatchLoader.LoadPatches(
                        contentPack: current,
                        rawPatches: content.Changes,
                        rootIndexPath: new[] { current.Index },
                        path: current.LogPath,
                        reindex: false,
                        parentPatch: null
                    );

                    // load custom locations
                    foreach (CustomLocationConfig location in content.CustomLocations)
                    {
                        if (!this.CustomLocationManager.TryAddCustomLocationConfig(location, current.ContentPack, out string error))
                            this.Monitor.Log($"Ignored {current.Manifest.Name} > custom location '{location?.Name}': {error}", LogLevel.Warn);
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading content pack '{current.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }
            }

            this.CustomLocationManager.EnforceUniqueNames();

            this.PatchManager.Reindex(patchListChanged: true);
        }

        /// <summary>Reapply a content pack when its configuration changes.</summary>
        /// <param name="contentPack">The content pack to reapply.</param>
        private void ApplyNewConfig(LoadedContentPack contentPack)
        {
            // reset config tokens
            ModTokenContext modContext = this.TokenManager.TrackLocalTokens(contentPack.ContentPack);
            foreach (var token in contentPack.Config)
                this.AddConfigToken(token.Key, token.Value, modContext, contentPack);

            // update tokens
            this.TokenManager.UpdateContext(out _);

            // reload changes to force-reset config token references
            if (!contentPack.TryReloadContent(out string loadContentError))
            {
                this.Monitor.Log($"Failed to reload content pack '{contentPack.Manifest.Name}' for configuration changes: {loadContentError}. The content pack may not be in a valid state.", LogLevel.Error); // should never happen
                return;
            }

            // update patches
            this.PatchLoader.UnloadPatchesLoadedBy(contentPack, false);
            this.PatchLoader.LoadPatches(
                contentPack: contentPack,
                rawPatches: contentPack.Content.Changes,
                rootIndexPath: new[] { contentPack.Index },
                path: contentPack.LogPath,
                reindex: true,
                parentPatch: null
            );
        }

        /// <summary>Register a config token for a content pack.</summary>
        /// <param name="name">The field name.</param>
        /// <param name="field">The config field.</param>
        /// <param name="modContext">The mod context to which to add the token.</param>
        /// <param name="contentPack">The content pack for which to add the token.</param>
        private void AddConfigToken(string name, ConfigField field, ModTokenContext modContext, LoadedContentPack contentPack)
        {
            modContext.RemoveLocalToken(name); // only needed when resetting a token for Generic Mod Config Menu, but has no effect otherwise

            IValueProvider valueProvider = new ImmutableValueProvider(
                name: name,
                values: field.Value,
                allowedValues: field.AllowValues,
                canHaveMultipleValues: field.AllowMultiple,
                isMutable: true // avoid inlining config values since they can be edited at runtime (e.g. via Generic Mod Config Menu)
            );
            IToken token = new Token(valueProvider, scope: contentPack.Manifest.UniqueID);
            modContext.AddLocalToken(token);
        }
    }
}
