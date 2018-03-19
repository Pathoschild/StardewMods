using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ContentPatcher.Framework;
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
            this.PatchManager = new PatchManager(this.Monitor, helper.Content.CurrentLocaleConstant);
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
            if (this.Config.VerboseLog)
                this.Monitor.Log($"Context: date=none, weather=none, locale={language}.", LogLevel.Trace);
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
            if (this.Config.VerboseLog)
                this.Monitor.Log($"Context: date={date.DayOfWeek} {date.Season} {date.Day}, weather={weather}, locale={language}.", LogLevel.Trace);
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
                if (this.Config.VerboseLog)
                    this.Monitor.Log($"Loading content pack '{pack.Manifest.Name}'...", LogLevel.Trace);

                try
                {
                    // read changes file
                    ContentConfig content = pack.ReadJsonFile<ContentConfig>(this.PatchFileName);
                    if (content == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it has no {this.PatchFileName} file.", LogLevel.Warn);
                        continue;
                    }
                    if (content.Format == null || content.Changes == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it doesn't specify the required {nameof(ContentConfig.Format)} or {nameof(ContentConfig.Changes)} fields.", LogLevel.Warn);
                        continue;
                    }
                    if (content.Format.ToString() != "1.0")
                    {
                        this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it uses unsupported format {content.Format} (supported version: 1.0).", LogLevel.Warn);
                        continue;
                    }

                    // load config.json
                    IDictionary<string, ConfigField> config = configFileHandler.Read(pack, content.ConfigSchema);
                    configFileHandler.Save(pack, config, this.Helper);

                    // load patches
                    int i = 0;
                    foreach (PatchConfig entry in content.Changes)
                    {
                        i++;
                        this.LoadPatch(pack, entry, config, logSkip: reasonPhrase => this.Monitor.Log($"Ignored {pack.Manifest.Name} > entry #{i}: {reasonPhrase}", LogLevel.Warn));
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading content pack '{pack.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                }
            }
        }

        /// <summary>Load one patch from a content pack's <c>content.json</c> file.</summary>
        /// <param name="pack">The content pack being loaded.</param>
        /// <param name="entry">The change to load.</param>
        /// <param name="config">The content pack's config values.</param>
        /// <param name="logSkip">The callback to invoke with the error reason if loading it fails.</param>
        private void LoadPatch(IContentPack pack, PatchConfig entry, IDictionary<string, ConfigField> config, Action<string> logSkip)
        {
            try
            {
                // skip if disabled
                if (!entry.Enabled)
                    return;

                // parse action
                if (!Enum.TryParse(entry.Action, out PatchType action))
                {
                    logSkip(string.IsNullOrWhiteSpace(entry.Action)
                        ? $"must set the {nameof(PatchConfig.Action)} field."
                        : $"invalid {nameof(PatchConfig.Action)} value '{entry.Action}', expected one of: {string.Join(", ", Enum.GetNames(typeof(PatchType)))}."
                    );
                    return;
                }

                // parse target asset
                string assetName = !string.IsNullOrWhiteSpace(entry.Target) ? this.Helper.Content.NormaliseAssetName(entry.Target) : null;
                if (assetName == null)
                {
                    logSkip($"must set the {nameof(PatchConfig.Target)} field.");
                    return;
                }

                // apply config
                foreach (string key in config.Keys)
                {
                    if (entry.When.TryGetValue(key, out string values))
                    {
                        InvariantHashSet expected = this.PatchManager.ParseCommaDelimitedField(values);
                        if (!expected.Intersect(config[key].Value).Any())
                            return;

                        entry.When.Remove(key);
                    }
                }

                // parse conditions
                ConditionDictionary conditions;
                {
                    if (!this.PatchManager.TryParseConditions(entry.When, out conditions, out string error))
                    {
                        logSkip($"the {nameof(PatchConfig.When)} field is invalid: {error}.");
                        return;
                    }
                }

                // parse & save patch
                switch (action)
                {
                    // load asset
                    case PatchType.Load:
                        {
                            // init patch
                            if (!this.TryPrepareLocalAsset(pack, entry.FromFile, config, conditions, logSkip, out TokenString fromAsset))
                                return;
                            IPatch patch = new LoadPatch(this.AssetLoader, pack, assetName, conditions, fromAsset);

                            // detect conflicting loaders
                            IPatch[] conflictingLoaders = this.PatchManager.GetConflictingLoaders(patch).ToArray();
                            if (conflictingLoaders.Any())
                            {
                                if (conflictingLoaders.Any(p => p.ContentPack == pack))
                                    logSkip($"the {assetName} file is already being loaded by this content pack. Each file can only be loaded once (unless their conditions can't overlap).");
                                else
                                {
                                    string[] conflictingNames = conflictingLoaders.Select(p => p.ContentPack.Manifest.Name).Distinct().OrderBy(p => p).ToArray();
                                    logSkip($"the {assetName} file is already being loaded by {(conflictingNames.Length == 1 ? "another content pack" : "other content packs")} ({string.Join(", ", conflictingNames)}). Each file can only be loaded once (unless their conditions can't overlap).");
                                }
                                return;
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
                                logSkip($"either {nameof(PatchConfig.Entries)} or {nameof(PatchConfig.Fields)} must be specified for a '{action}' change.");
                                return;
                            }
                            if (entry.Entries != null && entry.Entries.Any(p => string.IsNullOrWhiteSpace(p.Value)))
                            {
                                logSkip($"the {nameof(PatchConfig.Entries)} can't contain empty values.");
                                return;
                            }
                            if (entry.Fields != null && entry.Fields.Any(p => p.Value == null || p.Value.Any(n => n.Value == null)))
                            {
                                logSkip($"the {nameof(PatchConfig.Fields)} can't contain empty values.");
                                return;
                            }

                            // save
                            this.PatchManager.Add(new EditDataPatch(this.AssetLoader, pack, assetName, conditions, entry.Entries, entry.Fields, this.Monitor));
                        }
                        break;

                    // edit image
                    case PatchType.EditImage:
                        {
                            // read patch mode
                            PatchMode patchMode = PatchMode.Replace;
                            if (!string.IsNullOrWhiteSpace(entry.PatchMode) && !Enum.TryParse(entry.PatchMode, true, out patchMode))
                            {
                                logSkip($"the {nameof(PatchConfig.PatchMode)} is invalid. Expected one of these values: [{string.Join(", ", Enum.GetNames(typeof(PatchMode)))}].");
                                return;
                            }

                            // save
                            if (!this.TryPrepareLocalAsset(pack, entry.FromFile, config, conditions, logSkip, out TokenString fromAsset))
                                return;
                            this.PatchManager.Add(new EditImagePatch(this.AssetLoader, pack, assetName, conditions, fromAsset, entry.FromArea, entry.ToArea, patchMode, this.Monitor));
                        }
                        break;

                    default:
                        logSkip($"unsupported patch type '{action}'.");
                        break;
                }
            }
            catch (Exception ex)
            {
                logSkip($"error reading info. Technical details:\n{ex}");
            }
        }

        /// <summary>Prepare a local asset file for a patch to use.</summary>
        /// <param name="pack">The content pack being loaded.</param>
        /// <param name="path">The asset path in the content patch.</param>
        /// <param name="config">The config values to apply.</param>
        /// <param name="conditions">The conditions to apply.</param>
        /// <param name="logSkip">The callback to invoke with the error reason if loading it fails.</param>
        /// <param name="tokenedPath">The parsed value.</param>
        /// <returns>Returns whether the local asset was successfully prepared.</returns>
        private bool TryPrepareLocalAsset(IContentPack pack, string path, IDictionary<string, ConfigField> config, ConditionDictionary conditions, Action<string> logSkip, out TokenString tokenedPath)
        {
            // normalise raw value
            path = this.NormaliseLocalAssetPath(pack, path);
            if (path == null)
            {
                logSkip($"must set the {nameof(PatchConfig.FromFile)} field for this action type.");
                tokenedPath = null;
                return false;
            }

            // tokenise
            if (!TokenString.TryParse(path, config, out tokenedPath, out string error))
            {
                logSkip($"the {nameof(PatchConfig.FromFile)} is invalid: {error}");
                tokenedPath = null;
                return false;
            }

            // validate all possible files exist
            // + preload PNG assets to avoid load-in-draw-loop error
            string lastPermutation = null;
            try
            {
                foreach (string permutation in this.PatchManager.GetPermutations(tokenedPath, conditions))
                {
                    lastPermutation = permutation;
                    this.AssetLoader.PreloadIfNeeded(pack, permutation);
                }
            }
            catch (FileNotFoundException)
            {
                logSkip($"the {nameof(PatchConfig.FromFile)} field specifies a file that doesn't exist: {lastPermutation}.");
                tokenedPath = null;
                return false;
            }

            // looks OK
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
    }
}
