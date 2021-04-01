using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly SortedSet<IPatch> Patches = new(PatchIndexComparer.Instance);

        /// <summary>The patches to apply, indexed by token.</summary>
        private readonly InvariantDictionary<SortedSet<IPatch>> PatchesAffectedByToken = new();

        /// <summary>The patches to apply, indexed by asset name.</summary>
        private readonly InvariantDictionary<SortedSet<IPatch>> PatchesByCurrentTarget = new();

        /// <summary>The new patches which haven't received a context update yet.</summary>
        private readonly HashSet<IPatch> PendingPatches = new();

        /// <summary>Assets for which patches were removed, which should be reloaded on the next context update.</summary>
        private readonly InvariantHashSet AssetsWithRemovedPatches = new();

        /// <summary>The token changes queued for periodic update types.</summary>
        private readonly IDictionary<ContextUpdateType, InvariantHashSet> QueuedTokenChanges = new Dictionary<ContextUpdateType, InvariantHashSet>
        {
            [ContextUpdateType.OnTimeChange] = new(),
            [ContextUpdateType.OnLocationChange] = new(),
            [ContextUpdateType.All] = new()
        };


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
        /// <inheritdoc />
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
                        this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}");
                        break;

                    default:
                        this.Monitor.Log($"Multiple content packs want to load the '{asset.AssetName}' asset ({string.Join(", ", modNames)}). None will be applied. You should remove some of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                        this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}");
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

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            bool canEdit = this.GetCurrentEditors(asset).Any();
            this.Monitor.VerboseLog($"check: [{(canEdit ? "X" : " ")}] can edit {asset.AssetName}");
            return canEdit;
        }

        /// <inheritdoc />
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
                this.Monitor.Log($"{patch.ContentPack.Manifest.Name} loaded {asset.AssetName}.");

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

        /// <inheritdoc />
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
                    this.Monitor.Log($"{patch.ContentPack.Manifest.Name} edited {asset.AssetName}.");

                try
                {
                    patch.Edit<T>(asset);
                    patch.IsApplied = true;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Unhandled exception applying patch: {patch.Path}.\n{ex}", LogLevel.Error);
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
            this.Monitor.VerboseLog($"Updating context for {updateType} tick...");

            // Patches can have variable update rates, so we keep track of updated tokens here so
            // we update patches at their next update point.
            if (updateType == ContextUpdateType.All)
            {
                // all token updates apply at day start
                globalChangedTokens = new InvariantHashSet(globalChangedTokens);
                foreach (var queue in this.QueuedTokenChanges.Values)
                {
                    globalChangedTokens.AddMany(queue);
                    queue.Clear();
                }
            }
            else
            {
                // queue token changes for other update points
                foreach (KeyValuePair<ContextUpdateType, InvariantHashSet> pair in this.QueuedTokenChanges)
                {
                    if (pair.Key != updateType)
                        pair.Value.AddMany(globalChangedTokens);
                }

                // get queued changes for the current update point
                globalChangedTokens.AddMany(this.QueuedTokenChanges[updateType]);
                this.QueuedTokenChanges[updateType].Clear();
            }

            // run code patches
            // needs to come before next block, since it returns early
            foreach ( var patch in this.Patches )
            {
                if ( patch is CodePatch codePatch )
                {
                    this.Monitor.Log( "Checking code patch " + patch.Path + " " + patch.UpdateRate + " " + updateType );
                    if ( patch.UpdateRate.HasFlag( UpdateRate.OnDayStart ) && updateType == ContextUpdateType.All ||
                         patch.UpdateRate.HasFlag( UpdateRate.OnLocationChange ) && updateType == ContextUpdateType.OnLocationChange ||
                         patch.UpdateRate.HasFlag( UpdateRate.OnTimeChange ) && updateType == ContextUpdateType.OnTimeChange )
                    {
                        this.Monitor.Log( "Running code patch " + patch.Path );
                        try
                        {
                            codePatch.Run();
                        }
                        catch ( Exception e )
                        {
                            this.Monitor.Log( $"Exception when running patch {patch.Path}: {e}", LogLevel.Error );
                        }
                    }
                }
            }

            // get changes to apply
            HashSet<IPatch> patches = this.GetPatchesToUpdate(globalChangedTokens, updateType);
            InvariantHashSet reloadAssetNames = new InvariantHashSet(this.AssetsWithRemovedPatches);
            if (!patches.Any() && !reloadAssetNames.Any())
                return;

            // init for verbose logging
            List<PatchAuditChange> verbosePatchesReloaded = this.Monitor.IsVerbose
                ? new()
                : null;

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
                string wasFromAsset = patch.FromAsset;
                string wasTargetAsset = patch.TargetAsset;
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
                    this.Monitor.Log(ex.ToString());
                    changed = false;
                }
                bool isReady = patch.IsReady;

                // force reindex
                // This is only needed for EditData patches which use FromFile, since the loaded
                // file may include tokens which couldn't be analyzed when the patch was added.
                // This scenario was deprecated in Content Patcher 1.16, when Include patches were
                // added.
                if (patch.Type == PatchType.EditData && patch.FromAsset != wasFromAsset)
                {
                    this.RemovePatchFromIndexes(patch);
                    this.IndexPatch(patch, indexByToken: true);
                }

                // track patches to reload
                bool reloadAsset = isReady != wasReady || (isReady && changed);
                if (reloadAsset)
                {
                    patch.IsApplied = false;
                    if (wasReady)
                        reloadAssetNames.Add(wasTargetAsset);
                    if (isReady)
                        reloadAssetNames.Add(patch.TargetAsset);
                }

                // track for logging
                verbosePatchesReloaded?.Add(new PatchAuditChange(patch, wasReady, wasFromAsset, wasTargetAsset, reloadAsset));

                // log change
                if (this.Monitor.IsVerbose)
                {
                    IList<string> changes = new List<string>();
                    if (wasReady != isReady)
                        changes.Add(isReady ? "enabled" : "disabled");
                    if (wasTargetAsset != patch.TargetAsset)
                        changes.Add($"target: {wasTargetAsset} => {patch.TargetAsset}");
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

            // log changes
            if (verbosePatchesReloaded?.Count > 0)
            {
                StringBuilder report = new StringBuilder();
                report.AppendLine($"{verbosePatchesReloaded.Count} patches were rechecked for {updateType} tick.");

                foreach (PatchAuditChange entry in verbosePatchesReloaded.OrderBy(p => p.Patch.Path.ToString()))
                {
                    var patch = entry.Patch;

                    List<string> notes = new List<string>();

                    if (entry.WillInvalidate)
                    {
                        var assetNames = new[] { entry.WasTargetAsset, patch.TargetAsset }
                            .Select(p => p?.Trim())
                            .Where(p => !string.IsNullOrEmpty(p))
                            .Distinct(StringComparer.OrdinalIgnoreCase);
                        notes.Add($"invalidates {string.Join(", ", assetNames.OrderBy(p => p))}");
                    }

                    if (entry.WasReady != patch.IsReady)
                        notes.Add(patch.IsReady ? "=> ready" : "=> not ready");
                    if (entry.WasFromAsset != patch.FromAsset)
                        notes.Add($"{nameof(patch.FromAsset)} '{entry.WasFromAsset}' => '{patch.FromAsset}'");
                    if (entry.WasTargetAsset != patch.TargetAsset)
                        notes.Add($"{nameof(patch.TargetAsset)} '{entry.WasTargetAsset}' => '{patch.TargetAsset}'");

                    report.AppendLine($"   - {patch.Type} {patch.Path}");
                    foreach (string note in notes)
                        report.AppendLine($"      - {note}");
                }

                this.Monitor.Log(report.ToString());
            }

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
            // reset
            this.PatchesByCurrentTarget.Clear();
            if (patchListChanged)
                this.PatchesAffectedByToken.Clear();

            // reindex
            foreach (IPatch patch in this.Patches)
                this.IndexPatch(patch, indexByToken: patchListChanged);
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
            if (this.PatchesByCurrentTarget.TryGetValue(assetName, out SortedSet<IPatch> patches))
                return patches;
            return new IPatch[0];
        }

        /// <summary>Get all valid patches grouped by their current target value.</summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<IPatch>>> GetPatchesByTarget()
        {
            foreach (KeyValuePair<string, SortedSet<IPatch>> pair in this.PatchesByCurrentTarget)
                yield return new KeyValuePair<string, IEnumerable<IPatch>>(pair.Key, pair.Value);
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
                if (this.PatchesAffectedByToken.TryGetValue(tokenName, out SortedSet<IPatch> affectedPatches))
                {
                    foreach (IPatch patch in affectedPatches)
                    {
                        if (updateType == ContextUpdateType.All || patch.UpdateRate.HasFlag((UpdateRate)updateType))
                            patches.Add(patch);
                    }
                }
            }

            // add uninitialized patches
            patches.AddMany(this.PendingPatches);

            return patches;
        }

        /// <summary>Add a patch to the lookup indexes.</summary>
        /// <param name="patch">The patch to index.</param>
        /// <param name="indexByToken">Whether to also index by tokens used.</param>
        private void IndexPatch(IPatch patch, bool indexByToken)
        {
            // index by target asset
            if (patch.TargetAsset != null)
            {
                if (!this.PatchesByCurrentTarget.TryGetValue(patch.TargetAsset, out SortedSet<IPatch> list))
                    this.PatchesByCurrentTarget[patch.TargetAsset] = list = new SortedSet<IPatch>(PatchIndexComparer.Instance);
                list.Add(patch);
            }

            // index by tokens used
            if (indexByToken)
            {
                void IndexForToken(string tokenName)
                {
                    if (!this.PatchesAffectedByToken.TryGetValue(tokenName, out SortedSet<IPatch> affected))
                        this.PatchesAffectedByToken[tokenName] = affected = new SortedSet<IPatch>(PatchIndexComparer.Instance);
                    affected.Add(patch);
                }

                // get direct tokens
                InvariantHashSet tokensUsed = new InvariantHashSet(patch.GetTokensUsed());
                foreach (string tokenName in tokensUsed)
                    IndexForToken(tokenName);

                // get indirect tokens
                ModTokenContext modContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);
                foreach (IToken token in this.TokenManager.GetTokens(enforceContext: false))
                {
                    if (!tokensUsed.Contains(token.Name) && modContext.GetTokensAffectedBy(token.Name).Any(name => tokensUsed.Contains(name)))
                        IndexForToken(token.Name);
                }
            }
        }

        /// <summary>Whether to remove a patch from the indexes. Normally the indexes should be cleared and rebuilt instead; this method should only be used when forcefully reindexing a patch, which is only needed if it couldn't be analyzed when the patch was added, which in turn should only be the case for <see cref="PatchType.EditData"/> patches which use <see cref="PatchConfig.FromFile"/> (which was deprecated in Content Patcher 1.16).</summary>
        /// <param name="patch">The patch to remove.</param>
        private void RemovePatchFromIndexes(IPatch patch)
        {
            foreach (var lookup in new[] { this.PatchesByCurrentTarget, this.PatchesAffectedByToken })
            {
                foreach (string key in lookup.Keys.ToArray())
                {
                    ISet<IPatch> list = lookup[key];
                    if (list.Contains(patch))
                    {
                        list.Remove(patch);
                        if (!list.Any())
                            lookup.Remove(key);
                    }
                }
            }
        }
    }
}
