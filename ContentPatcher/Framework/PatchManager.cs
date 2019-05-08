using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Validators;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using xTile;

namespace ContentPatcher.Framework
{
    /// <summary>Manages loaded patches.</summary>
    internal class PatchManager : IAssetLoader, IAssetEditor
    {
        /*********
        ** Fields
        *********/
        /****
        ** State
        ****/
        /// <summary>Manages the available contextual tokens.</summary>
        private readonly TokenManager TokenManager;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Handle special validation logic on loaded or edited assets.</summary>
        private readonly IAssetValidator[] AssetValidators;

        /// <summary>The patches which are permanently disabled for this session.</summary>
        private readonly IList<DisabledPatch> PermanentlyDisabledPatches = new List<DisabledPatch>();

        /// <summary>The patches to apply.</summary>
        private readonly HashSet<IPatch> Patches = new HashSet<IPatch>();

        /// <summary>The patches to apply, indexed by asset name.</summary>
        private InvariantDictionary<HashSet<IPatch>> PatchesByCurrentTarget = new InvariantDictionary<HashSet<IPatch>>();

        /// <summary>The patches to apply, indexed by token.</summary>
        private InvariantDictionary<HashSet<IPatch>> PatchesByToken = new InvariantDictionary<HashSet<IPatch>>();
        

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="tokenManager">Manages the available contextual tokens.</param>
        /// <param name="assetValidators">Handle special validation logic on loaded or edited assets.</param>
        public PatchManager(IMonitor monitor, TokenManager tokenManager, IAssetValidator[] assetValidators)
        {
            this.Monitor = monitor;
            this.TokenManager = tokenManager;
            this.AssetValidators = assetValidators;
        }

        /****
        ** Patching
        ****/
        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            IPatch[] patches = this.GetCurrentLoaders(asset).ToArray();
            if (patches.Length > 1)
            {
                this.Monitor.Log($"Multiple patches want to load {asset.AssetName} ({string.Join(", ", from entry in patches orderby entry.LogName select entry.LogName)}). None will be applied.", LogLevel.Error);
                return false;
            }

            bool canLoad = patches.Any();
            this.Monitor.VerboseLog($"check: [{(canLoad ? "X" : " ")}] can load {asset.AssetName}");
            return canLoad;
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            bool canEdit = this.GetCurrentEditors(asset).Any();
            this.Monitor.VerboseLog($"check: [{(canEdit ? "X" : " ")}] can edit {asset.AssetName}");
            return canEdit;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            // get applicable patches for context
            IPatch[] patches = this.GetCurrentLoaders(asset).ToArray();
            if (!patches.Any())
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because no patches currently apply. This should never happen because it means validation failed.");
            if (patches.Length > 1)
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because multiple patches apply ({string.Join(", ", from entry in patches orderby entry.LogName select entry.LogName)}). This should never happen because it means validation failed.");

            // apply patch
            IPatch patch = patches.Single();
            if (this.Monitor.IsVerbose)
                this.Monitor.VerboseLog($"Patch \"{patch.LogName}\" loaded {asset.AssetName}.");
            else
                this.Monitor.Log($"{patch.ContentPack.Manifest.Name} loaded {asset.AssetName}.", LogLevel.Trace);

            T data = patch.Load<T>(asset);

            foreach (IAssetValidator validator in this.AssetValidators)
            {
                if (!validator.TryValidate(asset, data, patch, out string error))
                {
                    this.Monitor.Log($"Can't apply patch {patch.LogName} to {asset.AssetName}: {error}.", LogLevel.Error);
                    return default;
                }
            }

            patch.IsApplied = true;
            return data;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            IPatch[] patches = this.GetCurrentEditors(asset).ToArray();
            if (!patches.Any())
                throw new InvalidOperationException($"Can't edit asset key '{asset.AssetName}' because no patches currently apply. This should never happen.");

            InvariantHashSet loggedContentPacks = new InvariantHashSet();
            foreach (IPatch patch in patches)
            {
                if (this.Monitor.IsVerbose)
                    this.Monitor.VerboseLog($"Applied patch \"{patch.LogName}\" to {asset.AssetName}.");
                else if (loggedContentPacks.Add(patch.ContentPack.Manifest.Name))
                    this.Monitor.Log($"{patch.ContentPack.Manifest.Name} edited {asset.AssetName}.", LogLevel.Trace);

                try
                {
                    patch.Edit<T>(asset);
                    patch.IsApplied = true;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"unhandled exception applying patch: {patch.LogName}.\n{ex}", LogLevel.Error);
                    patch.IsApplied = false;
                }
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        public void UpdateContext(IContentHelper contentHelper)
        {
            this.Monitor.VerboseLog("Propagating context...");
            UpdateContextImpl(contentHelper, this.Patches);
        }

        /// <summary>Update the current context, checking only specific tokens.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        /// <param name="tokens">The tokens to check with.</param>
        public void UpdateSpecificContext(IContentHelper contentHelper, InvariantHashSet tokens)
        {
            this.Monitor.VerboseLog("Propagating specific context...");

            // collect more tokens
            foreach ( string token in tokens.ToArray() )
            {
                if (!this.TokenManager.BasicTokensUsedBy.TryGetValue(token, out InvariantHashSet moreTokens))
                    continue;
                foreach (string extraToken in moreTokens )
                {
                    if (!tokens.Contains(extraToken))
                        tokens.Add(extraToken);
                }
            }

            // collect patches
            IEnumerable<string> tokPatches = this.PatchesByToken.Keys.Intersect(tokens);
            IEnumerable<KeyValuePair<string, HashSet<IPatch>>> patchesSets = this.PatchesByToken.Where(p => tokPatches.Contains(p.Key));
            IEnumerable<IPatch> patches = new HashSet<IPatch>();
            foreach (KeyValuePair<string, HashSet<IPatch>> patchesSet in patchesSets)
                patches = patches.Union(patchesSet.Value);

            // update patches
            UpdateContextImpl(contentHelper, patches);
        }

        /****
        ** Patches
        ****/
        /// <summary>Add a patch.</summary>
        /// <param name="patch">The patch to add.</param>
        public void Add(IPatch patch)
        {
            // set initial context
            IContext tokenContext = this.TokenManager.TrackLocalTokens(patch.ContentPack.Pack);
            patch.UpdateContext(tokenContext);

            // add to patch list
            this.Monitor.VerboseLog($"      added {patch.Type} {patch.TargetAsset}.");
            this.Patches.Add(patch);

            // add to lookup cache
            if (this.PatchesByCurrentTarget.TryGetValue(patch.TargetAsset, out HashSet<IPatch> patches))
                patches.Add(patch);
            else
                this.PatchesByCurrentTarget[patch.TargetAsset] = new HashSet<IPatch> { patch };

            // add to token cache
            foreach (Condition cond in patch.Conditions)
            {
                if (this.PatchesByToken.TryGetValue(cond.Name, out HashSet<IPatch> tokPatches))
                    tokPatches.Add(patch);
                else
                    this.PatchesByToken[cond.Name] = new HashSet<IPatch> { patch };
            }
            foreach (string tok in patch.GetTokensUsed())
            {
                if (this.PatchesByToken.TryGetValue(tok, out HashSet<IPatch> tokPatches))
                    tokPatches.Add(patch);
                else
                    this.PatchesByToken[tok] = new HashSet<IPatch> { patch };
            }
        }

        /// <summary>Add a patch that's permanently disabled for this session.</summary>
        /// <param name="patch">The patch to add.</param>
        public void AddPermanentlyDisabled(DisabledPatch patch)
        {
            this.PermanentlyDisabledPatches.Add(patch);
        }

        /// <summary>Get valid patches regardless of context.</summary>
        public IEnumerable<IPatch> GetPatches()
        {
            return this.Patches;
        }

        /// <summary>Get valid patches regardless of context.</summary>
        /// <param name="assetName">The asset name for which to find patches.</param>
        public IEnumerable<IPatch> GetPatches(string assetName)
        {
            if (this.PatchesByCurrentTarget.TryGetValue(assetName, out HashSet<IPatch> patches))
                return patches;
            return new IPatch[0];
        }

        /// <summary>Get patches which are permanently disabled for this session, along with the reason they were.</summary>
        public IEnumerable<DisabledPatch> GetPermanentlyDisabledPatches()
        {
            return this.PermanentlyDisabledPatches;
        }

        /// <summary>Get patches which load the given asset in the current context.</summary>
        /// <param name="asset">The asset being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentLoaders(IAssetInfo asset)
        {
            return this
                .GetPatches(asset.AssetName)
                .Where(patch => patch.Type == PatchType.Load && patch.IsReady);
        }

        /// <summary>Get patches which edit the given asset in the current context.</summary>
        /// <param name="asset">The asset being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentEditors(IAssetInfo asset)
        {
            PatchType? patchType = this.GetEditType(asset.DataType);
            if (patchType == null)
                return new IPatch[0];

            return this
                .GetPatches(asset.AssetName)
                .Where(patch => patch.Type == patchType && patch.IsReady);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get the patch type which applies when editing a given asset type.</summary>
        /// <param name="assetType">The asset type.</param>
        private PatchType? GetEditType(Type assetType)
        {
            if (assetType == typeof(Texture2D))
                return PatchType.EditImage;
            if (assetType == typeof(Map))
                return null;
            else
                return PatchType.EditData;
        }

        /// <summary>Implementation of updating the context.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        public void UpdateContextImpl(IContentHelper contentHelper, IEnumerable<IPatch> patches)
        {
            // update patches
            InvariantHashSet reloadAssetNames = new InvariantHashSet();
            string prevAssetName = null;
            foreach (IPatch patch in patches.OrderByIgnoreCase(p => p.TargetAsset).ThenByIgnoreCase(p => p.LogName))
            {
                // log asset name
                if (this.Monitor.IsVerbose && prevAssetName != patch.TargetAsset)
                {
                    this.Monitor.VerboseLog($"   {patch.TargetAsset}:");
                    prevAssetName = patch.TargetAsset;
                }

                // track old values
                string wasAssetName = patch.TargetAsset;
                bool wasReady = patch.IsReady;

                // update patch
                IContext tokenContext = this.TokenManager.TrackLocalTokens(patch.ContentPack.Pack);
                bool changed = patch.UpdateContext(tokenContext);
                bool isReady = patch.IsReady;

                // track patches to reload
                bool reload = (wasReady && changed) || (!wasReady && isReady);
                if (reload)
                {
                    patch.IsApplied = false;
                    if (wasReady)
                        reloadAssetNames.Add(wasAssetName);
                    if (isReady)
                        reloadAssetNames.Add(patch.TargetAsset);
                }

                // log change
                if (this.Monitor.IsVerbose)
                {
                    IList<string> changes = new List<string>();
                    if (wasReady != isReady)
                        changes.Add(isReady ? "enabled" : "disabled");
                    if (wasAssetName != patch.TargetAsset)
                        changes.Add($"target: {wasAssetName} => {patch.TargetAsset}");
                    string changesStr = string.Join(", ", changes);

                    this.Monitor.VerboseLog($"      [{(isReady ? "X" : " ")}] {patch.LogName}: {(changes.Any() ? changesStr : "OK")}");
                }

                // warn for invalid load patch
                if (patch is LoadPatch loadPatch && patch.IsReady && !patch.ContentPack.HasFile(loadPatch.FromLocalAsset.Value))
                    this.Monitor.Log($"Patch error: {patch.LogName} has a {nameof(PatchConfig.FromFile)} which matches non-existent file '{loadPatch.FromLocalAsset.Value}'.", LogLevel.Error);
            }

            // rebuild asset name lookup
            this.PatchesByCurrentTarget = new InvariantDictionary<HashSet<IPatch>>(
                from patchGroup in this.Patches.GroupByIgnoreCase(p => p.TargetAsset)
                let key = patchGroup.Key
                let value = new HashSet<IPatch>(patchGroup)
                select new KeyValuePair<string, HashSet<IPatch>>(key, value)
            );

            // reload assets if needed
            if (reloadAssetNames.Any())
            {
                this.Monitor.VerboseLog($"   reloading {reloadAssetNames.Count} assets: {string.Join(", ", reloadAssetNames.OrderByIgnoreCase(p => p))}");
                contentHelper.InvalidateCache(asset =>
                {
                    this.Monitor.VerboseLog($"      [{(reloadAssetNames.Contains(asset.AssetName) ? "X" : " ")}] reload {asset.AssetName}");
                    return reloadAssetNames.Contains(asset.AssetName);
                });
            }
        }
    }
}
