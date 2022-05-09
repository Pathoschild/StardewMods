using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Api;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Validators;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

[assembly: InternalsVisibleTo("Pathoschild.Stardew.Tests.Mods")]
namespace ContentPatcher
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages state for each screen.</summary>
        private PerScreen<ScreenManager> ScreenManager = null!; // set in Entry

        /// <summary>The raw data for loaded content packs.</summary>
        private LoadedContentPack[]? ContentPacks;

        /// <summary>The recognized format versions and their migrations.</summary>
        private readonly Func<ContentConfig?, IMigration[]> GetFormatVersions = content => new IMigration[]
        {
            new Migration_1_0(),
            new Migration_1_3(),
            new Migration_1_4(),
            new Migration_1_5(),
            new Migration_1_6(),
            new Migration_1_7(),
            new Migration_1_8(),
            new Migration_1_9(),
            new Migration_1_10(),
            new Migration_1_11(),
            new Migration_1_13(),
            new Migration_1_14(),
            new Migration_1_15_Prevalidation(),
            new Migration_1_15_Rewrites(content),
            new Migration_1_16(),
            new Migration_1_17(),
            new Migration_1_18(),
            new Migration_1_19(),
            new Migration_1_20(),
            new Migration_1_21(),
            new Migration_1_22(),
            new Migration_1_23(),
            new Migration_1_24(),
            new Migration_1_25(),
            new Migration_1_26()
        };

        /// <summary>The special validation logic to apply to assets affected by patches.</summary>
        private readonly Func<IAssetValidator[]> AssetValidators = () => new IAssetValidator[]
        {
            new StardewValley_1_3_36_Validator()
        };

        /// <summary>Handles the 'patch' console command.</summary>
        private CommandHandler CommandHandler = null!; // set in Entry

        /// <summary>The mod configuration.</summary>
        private ModConfig Config = null!; // set in Entry

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys => this.Config.Controls;

        /// <summary>The debug overlay (if enabled).</summary>
        private readonly PerScreen<DebugOverlay?> DebugOverlay = new();

        /// <summary>The mod tokens queued for addition. This is null after the first update tick, when new tokens can no longer be added.</summary>
        private readonly List<ModProvidedToken> QueuedModTokens = new();

        /// <summary>The game tick when the conditions API became ready for use.</summary>
        private int ConditionsApiReadyTick = int.MaxValue;

        /// <summary>Whether the next tick is the first one for the main screen.</summary>
        private bool IsFirstTick = true;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            this.ScreenManager = new(this.CreateScreenManager);

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;
        }

        /// <summary>Get an API that other mods can access. This is always called after <see cref="Entry"/>.</summary>
        public override object GetApi()
        {
            return new ContentPatcherAPI(
                contentPatcherID: this.ModManifest.UniqueID,
                monitor: this.Monitor,
                reflection: this.Helper.Reflection,
                addModToken: this.AddModToken,
                isConditionsApiReady: () => Game1.ticks >= this.ConditionsApiReadyTick,
                parseConditions: this.ParseConditionsForApi
            );
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <inheritdoc cref="IInputEvents.ButtonsChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (this.Config.EnableDebugFeatures)
            {
                // toggle overlay
                if (this.Keys.ToggleDebug.JustPressed())
                {
                    if (this.DebugOverlay.Value == null)
                        this.DebugOverlay.Value = new DebugOverlay(this.Helper.Events, this.Helper.Input, this.Helper.GameContent, this.Helper.Reflection);
                    else
                    {
                        this.DebugOverlay.Value.Dispose();
                        this.DebugOverlay.Value = null;
                    }
                }

                // cycle textures
                else if (this.DebugOverlay.Value != null)
                {
                    if (this.Keys.DebugPrevTexture.JustPressed())
                        this.DebugOverlay.Value.PrevTexture();
                    if (this.Keys.DebugNextTexture.JustPressed())
                        this.DebugOverlay.Value.NextTexture();
                }
            }
        }

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            this.ScreenManager.Value.OnAssetRequested(e);
        }

        /// <inheritdoc cref="ISpecializedEvents.LoadStageChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnLoadStageChanged(object? sender, LoadStageChangedEventArgs e)
        {
            this.ScreenManager.Value.OnLoadStageChanged(e.OldStage, e.NewStage);
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            this.ScreenManager.Value.OnDayStarted();
        }

        /// <inheritdoc cref="IGameLoopEvents.TimeChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            this.ScreenManager.Value.OnTimeChanged();
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            this.ScreenManager.Value.OnWarped();
        }

        /// <inheritdoc cref="IGameLoopEvents.ReturnedToTitle"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            this.ScreenManager.Value.OnReturnedToTitle();
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // initialize after first tick on main screen so other mods can register their tokens in SMAPI's GameLoop.GameLaunched event
            if (this.IsFirstTick)
            {
                this.IsFirstTick = false;
                this.Initialize();
                this.ConditionsApiReadyTick = Game1.ticks + 1; // mods can only use conditions API on the next tick, to avoid race conditions
            }

            // run update logic
            LoadedContentPack[] contentPacks = this.ContentPacks!; // set in Initialize() above
            this.InitializeScreenManagerIfNeeded(contentPacks);
            this.ScreenManager.Value.OnUpdateTicked();
        }

        /// <inheritdoc cref="IContentEvents.LocaleChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
        {
            if (!this.IsFirstTick)
                this.ScreenManager.Value.OnLocaleChanged();
        }

        /****
        ** Methods
        ****/
        /// <summary>Initialize the mod and content packs.</summary>
        private void Initialize()
        {
            var helper = this.Helper;

            // fetch content packs
            this.ContentPacks = this.GetContentPacks().ToArray();

            // log custom tokens
            {
                var tokensByMod = (
                    from token in this.QueuedModTokens.OrderByHuman(p => p.Name)
                    group token by token.Mod into modGroup
                    select new { ModName = modGroup.Key.Name, ModPrefix = modGroup.First().NamePrefix, TokenNames = modGroup.Select(p => p.NameWithoutPrefix).ToArray() }
                );
                foreach (var group in tokensByMod)
                    this.Monitor.Log($"{group.ModName} added {(group.TokenNames.Length == 1 ? "a custom token" : $"{group.TokenNames.Length} custom tokens")} with prefix '{group.ModPrefix}': {string.Join(", ", group.TokenNames)}.");
            }

            // set up events
            if (this.Config.EnableDebugFeatures)
                helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Specialized.LoadStageChanged += this.OnLoadStageChanged;

            // load screen manager
            this.InitializeScreenManagerIfNeeded(this.ContentPacks);

            // set up commands
            this.CommandHandler = new CommandHandler(
                screenManager: this.ScreenManager,
                monitor: this.Monitor,
                contentHelper: this.Helper.GameContent,
                contentPacks: this.ContentPacks,
                getContext: modID => modID == null ? this.ScreenManager.Value.TokenManager : this.ScreenManager.Value.TokenManager.GetContextFor(modID),
                updateContext: () => this.ScreenManager.Value.UpdateContext(ContextUpdateType.All)
            );
            this.CommandHandler.RegisterWith(helper.ConsoleCommands);

            // register content packs with Generic Mod Config Menu
            foreach (LoadedContentPack contentPack in this.ContentPacks)
            {
                if (contentPack.Config.Any())
                {
                    GenericModConfigMenuIntegrationForContentPack configMenu = new GenericModConfigMenuIntegrationForContentPack(
                        contentPack: contentPack.ContentPack,
                        modRegistry: this.Helper.ModRegistry,
                        monitor: this.Monitor,
                        manifest: contentPack.Manifest,
                        parseCommaDelimitedField: this.ParseCommaDelimitedField,
                        config: contentPack.Config,
                        saveAndApply: () => this.OnContentPackConfigChanged(contentPack)
                    );
                    configMenu.Register();
                }
            }
        }

        /// <summary>Create a raw uninitialized screen manager instance.</summary>
        private ScreenManager CreateScreenManager()
        {
            var modTokens = this.QueuedModTokens.ToArray();
            return new ScreenManager(
                helper: this.Helper,
                monitor: this.Monitor,
                installedMods: this.GetInstalledMods(),
                modTokens: modTokens,
                assetValidators: this.AssetValidators()
            );
        }

        /// <summary>Initialize the screen manager if needed.</summary>
        /// <param name="contentPacks">The raw data for loaded content packs.</param>
        private void InitializeScreenManagerIfNeeded(LoadedContentPack[] contentPacks)
        {
            var manager = this.ScreenManager.Value;
            if (!manager.IsInitialized)
                manager.Initialize(contentPacks, this.GetInstalledMods());
        }

        /// <summary>Get the unique IDs for all installed mods and content packs.</summary>
        private InvariantHashSet GetInstalledMods()
        {
            return new InvariantHashSet(
                this.Helper.ModRegistry
                    .GetAll()
                    .Select(p => p.Manifest.UniqueID)
                    .OrderByHuman()
            );
        }

        /// <summary>Raised after a content pack's configuration changed.</summary>
        /// <param name="contentPack">The content pack instance.</param>
        private void OnContentPackConfigChanged(LoadedContentPack contentPack)
        {
            // re-save config.json
            contentPack.ConfigFileHandler.Save(contentPack.ContentPack, contentPack.Config, this.Helper);

            // update tokens
            foreach (var screenManager in this.ScreenManager.GetActiveValues())
                screenManager.Value.OnContentPackConfigChanged(contentPack);
        }

        /// <summary>Add a mod-provided token.</summary>
        /// <param name="token">The token to add.</param>
        private void AddModToken(ModProvidedToken token)
        {
            if (!this.IsFirstTick)
            {
                this.Monitor.Log($"Rejected token added by {token.Mod.Name} because tokens can't be added after SMAPI's {nameof(this.Helper.Events.GameLoop)}.{nameof(this.Helper.Events.GameLoop.GameLaunched)} event.", LogLevel.Error);
                return;
            }

            this.QueuedModTokens.Add(token);
        }

        /// <summary>Parse raw conditions for an API consumer.</summary>
        /// <param name="manifest">The manifest of the mod parsing the conditions.</param>
        /// <param name="rawConditions">The raw conditions to parse.</param>
        /// <param name="formatVersion">The format version for which to parse conditions.</param>
        /// <param name="assumeModIds">The unique IDs of mods whose custom tokens to allow in the <paramref name="rawConditions"/>.</param>
        private IManagedConditions ParseConditionsForApi(IManifest manifest, InvariantDictionary<string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null)
        {
            InvariantHashSet assumeModIdsLookup = new(assumeModIds ?? Enumerable.Empty<string>()) { manifest.UniqueID };
            IMigration migrator = new AggregateMigration(formatVersion, this.GetFormatVersions(null));

            return new ApiManagedConditions(
                parse: () =>
                {
                    ScreenManager screen = this.ScreenManager.Value;
                    IContext context = screen.TokenManager;
                    TokenParser tokenParser = new(context, manifest, migrator, assumeModIdsLookup);

                    bool isValid = screen.PatchLoader.TryParseConditions(rawConditions, tokenParser, new LogPathBuilder(), out Condition[] conditions, out _, out string? error);
                    var managed = new ApiManagedConditionsForSingleScreen(conditions, context, isValid: isValid, validationError: error);
                    managed.UpdateContext();

                    return managed;
                }
            );
        }

        /// <summary>Load the registered content packs.</summary>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private IEnumerable<LoadedContentPack> GetContentPacks()
        {
            this.Monitor.VerboseLog("Preloading content packs...");

            int index = -1;
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                // load raw content pack
                RawContentPack rawContentPack;
                try
                {
                    rawContentPack = new RawContentPack(contentPack, ++index, this.GetFormatVersions);

                    if (!rawContentPack.TryReloadContent(out string? error))
                    {
                        this.Monitor.Log($"Could not load content pack '{contentPack.Manifest.Name}': {error}.", LogLevel.Error);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error preloading content pack '{contentPack.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }

                // load config
                ConfigFileHandler configFileHandler;
                InvariantDictionary<ConfigField> config;
                try
                {
                    configFileHandler = new ConfigFileHandler("config.json", this.ParseCommaDelimitedField, (pack, label, reason) => this.Monitor.Log($"Ignored {pack.Manifest.Name} > {label}: {reason}", LogLevel.Warn));
                    config = configFileHandler.Read(contentPack, rawContentPack.Content.ConfigSchema, rawContentPack.Content.Format!);
                    configFileHandler.Save(contentPack, config, this.Helper);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading configuration for content pack '{contentPack.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }

                // build content pack
                yield return new LoadedContentPack(rawContentPack, configFileHandler, config);
            }
        }

        /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
        /// <param name="field">The field value to parse.</param>
        private InvariantHashSet ParseCommaDelimitedField(string? field)
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
    }
}
