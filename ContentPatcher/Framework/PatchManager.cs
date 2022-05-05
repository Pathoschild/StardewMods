using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Validators;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using xTile;

namespace ContentPatcher.Framework
{
    /// <summary>Manages loaded patches.</summary>
    internal class PatchManager
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
        private readonly Dictionary<IAssetName, SortedSet<IPatch>> PatchesByCurrentTarget = new();

        /// <summary>The new patches which haven't received a context update yet.</summary>
        private readonly HashSet<IPatch> PendingPatches = new();

        /// <summary>Assets for which patches were removed, which should be reloaded on the next context update.</summary>
        private readonly HashSet<IAssetName> AssetsWithRemovedPatches = new();

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
        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="e">The event data.</param>
        /// <param name="ignoreLoadPatches">Whether to ignore any load patches for this asset.</param>
        public void OnAssetRequested(AssetRequestedEventArgs e, bool ignoreLoadPatches)
        {
            IAssetName assetName = e.NameWithoutLocale;
            IPatch[] loaders = !ignoreLoadPatches
                ? this.GetCurrentLoaders(assetName).ToArray()
                : Array.Empty<IPatch>();
            IPatch[] editors = this.GetCurrentEditors(assetName, e.DataType).ToArray();

            if (loaders.Any() || editors.Any())
            {
                MethodInfo apply = this
                    .GetType()
                    .GetMethod(nameof(this.ApplyPatchesToAsset), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(e.DataType);

                apply.Invoke(this, new object[] { e, loaders, editors });
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="contentHelper">The content helper through which to invalidate assets.</param>
        /// <param name="globalChangedTokens">The global token values which changed.</param>
        /// <param name="updateType">The context update type.</param>
        public void UpdateContext(IGameContentHelper contentHelper, InvariantHashSet globalChangedTokens, ContextUpdateType updateType)
        {
            this.Monitor.VerboseLog($"Updating context for {updateType} tick...");

            // Patches can have variable update rates, so we keep track of updated tokens here so
            // we update patches at their next update point.
            if (updateType == ContextUpdateType.All)
            {
                // all token updates apply at day start
                globalChangedTokens = new InvariantHashSet(globalChangedTokens);
                foreach (var tokenQueue in this.QueuedTokenChanges.Values)
                {
                    globalChangedTokens.AddMany(tokenQueue);
                    tokenQueue.Clear();
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

            // get changes to apply
            Queue<IPatch> patchQueue = new Queue<IPatch>(this.GetPatchesToUpdate(globalChangedTokens, updateType));
            ISet<IAssetName> reloadAssetNames = new HashSet<IAssetName>(this.AssetsWithRemovedPatches);
            if (!patchQueue.Any() && !reloadAssetNames.Any())
                return;

            // reset queued patches
            // This needs to be done *before* we update patches, to avoid clearing patches added by Include patches
            IPatch[] wasPending = this.PendingPatches.ToArray();
            this.PendingPatches.Clear();
            this.AssetsWithRemovedPatches.Clear();

            // init for verbose logging
            List<PatchAuditChange>? verbosePatchesReloaded = this.Monitor.IsVerbose
                ? new()
                : null;

            // update patches
            IAssetName? prevAssetName = null;
            HashSet<IPatch> newPatches = new(new ObjectReferenceComparer<IPatch>());
            while (patchQueue.Any())
            {
                IPatch patch = patchQueue.Dequeue();

                // log asset name
                if (this.Monitor.IsVerbose && prevAssetName != patch.TargetAsset)
                {
                    this.Monitor.VerboseLog($"   {patch.TargetAsset}:");
                    prevAssetName = patch.TargetAsset;
                }

                // track old values
                string? wasFromAsset = patch.FromAsset;
                IAssetName? wasTargetAsset = patch.TargetAsset;
                bool wasReady = patch.IsReady && !wasPending.Contains(patch);

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

                // handle specific patch types
                switch (patch.Type)
                {
                    case PatchType.EditData:
                        // force reindex
                        // This is only needed for EditData patches which use FromFile, since the
                        // loaded file may include tokens which couldn't be analyzed when the patch
                        // was added. This scenario was deprecated in Content Patcher 1.16, when
                        // Include patches were added.
                        if (patch.FromAsset != wasFromAsset)
                        {
                            this.RemovePatchFromIndexes(patch);
                            this.IndexPatch(patch, indexByToken: true);
                        }
                        break;

                    case PatchType.Include:
                        // queue new patches
                        if (patch is IncludePatch { PatchesJustLoaded: not null } include)
                        {
                            foreach (IPatch includedPatch in include.PatchesJustLoaded)
                            {
                                newPatches.Add(includedPatch);
                                patchQueue.Enqueue(includedPatch);
                            }
                        }
                        break;

                    case PatchType.Load:
                        // warn for invalid load patch
                        // Other patch types show an error when they're applied instead, but that's
                        // not possible for a load patch since we can't cleanly abort a load.
                        if (patch is LoadPatch loadPatch && patch.IsReady && !patch.FromAssetExists())
                            this.Monitor.Log($"Patch error: {patch.Path} has a {nameof(PatchConfig.FromFile)} which matches non-existent file '{loadPatch.FromAsset}'.", LogLevel.Error);
                        break;
                }

                // if the patch was just added via Include, it implicitly changed too
                changed = changed || newPatches.Contains(patch);

                // track patches to reload
                bool reloadAsset = isReady != wasReady || (isReady && changed);
                if (reloadAsset)
                {
                    patch.IsApplied = false;
                    if (wasReady && wasTargetAsset != null)
                        reloadAssetNames.Add(wasTargetAsset);
                    if (isReady && patch.TargetAsset != null)
                        reloadAssetNames.Add(patch.TargetAsset);
                }

                // log change
                verbosePatchesReloaded?.Add(new PatchAuditChange(patch, wasReady, wasFromAsset, wasTargetAsset, reloadAsset));
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
            }

            // reset indexes
            this.Reindex(patchListChanged: false);

            // log changes
            if (verbosePatchesReloaded?.Count > 0)
            {
                StringBuilder report = new StringBuilder();
                report.AppendLine($"{verbosePatchesReloaded.Count} patches were rechecked for {updateType} tick.");

                foreach (PatchAuditChange entry in verbosePatchesReloaded.OrderByHuman(p => p.Patch.Path.ToString()))
                {
                    var patch = entry.Patch;

                    List<string> notes = new();

                    if (entry.WillInvalidate)
                    {
                        IEnumerable<IAssetName> assetNames = new[] { entry.WasTargetAsset, patch.TargetAsset }
                            .WhereNotNull()
                            .Distinct();
                        notes.Add($"invalidates {string.Join(", ", assetNames.Select(p => p.Name).OrderByHuman())}");
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
                if (this.Monitor.IsVerbose)
                    this.Monitor.VerboseLog($"   reloading {reloadAssetNames.Count} assets: {string.Join(", ", reloadAssetNames.OrderByHuman(p => p.Name))}");
                contentHelper.InvalidateCache(asset =>
                {
                    bool match = reloadAssetNames.Contains(asset.NameWithoutLocale);
                    this.Monitor.VerboseLog($"      [{(match ? "X" : " ")}] reload {asset.Name}");
                    return match;
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
            if (patch.IsApplied && patch.TargetAsset != null)
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
        public IEnumerable<IPatch> GetPatches(IAssetName assetName)
        {
            if (this.PatchesByCurrentTarget.TryGetValue(assetName, out SortedSet<IPatch>? patches))
                return patches;
            return Array.Empty<IPatch>();
        }

        /// <summary>Get all valid patches grouped by their current target value.</summary>
        public IEnumerable<KeyValuePair<IAssetName, IEnumerable<IPatch>>> GetPatchesByTarget()
        {
            foreach ((IAssetName assetName, ISet<IPatch> list) in this.PatchesByCurrentTarget)
                yield return new KeyValuePair<IAssetName, IEnumerable<IPatch>>(assetName, list);
        }

        /// <summary>Get patches which are permanently disabled for this session, along with the reason they were.</summary>
        public IEnumerable<DisabledPatch> GetPermanentlyDisabledPatches()
        {
            return this.PermanentlyDisabledPatches;
        }

        /// <summary>Get patches which load the given asset in the current context.</summary>
        /// <param name="assetName">The asset being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentLoaders(IAssetName assetName)
        {
            return this
                .GetPatches(assetName)
                .Where(patch => patch.Type == PatchType.Load && patch.IsReady);
        }

        /// <summary>Get patches which edit the given asset in the current context.</summary>
        /// <param name="assetName">The asset being intercepted.</param>
        /// <param name="dataType">The asset data type.</param>
        public IEnumerable<IPatch> GetCurrentEditors(IAssetName assetName, Type dataType)
        {
            PatchType? patchType = this.GetEditType(dataType);
            if (patchType == null)
                return Array.Empty<IPatch>();

            return this
                .GetPatches(assetName)
                .Where(patch => patch.Type == patchType && patch.IsReady);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Apply load and edit patches to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="e">The asset requested context.</param>
        /// <param name="loaders">The load patch to apply.</param>
        /// <param name="editors">The edit patch to apply.</param>
        private void ApplyPatchesToAsset<T>(AssetRequestedEventArgs e, IPatch[] loaders, IPatch[] editors)
            where T : notnull
        {
            IAssetName assetName = e.NameWithoutLocale;

            // pre-validate loaders to show more user-friendly messages
            if (loaders.Length > 1)
            {
                string[] modNames = loaders.Select(p => p.ContentPack.Manifest.Name).Distinct().OrderByHuman().ToArray();
                string[] patchNames = loaders.Select(p => p.Path.ToString()).OrderByHuman().ToArray();
                switch (modNames.Length)
                {
                    case 1:
                        this.Monitor.Log($"'{modNames[0]}' has multiple patches which load the '{assetName}' asset at the same time ({string.Join(", ", patchNames)}). None will be applied. You should report this to the content pack author.", LogLevel.Error);
                        break;

                    case 2:
                        this.Monitor.Log($"Two content packs want to load the '{assetName}' asset ({string.Join(" and ", modNames)}). Neither will be applied. You should remove one of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                        this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}");
                        break;

                    default:
                        this.Monitor.Log($"Multiple content packs want to load the '{assetName}' asset ({string.Join(", ", modNames)}). None will be applied. You should remove some of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                        this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}");
                        break;
                }

                loaders = Array.Empty<IPatch>();
            }
            else if (loaders.Length == 1 && !loaders[0].FromAssetExists())
            {
                this.Monitor.Log($"Can't apply load \"{loaders[0].Path}\" to {loaders[0].TargetAsset}: the {nameof(PatchConfig.FromFile)} file '{loaders[0].FromAsset}' doesn't exist.", LogLevel.Warn);
                loaders = Array.Empty<IPatch>();
            }

            // apply load patches
            if (loaders.Any())
            {
                foreach (IPatch patch in loaders)
                {
                    e.LoadFrom(
                        load: () => this.ApplyLoad<T>(patch, assetName)!,
                        priority: AssetLoadPriority.Exclusive,
                        onBehalfOf: patch.ContentPack.Manifest.UniqueID
                    );
                }
            }

            // apply edit patches
            if (editors.Any())
            {
                foreach (IPatch patch in editors)
                {
                    e.Edit(
                        apply: data => this.ApplyEdit<T>(patch, data),
                        priority: AssetEditPriority.Default,
                        onBehalfOf: patch.ContentPack.Manifest.UniqueID
                    );
                }
            }

            // log result
            if (this.Monitor.IsVerbose)
                this.Monitor.VerboseLog($"asset requested: can [{(loaders.Any() ? "X" : " ")}] load [{(editors.Any() ? "X" : " ")}] edit {assetName}");
        }

        /// <summary>Apply a load patch to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="patch">The patch to apply.</param>
        /// <param name="assetName">The asset name to load.</param>
        /// <returns>Returns the loaded asset data.</returns>
        private T? ApplyLoad<T>(IPatch patch, IAssetName assetName)
            where T : notnull
        {
            if (this.Monitor.IsVerbose)
                this.Monitor.VerboseLog($"Patch \"{patch.Path}\" loaded {assetName}.");

            T data = patch.Load<T>(assetName);

            foreach (IAssetValidator validator in this.AssetValidators)
            {
                if (!validator.TryValidate(assetName, data, patch, out string? error))
                {
                    this.Monitor.Log($"Can't apply patch {patch.Path} to {assetName}: {error}.", LogLevel.Error);
                    return default;
                }
            }

            patch.IsApplied = true;
            return data;
        }

        /// <summary>Apply an edit patch to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="patch">The patch to apply.</param>
        /// <param name="asset">The asset data to edit.</param>
        private void ApplyEdit<T>(IPatch patch, IAssetData asset)
            where T : notnull
        {
            if (this.Monitor.IsVerbose)
                this.Monitor.VerboseLog($"Applied patch \"{patch.Path}\" to {asset.Name}.");

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
                if (this.PatchesAffectedByToken.TryGetValue(tokenName, out SortedSet<IPatch>? affectedPatches))
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
                if (!this.PatchesByCurrentTarget.TryGetValue(patch.TargetAsset, out SortedSet<IPatch>? list))
                    this.PatchesByCurrentTarget[patch.TargetAsset] = list = new SortedSet<IPatch>(PatchIndexComparer.Instance);
                list.Add(patch);
            }

            // index by tokens used
            if (indexByToken)
            {
                void IndexForToken(string tokenName)
                {
                    if (!this.PatchesAffectedByToken.TryGetValue(tokenName, out SortedSet<IPatch>? affected))
                        this.PatchesAffectedByToken[tokenName] = affected = new SortedSet<IPatch>(PatchIndexComparer.Instance);
                    affected.Add(patch);
                }

                // get mod context
                ModTokenContext modContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);

                // get direct tokens
                InvariantHashSet tokensUsed = new InvariantHashSet(patch.GetTokensUsed().Select(name => this.TokenManager.ResolveAlias(patch.ContentPack.Manifest.UniqueID, name)));
                foreach (string tokenName in tokensUsed)
                    IndexForToken(tokenName);

                // get indirect tokens
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
            // by asset name
            foreach ((IAssetName name, ISet<IPatch> list) in this.PatchesByCurrentTarget.ToArray())
            {
                if (list.Contains(patch))
                {
                    list.Remove(patch);
                    if (!list.Any())
                        this.PatchesByCurrentTarget.Remove(name);
                }
            }

            // by token
            foreach ((string key, ISet<IPatch> list) in this.PatchesAffectedByToken.ToArray())
            {
                if (list.Contains(patch))
                {
                    list.Remove(patch);
                    if (!list.Any())
                        this.PatchesAffectedByToken.Remove(key);
                }
            }
        }
    }
}
