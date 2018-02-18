using System;
using System.Collections.Generic;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Patchers;
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
        private readonly IDictionary<string, LoadPatch> Loaders = new Dictionary<string, LoadPatch>();

        /// <summary>The asset patchers indexed by asset name.</summary>
        private readonly IDictionary<string, IList<IPatch>> Patchers = new Dictionary<string, IList<IPatch>>();


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
            return this.Loaders.ContainsKey(asset.AssetName);
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
            if (!this.Loaders.TryGetValue(asset.AssetName, out LoadPatch patch))
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because there are no replacements registered for that key.");

            return patch.ContentPack.LoadAsset<T>(patch.LocalAsset);
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (!this.Patchers.TryGetValue(asset.AssetName, out IList<IPatch> patches))
                throw new InvalidOperationException($"Can't edit asset key '{asset.AssetName}' because there are no patches registered for that key.");

            foreach (IPatch patch in patches)
                patch.Apply<T>(asset);
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
                PatchConfig[] config = pack.ReadJsonFile<PatchConfig[]>(this.PatchFileName);
                if (config == null)
                {
                    this.Monitor.Log($"Ignored content pack '{pack.Manifest.Name}' because it has no {this.PatchFileName} file.", LogLevel.Warn);
                    continue;
                }

                // load patches
                int i = 0;
                foreach (PatchConfig entry in config)
                {
                    i++;
                    void LogSkip(string reasonPhrase) => this.Monitor.Log($"Ignored {pack.Manifest.Name} > entry #{i}: {reasonPhrase}", LogLevel.Warn);

                    try
                    {
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

                        // parse for type
                        switch (action)
                        {
                            // load asset
                            case "load":
                                // make sure only one loader is set per file
                                if (this.Loaders.TryGetValue(assetName, out LoadPatch prevPatch))
                                {
                                    LogSkip($"the {assetName} file is already being loaded by {(pack == prevPatch.ContentPack ? "this content pack" : $"the '{prevPatch.ContentPack.Manifest.Name}' content pack")}. Each file can only be loaded once.");
                                    continue;
                                }

                                // save
                                this.Loaders.Add(assetName, new LoadPatch(pack, assetName, localAsset));
                                break;

                            // edit image
                            case "editimage":
                                this.AddPatch(assetName, new EditImagePatch(pack, assetName, localAsset, entry.FromArea, entry.ToArea, this.Monitor));
                                break;

                            default:
                                LogSkip($"unknown patch type '{action}'.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"Ignored {pack.Manifest.Name} > entry #{i}: error reading info. Technical details:\n{ex}", LogLevel.Error);
                    }
                }
            }
        }

        /// <summary>Add a patcher to the <see cref="Patchers"/> list.</summary>
        /// <param name="assetName">The normalised asset name.</param>
        /// <param name="patch">The patcher to apply.</param>
        private void AddPatch(string assetName, IPatch patch)
        {
            if (this.Patchers.TryGetValue(assetName, out IList<IPatch> list))
                list.Add(patch);
            else
                this.Patchers[assetName] = new List<IPatch> { patch };
        }
    }
}
