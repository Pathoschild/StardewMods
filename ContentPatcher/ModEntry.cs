using System;
using System.IO;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
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

        /// <summary>Handles loading assets from content packs.</summary>
        private readonly AssetLoader AssetLoader = new AssetLoader();

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
            this.PatchManager = new PatchManager(helper.Content.CurrentLocaleConstant);
            this.LoadContentPacks();

            // register patcher
            helper.Content.AssetLoaders.Add(this.PatchManager);
            helper.Content.AssetEditors.Add(this.PatchManager);

            // set up events
            if (this.Config.EnableDebugFeatures)
                InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            SaveEvents.AfterReturnToTitle += this.SaveEvents_AfterReturnToTitle;
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
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
            this.Monitor.Log($"Context: date=none, weather=none, locale={language}.");
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
            this.Monitor.Log($"Context: date={date.DayOfWeek} {date.Season} {date.Day}, weather={weather}, locale={language}.");
            this.PatchManager.UpdateContext(contentHelper, language, date, weather);
        }

        /****
        ** Methods
        ****/
        /// <summary>Load the patches from all registered content packs.</summary>
        private void LoadContentPacks()
        {
            foreach (IContentPack pack in this.Helper.GetContentPacks())
            {
                this.Monitor.Log($"Loading content pack '{pack.Manifest.Name}'...", LogLevel.Trace);

                // read config
                ContentConfig config = pack.ReadJsonFile<ContentConfig>(this.PatchFileName);
                if (config == null)
                {
                    this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it has no {this.PatchFileName} file.", LogLevel.Warn);
                    continue;
                }
                if (config.Format == null || config.Changes == null)
                {
                    this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it doesn't specify the required {nameof(ContentConfig.Format)} or {nameof(ContentConfig.Changes)} fields.", LogLevel.Warn);
                    continue;
                }
                if (config.Format.ToString() != "1.0")
                {
                    this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it uses unsupported format {config.Format} (supported version: 1.0).", LogLevel.Warn);
                    continue;
                }

                // load patches
                int i = 0;
                foreach (PatchConfig entry in config.Changes)
                {
                    i++;
                    void LogSkip(string reasonPhrase) => this.Monitor.Log($"Ignored {pack.Manifest.Name} > entry #{i}: {reasonPhrase}", LogLevel.Warn);

                    try
                    {
                        // skip if disabled
                        if (!entry.Enabled)
                            continue;

                        // parse action
                        if (!Enum.TryParse(entry.Action, out PatchType action))
                        {
                            LogSkip(string.IsNullOrWhiteSpace(entry.Action)
                                ? $"must set the {nameof(PatchConfig.Action)} field."
                                : $"invalid {nameof(PatchConfig.Action)} value '{entry.Action}', expected one of: {string.Join(", ", Enum.GetNames(typeof(PatchType)))}."
                            );
                            continue;
                        }

                        // parse target asset
                        string assetName = !string.IsNullOrWhiteSpace(entry.Target)
                            ? this.Helper.Content.NormaliseAssetName(entry.Target)
                            : null;
                        if (assetName == null)
                        {
                            LogSkip($"must set the {nameof(PatchConfig.Target)} field.");
                            continue;
                        }

                        // parse source asset
                        string localAsset = this.NormaliseLocalAssetPath(pack, entry.FromFile);
                        if (localAsset == null && (action == PatchType.Load || action == PatchType.EditImage))
                        {
                            LogSkip($"must set the {nameof(PatchConfig.FromFile)} field for action '{action}'.");
                            continue;
                        }
                        if (localAsset != null)
                        {
                            localAsset = this.AssetLoader.GetActualPath(pack, localAsset);
                            if (localAsset == null)
                            {
                                LogSkip($"the {nameof(PatchConfig.FromFile)} field specifies a file that doesn't exist: {entry.FromFile}.");
                                continue;
                            }
                        }

                        // parse conditions
                        ConditionDictionary conditions;
                        {
                            if (!this.PatchManager.TryParseConditions(entry.When, out conditions, out string error))
                            {
                                LogSkip($"the {nameof(PatchConfig.When)} field is invalid: {error}.");
                                continue;
                            }
                        }

                        // parse & save patch
                        switch (action)
                        {
                            // load asset
                            case PatchType.Load:
                                {
                                    // init patch
                                    IPatch patch = new LoadPatch(this.AssetLoader, pack, assetName, conditions, localAsset);

                                    // detect conflicting loaders
                                    IPatch[] conflictingLoaders = this.PatchManager.GetConflictingLoaders(patch).ToArray();
                                    if (conflictingLoaders.Any())
                                    {
                                        if (conflictingLoaders.Any(p => p.ContentPack == pack))
                                            LogSkip($"the {assetName} file is already being loaded by this content pack in the same contexts. Each file can only be loaded once.");
                                        else
                                        {
                                            string[] conflictingNames = conflictingLoaders.Select(p => p.ContentPack.Manifest.Name).Distinct().OrderBy(p => p).ToArray();
                                            LogSkip($"the {assetName} file is already being loaded by other content packs in the same contexts ({string.Join(", ", conflictingNames)}). Each file can only be loaded once.");
                                        }
                                        continue;
                                    }

                                    // add
                                    this.PatchManager.Add(patch);
                                }
                                break;

                            // edit data
                            case PatchType.EditData:
                                {
                                    // validate
                                    if (entry.Entries == null && entry.Fields == null)
                                    {
                                        LogSkip($"either {nameof(PatchConfig.Entries)} or {nameof(PatchConfig.Fields)} must be specified for a '{action}' change.");
                                        continue;
                                    }
                                    if (entry.Entries != null && entry.Entries.Any(p => string.IsNullOrWhiteSpace(p.Value)))
                                    {
                                        LogSkip($"the {nameof(PatchConfig.Entries)} can't contain empty values.");
                                        continue;
                                    }
                                    if (entry.Fields != null && entry.Fields.Any(p => p.Value == null || p.Value.Any(n => n.Value == null)))
                                    {
                                        LogSkip($"the {nameof(PatchConfig.Fields)} can't contain empty values.");
                                        continue;
                                    }

                                    // save
                                    this.PatchManager.Add(new EditDataPatch(this.AssetLoader, pack, assetName, conditions, entry.Entries, entry.Fields, this.Monitor));
                                }
                                break;

                            // edit image
                            case PatchType.EditImage:
                                // read patch mode
                                PatchMode patchMode = PatchMode.Replace;
                                if (!string.IsNullOrWhiteSpace(entry.PatchMode) && !Enum.TryParse(entry.PatchMode, true, out patchMode))
                                {
                                    LogSkip($"the {nameof(PatchConfig.PatchMode)} is invalid. Expected one of these values: [{string.Join(", ", Enum.GetNames(typeof(PatchMode)))}].");
                                    continue;
                                }

                                // save
                                this.PatchManager.Add(new EditImagePatch(this.AssetLoader, pack, assetName, conditions, localAsset, entry.FromArea, entry.ToArea, patchMode, this.Monitor));
                                break;

                            default:
                                LogSkip($"unsupported patch type '{action}'.");
                                break;
                        }

                        // preload PNG assets to avoid load-in-draw-loop error
                        if (localAsset != null)
                            this.AssetLoader.PreloadIfNeeded(pack, localAsset);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"Ignored {pack.Manifest.Name} > entry #{i}: error reading info. Technical details:\n{ex}", LogLevel.Error);
                    }
                }
            }
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
    }
}
