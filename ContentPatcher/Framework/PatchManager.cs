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

        /// <summary>The patches to apply, indexed by token.</summary>
        private readonly InvariantDictionary<HashSet<IPatch>> PatchesAffectedByToken = new InvariantDictionary<HashSet<IPatch>>();

        /// <summary>The patches to apply, indexed by asset name.</summary>
        private InvariantDictionary<HashSet<IPatch>> PatchesByCurrentTarget = new InvariantDictionary<HashSet<IPatch>>();

        /// <summary>The new patches which haven't received a context update yet.</summary>
        private readonly HashSet<IPatch> PendingPatches = new HashSet<IPatch>();

        /// <summary>Assets for which patches were removed, which should be reloaded on the next context update.</summary>
        private readonly InvariantHashSet AssetsWithRemovedPatches = new InvariantHashSet();

        /// <summary>The tokens which changed since the last daily update.</summary>
        private readonly InvariantHashSet QueuedDailyTokenChanges = new InvariantHashSet();


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
            // get load patches
            IPatch[] patches = this.GetCurrentLoaders(asset).ToArray();

            // validate
            if (patches.Length > 1)
            {
                // show simple error for most common cases
                string[] modNames = patches.Select(p => p.ContentPack.Manifest.Name).Distinct().OrderByIgnoreCase(p => p).ToArray();
                string[] patchNames = patches.Select(p => p.Path.ToString()).OrderByIgnoreCase(p => p).ToArray();
                switch (modNames.Length)
                {
                    case 1:
                        this.Monitor.Log($"'{modNames[0]}' has multiple patches which load the '{asset.AssetName}' asset at the same time ({string.Join(", ", patchNames)}). None will be applied. You should report this to the content pack author.", LogLevel.Error);
                        break;

                    case 2:
                        this.Monitor.Log($"Two content packs want to load the '{asset.AssetName}' asset ({string.Join(" and ", modNames)}). Neither will be applied. You should remove one of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                        this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}", LogLevel.Trace);
                        break;

                    default:
                        this.Monitor.Log($"Multiple content packs want to load the '{asset.AssetName}' asset ({string.Join(", ", modNames)}). None will be applied. You should remove some of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                        this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}", LogLevel.Trace);
                        break;
                }
                return false;
            }
            if (patches.Length == 1 && !patches[0].FromAssetExists())
            {
                this.Monitor.Log($"Can't apply load \"{patches[0].Path}\" to {patches[0].TargetAsset}: the {nameof(PatchConfig.FromFile)} file '{patches[0].FromAsset}' doesn't exist.", LogLevel.Warn);
                return false;
            }

            // return result
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
                throw new InvalidOperationException($"Can't load asset key '{asset.AssetName}' because multiple patches apply ({string.Join(", ", from entry in patches orderby entry.Path select entry.Path)}). This should never happen because it means validation failed.");

            // apply patch
            IPatch patch = patches.Single();
            if (this.Monitor.IsVerbose)
                this.Monitor.VerboseLog($"Patch \"{patch.Path}\" loaded {asset.AssetName}.");
            else
                this.Monitor.Log($"{patch.ContentPack.Manifest.Name} loaded {asset.AssetName}.", LogLevel.Trace);

            T data = patch.Load<T>(asset);

            foreach (IAssetValidator validator in this.AssetValidators)
            {
                if (!validator.TryValidate(asset, data, patch, out string error))
                {
                    this.Monitor.Log($"Can't apply patch {patch.Path} to {asset.AssetName}: {error}.", LogLevel.Error);
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
                    this.Monitor.VerboseLog($"Applied patch \"{patch.Path}\" to {asset.AssetName}.");
                else if (loggedContentPacks.Add(patch.ContentPack.Manifest.Name))
                    this.Monitor.Log($"{patch.ContentPack.Manifest.Name} edited {asset.AssetName}.", LogLevel.Trace);

                try
                {
                    patch.Edit<T>(asset);
                    patch.IsApplied = true;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"unhandled exception applying patch: {patch.Path}.\n{ex}", LogLevel.Error);
                    patch.IsApplied = false;
                }
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        /// <param name="globalChangedTokens">The global token values which changed.</param>
        /// <param name="updateType">The context update type.</param>
        public void UpdateContext(IContentHelper contentHelper, InvariantHashSet globalChangedTokens, ContextUpdateType updateType)
        {
            this.Monitor.VerboseLog("Propagating context...");

            // track token changes for the next start-of-day update
            if (updateType.HasFlag((ContextUpdateType)UpdateRate.OnDayStart))
            {
                globalChangedTokens = new InvariantHashSet(globalChangedTokens);
                foreach (string token in this.QueuedDailyTokenChanges)
                    globalChangedTokens.Add(token);
                this.QueuedDailyTokenChanges.Clear();
            }
            else
            {
                foreach (string token in globalChangedTokens)
                    this.QueuedDailyTokenChanges.Add(token);
            }

            // get changes to apply
            HashSet<IPatch> patches = this.GetPatchesToUpdate(globalChangedTokens, updateType);
            InvariantHashSet reloadAssetNames = new InvariantHashSet(this.AssetsWithRemovedPatches);
            if (!patches.Any() && !reloadAssetNames.Any())
                return;

            // update patches
            string prevAssetName = null;
            foreach (IPatch patch in patches)
            {
                // log asset name
                if (this.Monitor.IsVerbose && prevAssetName != patch.TargetAsset)
                {
                    this.Monitor.VerboseLog($"   {patch.TargetAsset}:");
                    prevAssetName = patch.TargetAsset;
                }

                // track old values
                string wasAssetName = patch.TargetAsset;
                bool wasReady = patch.IsReady && !this.PendingPatches.Contains(patch);

                // update patch
                IContext tokenContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);
                bool changed;
                try
                {
                    changed = patch.UpdateContext(tokenContext);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Patch error: {patch.Path} failed on context update (see log file for details).\n{ex.Message}", LogLevel.Error);
                    this.Monitor.Log(ex.ToString(), LogLevel.Trace);
                    changed = false;
                }
                bool isReady = patch.IsReady;

                // track patches to reload
                bool reloadAsset = isReady != wasReady || (isReady && changed);
                if (reloadAsset)
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

                    this.Monitor.VerboseLog($"      [{(isReady ? "X" : " ")}] {patch.Path}: {(changes.Any() ? changesStr : "OK")}");
                }

                // warn for invalid load patch
                // (Other patch types show an error when applied, but that's not possible for a load patch since we can't cleanly abort a load.)
                if (patch is LoadPatch loadPatch && patch.IsReady && !patch.FromAssetExists())
                    this.Monitor.Log($"Patch error: {patch.Path} has a {nameof(PatchConfig.FromFile)} which matches non-existent file '{loadPatch.FromAsset}'.", LogLevel.Error);
            }

            // reset indexes
            this.PendingPatches.Clear();
            this.AssetsWithRemovedPatches.Clear();
            this.Reindex(patchListChanged: false);

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

        /****
        ** Patches
        ****/
        /// <summary>Add a patch.</summary>
        /// <param name="patch">The patch to add.</param>
        /// <param name="reindex">Whether to reindex the patch list immediately.</param>
        public void Add(IPatch patch, bool reindex = true)
        {
            // set initial context
            ModTokenContext modContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);
            patch.UpdateContext(modContext);

            // add to patch list
            this.Monitor.VerboseLog($"      added {patch.Type} {patch.TargetAsset}.");
            this.Patches.Add(patch);
            this.PendingPatches.Add(patch);

            // rebuild indexes
            if (reindex)
                this.Reindex(patchListChanged: true);
        }

        /// <summary>Remove a patch.</summary>
        /// <param name="patch">The patch to remove.</param>
        /// <param name="reindex">Whether to reindex the patch list immediately.</param>
        public void Remove(IPatch patch, bool reindex = true)
        {
            // remove from patch list
            this.Monitor.VerboseLog($"      removed {patch.Path}.");
            if (!this.Patches.Remove(patch))
                return;

            // mark asset to reload
            if (patch.IsApplied)
                this.AssetsWithRemovedPatches.Add(patch.TargetAsset);

            // rebuild indexes
            if (reindex)
                this.Reindex(patchListChanged: true);
        }

        /// <summary>Rebuild the internal patch lookup indexes. This should only be called manually if patches were added/removed with the reindex option disabled.</summary>
        /// <param name="patchListChanged">Whether patches were added or removed.</param>
        public void Reindex(bool patchListChanged)
        {
            // rebuild target asset lookup
            this.PatchesByCurrentTarget = new InvariantDictionary<HashSet<IPatch>>(
                from patchGroup in this.Patches.GroupByIgnoreCase(p => p.TargetAsset)
                where patchGroup.Key != null // ignore include tokens for target lookups
                select new KeyValuePair<string, HashSet<IPatch>>(patchGroup.Key, new HashSet<IPatch>(patchGroup))
            );

            // rebuild affected-by-tokens lookup
            if (patchListChanged)
            {
                foreach (IPatch patch in this.Patches)
                {
                    ModTokenContext modContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);

                    InvariantHashSet tokensUsed = new InvariantHashSet(patch.GetTokensUsed());
                    foreach (string tokenName in tokensUsed)
                        this.TrackPatchAffectedByToken(patch, tokenName);
                    foreach (IToken token in this.TokenManager.GetTokens(enforceContext: false))
                    {
                        if (!tokensUsed.Contains(token.Name) && modContext.GetTokensAffectedBy(token.Name).Any(name => tokensUsed.Contains(name)))
                            this.TrackPatchAffectedByToken(patch, token.Name);
                    }
                }
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
        /// <summary>Track that a given token may cause the patch to update.</summary>
        /// <param name="patch">The affected patch.</param>
        /// <param name="tokenName">The token name.</param>
        private void TrackPatchAffectedByToken(IPatch patch, string tokenName)
        {
            if (!this.PatchesAffectedByToken.TryGetValue(tokenName, out HashSet<IPatch> affected))
                this.PatchesAffectedByToken[tokenName] = affected = new HashSet<IPatch>(new ObjectReferenceComparer<IPatch>());
            affected.Add(patch);
        }

        /// <summary>Get the patch type which applies when editing a given asset type.</summary>
        /// <param name="assetType">The asset type.</param>
        private PatchType? GetEditType(Type assetType)
        {
            if (typeof(Texture2D).IsAssignableFrom(assetType))
                return PatchType.EditImage;
            if (typeof(Map).IsAssignableFrom(assetType))
                return PatchType.EditMap;
            else
                return PatchType.EditData;
        }

        /// <summary>Get the tokens which need a context update.</summary>
        /// <param name="globalChangedTokens">The global token values which changed.</param>
        /// <param name="updateType">The context update type.</param>
        private HashSet<IPatch> GetPatchesToUpdate(InvariantHashSet globalChangedTokens, ContextUpdateType updateType)
        {
            // add patches which depend on a changed token
            var patches = new HashSet<IPatch>(new ObjectReferenceComparer<IPatch>());
            foreach (string tokenName in globalChangedTokens)
            {
                if (this.PatchesAffectedByToken.TryGetValue(tokenName, out HashSet<IPatch> affectedPatches))
                {
                    foreach (IPatch patch in affectedPatches)
                    {
                        if (updateType.HasFlag((ContextUpdateType)patch.UpdateRate))
                            patches.Add(patch);
                    }
                }
            }

            // add uninitialized patches
            foreach (var patch in this.PendingPatches)
                patches.Add(patch);

            return patches;
        }
    }
}
