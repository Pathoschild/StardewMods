using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Patchers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ContentPatcher
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the file which contains patch metadata.</summary>
        private readonly string PatchFileName = "content.json";

        /// <summary>The asset loaders indexed by asset name.</summary>
        private readonly IDictionary<string, IList<LoadPatch>> Loaders = new Dictionary<string, IList<LoadPatch>>();

        /// <summary>The asset patchers indexed by asset name.</summary>
        private readonly IDictionary<string, IList<IPatch>> Patchers = new Dictionary<string, IList<IPatch>>();

        /// <summary>Handles the logic around loading assets from content packs.</summary>
        private readonly AssetLoader AssetLoader = new AssetLoader();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.LoadContentPacks();
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return
                this.Loaders.TryGetValue(asset.AssetName, out IList<LoadPatch> loaders)
                && loaders.Any(p => p.Locale == null || p.Locale.Equals(asset.Locale, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return this.Patchers.ContainsKey(asset.AssetName);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            if (!this.Loaders.TryGetValue(asset.AssetName, out IList<LoadPatch> loaders))
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because there are no replacements registered for that key.");

            foreach (LoadPatch loader in loaders)
            {
                if (loader.Locale != null && !asset.Locale.Equals(loader.Locale, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                return this.AssetLoader.Load<T>(loader.ContentPack, loader.LocalAsset);
            }
            throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because there are no replacements registered for the selected locale.");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (!this.Patchers.TryGetValue(asset.AssetName, out IList<IPatch> patches))
                throw new InvalidOperationException($"Can't edit asset key '{asset.AssetName}' because there are no patches registered for that key.");

            foreach (IPatch patch in patches)
            {
                if (patch.Locale != null && !asset.Locale.Equals(patch.Locale, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                patch.Apply<T>(asset);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Load the loaders and patchers from all registered content packs.</summary>
        private void LoadContentPacks()
        {
            foreach (IContentPack pack in this.Helper.GetContentPacks())
            {
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

                        // read action
                        string action = entry.Action?.Trim().ToLower();
                        if (string.IsNullOrWhiteSpace(action))
                        {
                            LogSkip($"must set the {nameof(PatchConfig.Action)} field.");
                            continue;
                        }

                        // read target asset
                        string assetName = !string.IsNullOrWhiteSpace(entry.Target)
                            ? this.Helper.Content.NormaliseAssetName(entry.Target)
                            : null;
                        if (assetName == null)
                        {
                            LogSkip($"must set the {nameof(PatchConfig.Target)} field.");
                            continue;
                        }

                        // read source asset
                        string localAsset = !string.IsNullOrWhiteSpace(entry.FromFile)
                            ? this.Helper.Content.NormaliseAssetName(entry.FromFile)
                            : null;
                        if (localAsset == null && (action == "load" || action == "editimage"))
                        {
                            LogSkip($"must set the {nameof(PatchConfig.FromFile)} field for a '{action}' patch.");
                            continue;
                        }
                        if (localAsset != null && !this.AssetLoader.AssetExists(pack, localAsset))
                        {
                            LogSkip($"the {nameof(PatchConfig.FromFile)} field specifies a file that doesn't exist: {localAsset}.");
                            continue;
                        }

                        // read locale
                        string locale = !string.IsNullOrWhiteSpace(entry.Locale)
                            ? entry.Locale.Trim().ToLower()
                            : null;

                        // parse for type
                        switch (action)
                        {
                            // load asset
                            case "load":
                                {
                                    // check for conflicting loaders
                                    if (this.Loaders.TryGetValue(assetName, out IList<LoadPatch> loaders))
                                    {
                                        // can't add for all locales if any loaders already registered
                                        if (locale == null)
                                        {
                                            string[] localesLoadedBy = loaders.Select(p => $"{p.ContentPack.Manifest.Name}").ToArray();
                                            LogSkip($"the {assetName} file is already being loaded by '{string.Join("', '", localesLoadedBy)}'. Each file can only be loaded once.");
                                            continue;
                                        }

                                        // can't add if already loaded for all locales
                                        {
                                            LoadPatch globalPatch = loaders.FirstOrDefault(p => p.Locale == null);
                                            if (globalPatch != null)
                                            {
                                                LogSkip($"the {assetName} file is already being loaded by {(pack == globalPatch.ContentPack ? "this content pack" : $"the '{globalPatch.ContentPack.Manifest.Name}' content pack")}. Each file can only be loaded once.");
                                                continue;
                                            }
                                        }

                                        // can't add if already loaded for selected locale
                                        {
                                            LoadPatch localePatch = loaders.FirstOrDefault(p => p.Locale != null && p.Locale.Equals(locale, StringComparison.CurrentCultureIgnoreCase));
                                            if (localePatch != null)
                                            {
                                                LogSkip($"the {assetName} file is already being loaded for the {locale} locale by {(pack == localePatch.ContentPack ? "this content pack" : $"the '{localePatch.ContentPack.Manifest.Name}' content pack")}. Each file can only be loaded once per locale.");
                                                continue;
                                            }
                                        }
                                    }

                                    // add loader
                                    this.AddLoaderOrPatcher(this.Loaders, assetName, new LoadPatch(pack, assetName, locale, localAsset));
                                }
                                break;

                            // edit data
                            case "editdata":
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
                                }

                                // save
                                this.AddLoaderOrPatcher(this.Patchers, assetName, new EditDataPatch(pack, assetName, locale, entry.Entries, entry.Fields, this.Monitor));
                                break;

                            // edit image
                            case "editimage":
                                this.AddLoaderOrPatcher(this.Patchers, assetName, new EditImagePatch(pack, assetName, locale, localAsset, entry.FromArea, entry.ToArea, this.Monitor, this.AssetLoader));
                                break;

                            default:
                                LogSkip($"unknown patch type '{action}'.");
                                break;
                        }

                        // preload PNG assets
                        if (localAsset != null && localAsset.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            this.AssetLoader.PreloadIfNeeded<Texture2D>(pack, localAsset);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"Ignored {pack.Manifest.Name} > entry #{i}: error reading info. Technical details:\n{ex}", LogLevel.Error);
                    }
                }
            }
        }

        /// <summary>Add a loader or patcher to the list.</summary>
        /// <param name="dict">The dictionary to update.</param>
        /// <param name="assetName">The normalised asset name.</param>
        /// <param name="patch">The patcher to apply.</param>
        private void AddLoaderOrPatcher<TPatcher>(IDictionary<string, IList<TPatcher>> dict, string assetName, TPatcher patch)
        {
            if (dict.TryGetValue(assetName, out IList<TPatcher> list))
                list.Add(patch);
            else
                dict[assetName] = new List<TPatcher> { patch };
        }
    }
}
