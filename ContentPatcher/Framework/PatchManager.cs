using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Validators;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;

namespace ContentPatcher.Framework
{
    //
    // Optimization notes:
    //   - Don't remove entries from PatchesAffectedByToken / PatchesByCurrentTarget when removing patches:
    //        1. Dictionary inserts are much more expensive than lookups, so this leads to thrashing where the same
    //           keys are repeatedly removed & re-added and the dictionary tree gets repeatedly rebalanced.
    //        2. Some of the code assumes that a previously indexed key is guaranteed to be in the lookups, to avoid
    //           repeating does-entry-exist checks every time.
    //

    /// <summary>Manages loaded patches.</summary>
    internal class PatchManager
    {
        /*********
        ** Fields
        *********/
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
        private readonly InvariantDictionary<HashSet<IPatch>> PatchesAffectedByToken = new();

        /// <summary>The patches to apply, indexed by asset name.</summary>
        private readonly Dictionary<IAssetName, SortedSet<IPatch>> PatchesByCurrentTarget = new();

        /// <summary>The values under which each patch is indexed in <see cref="PatchesAffectedByToken"/> and <see cref="PatchesByCurrentTarget"/>.</summary>
        private readonly Dictionary<IPatch, IndexedPatchValues> IndexedPatchValues = new(new ObjectReferenceComparer<IPatch>());

        /// <summary>The new patches which haven't received a context update yet.</summary>
        private readonly HashSet<IPatch> PendingPatches = new();

        /// <summary>Assets for which patches were removed, which should be reloaded on the next context update.</summary>
        private readonly HashSet<IAssetName> AssetsWithRemovedPatches = new();

        /// <summary>The token changes queued for periodic update types.</summary>
        private readonly IDictionary<ContextUpdateType, MutableInvariantSet> QueuedTokenChanges = new Dictionary<ContextUpdateType, MutableInvariantSet>
        {
            [ContextUpdateType.OnTimeChange] = new(),
            [ContextUpdateType.OnLocationChange] = new(),
            [ContextUpdateType.All] = new()
        };

        /// <summary>A low-level content manager which is detached from SMAPI's content API, used to check whether an asset exists in the base game's content folder.</summary>
        private readonly LocalizedContentManager RawFileContentManager;


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
            this.RawFileContentManager = new LocalizedContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
        }

        /****
        ** Patching
        ****/
        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="e">The event data.</param>
        /// <param name="ignoreLoadPatches">Whether to ignore any load patches for this asset.</param>
        public void OnAssetRequested(AssetRequestedEventArgs e, bool ignoreLoadPatches)
        {
            LoadPatch[] loaders = !ignoreLoadPatches
                ? this.GetCurrentLoaders(e).ToArray()
                : Array.Empty<LoadPatch>();

            IPatch[] editors = this.GetCurrentEditors(e).ToArray();

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
        public void UpdateContext(IGameContentHelper contentHelper, IInvariantSet globalChangedTokens, ContextUpdateType updateType)
        {
            bool verbose = this.Monitor.IsVerbose;

            if (verbose)
                this.Monitor.Log($"Updating context for {updateType} tick...");

            // Patches can have variable update rates, so we keep track of updated tokens here so
            // we update patches at their next update point.
            {
                MutableInvariantSet affectedTokens = new MutableInvariantSet(globalChangedTokens);

                if (updateType == ContextUpdateType.All)
                {
                    // all token updates apply at day start
                    foreach (MutableInvariantSet tokenQueue in this.QueuedTokenChanges.Values)
                    {
                        affectedTokens.UnionWith(tokenQueue);
                        tokenQueue.Clear();
                    }
                }
                else
                {
                    // queue token changes for other update points
                    foreach ((ContextUpdateType curType, MutableInvariantSet queued) in this.QueuedTokenChanges)
                    {
                        if (curType != updateType)
                            queued.AddMany(globalChangedTokens);
                    }

                    // get queued changes for the current update point
                    affectedTokens.AddMany(this.QueuedTokenChanges[updateType]);
                    this.QueuedTokenChanges[updateType].Clear();
                }

                globalChangedTokens = affectedTokens.Lock();
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
            List<PatchAuditChange>? verbosePatchesReloaded = verbose
                ? new()
                : null;

            // update patches
            IAssetName? prevAssetName = null;
            HashSet<IPatch> newPatches = new(new ObjectReferenceComparer<IPatch>());
            while (patchQueue.Any())
            {
                IPatch patch = patchQueue.Dequeue();

                // log asset name
                if (verbose && prevAssetName != patch.TargetAsset)
                {
                    this.Monitor.Log($"   {patch.TargetAsset}:");
                    prevAssetName = patch.TargetAsset;
                }

                // track old values
                string? wasFromAsset = patch.FromAsset;
                IAssetName? wasTargetAsset = patch.TargetAsset;
                bool wasReady = patch.IsReady && !wasPending.Contains(patch);

                // update patch
                ModTokenContext tokenContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);
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
                        // force update index
                        // This is only needed for EditData patches which use FromFile, since the
                        // loaded file may include tokens which couldn't be analyzed when the patch
                        // was added. This scenario was deprecated in Content Patcher 1.16, when
                        // Include patches were added.
                        if (patch.FromAsset != wasFromAsset)
                            this.IndexPatch(patch, this.TokenManager.TrackLocalTokens(patch.ContentPack));
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

                // update index for target change
                if (!wasTargetAsset?.IsEquivalentTo(patch.TargetAsset) ?? patch.TargetAsset is not null)
                    this.IndexPatch(patch, tokenContext);

                // log change
                if (verbose)
                {
                    verbosePatchesReloaded!.Add(new PatchAuditChange(patch, wasReady, wasFromAsset, wasTargetAsset, reloadAsset));

                    IList<string> changes = new List<string>();
                    if (wasReady != isReady)
                        changes.Add(isReady ? "enabled" : "disabled");
                    if (wasTargetAsset != patch.TargetAsset)
                        changes.Add($"target: {wasTargetAsset} => {patch.TargetAsset}");
                    string changesStr = string.Join(", ", changes);

                    this.Monitor.Log($"      [{(isReady ? "X" : " ")}] {patch.Path}: {(changes.Any() ? changesStr : "OK")}");
                }
            }

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
                if (verbose)
                    this.Monitor.Log($"   reloading {reloadAssetNames.Count} assets: {string.Join(", ", reloadAssetNames.OrderByHuman(p => p.Name))}");
                contentHelper.InvalidateCache(asset =>
                {
                    bool match = reloadAssetNames.Contains(asset.NameWithoutLocale);
                    if (verbose)
                        this.Monitor.Log($"      [{(match ? "X" : " ")}] reload {asset.Name}");
                    return match;
                });
            }
        }

        /****
        ** Patches
        ****/
        /// <summary>Add a patch.</summary>
        /// <param name="patch">The patch to add.</param>
        public void Add(IPatch patch)
        {
            // set initial context
            ModTokenContext modContext = this.TokenManager.TrackLocalTokens(patch.ContentPack);
            patch.UpdateContext(modContext);

            // add to patch list
            if (this.Monitor.IsVerbose)
                this.Monitor.Log($"      added {patch.Type} {patch.TargetAsset}.");
            this.Patches.Add(patch);
            this.PendingPatches.Add(patch);

            // update indexes
            this.IndexPatch(patch, modContext);
        }

        /// <summary>Remove a patch.</summary>
        /// <param name="patch">The patch to remove.</param>
        public void Remove(IPatch patch)
        {
            // remove from patch list
            if (this.Monitor.IsVerbose)
                this.Monitor.Log($"      removed {patch.Path}.");
            if (!this.Patches.Remove(patch))
                return;

            // mark asset to reload
            if (patch.IsApplied && patch.TargetAsset != null)
                this.AssetsWithRemovedPatches.Add(patch.TargetAsset);

            // update indexes
            this.DeIndexPatch(patch);
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
        /// <param name="request">The asset request being intercepted.</param>
        public IEnumerable<LoadPatch> GetCurrentLoaders(AssetRequestedEventArgs request)
        {
            // If this is a request for a localized asset which isn't normally localized (like `Data/Buildings.fr-FR`)
            // *or* a new mod-provided asset, only load the base name. If the content manager doesn't find the
            // localized form, it'll load the base form next.
            IAssetName assetName = request.Name;
            IAssetName baseAssetName = request.NameWithoutLocale;
            bool skipLocalizedAssetByDefault =
                assetName.LocaleCode != null
                && (
                    !this.RawFileContentManager.DoesAssetExist<object>($"{baseAssetName}.fr-FR") // check fr-FR instead of the actual locale, since we do want to load it for a custom language
                    || !this.RawFileContentManager.DoesAssetExist<object>($"{baseAssetName}")
                );

            // find matching loader
            return this
                .GetPatches(baseAssetName)
                .Where(patch =>
                    patch.IsReady
                    && (patch.TargetLocale is null
                        ? !skipLocalizedAssetByDefault
                        : string.Equals(patch.TargetLocale, assetName.LocaleCode, StringComparison.InvariantCultureIgnoreCase)
                    )
                )
                .OfType<LoadPatch>();
        }

        /// <summary>Get patches which edit the given asset in the current context.</summary>
        /// <param name="request">The asset request being intercepted.</param>
        public IEnumerable<IPatch> GetCurrentEditors(AssetRequestedEventArgs request)
        {
            PatchType? patchType = this.GetEditType(request.DataType);
            if (patchType == null)
                return Array.Empty<IPatch>();

            return this
                .GetPatches(request.NameWithoutLocale)
                .Where(patch =>
                    patch.Type == patchType
                    && patch.IsReady
                    && (patch.TargetLocale is null || string.Equals(patch.TargetLocale, request.Name.LocaleCode, StringComparison.InvariantCultureIgnoreCase))
                );
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Apply load and edit patches to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="e">The asset requested context.</param>
        /// <param name="loaders">The load patch to apply.</param>
        /// <param name="editors">The edit patch to apply.</param>
        private void ApplyPatchesToAsset<T>(AssetRequestedEventArgs e, LoadPatch[] loaders, IPatch[] editors)
            where T : notnull
        {
            IAssetName assetName = e.NameWithoutLocale;

            // validate & select loader by priority
            LoadPatch? loader = null;
            foreach (LoadPatch candidate in loaders)
            {
                // skip if patch is invalid
                if (!candidate.FromAssetExists())
                {
                    this.Monitor.Log($"Can't apply load \"{candidate.Path}\" to {candidate.TargetAsset}: the {nameof(PatchConfig.FromFile)} file '{candidate.FromAsset}' doesn't exist.", LogLevel.Warn);
                    continue;
                }

                // skip if we already found a better match
                if (loader?.Priority > candidate.Priority)
                    continue;

                // abort if we have multiple exclusive patches
                const int exclusivePriority = (int)AssetLoadPriority.Exclusive;
                if (candidate.Priority is exclusivePriority && loader?.Priority == exclusivePriority)
                {
                    IPatch[] exclusiveLoaders = loaders.Where(p => p.Priority == exclusivePriority).ToArray();
                    string[] modNames = exclusiveLoaders.Select(p => p.ContentPack.Manifest.Name).Distinct().OrderByHuman().ToArray();
                    string[] patchNames = exclusiveLoaders.Select(p => p.Path.ToString()).OrderByHuman().ToArray();
                    switch (modNames.Length)
                    {
                        case 1:
                            this.Monitor.Log($"'{modNames[0]}' has multiple patches with the '{nameof(AssetLoadPriority.Exclusive)}' priority which load the '{assetName}' asset at the same time ({string.Join(", ", patchNames)}). None will be applied. You should report this to the content pack author.", LogLevel.Error);
                            break;

                        case 2:
                            this.Monitor.Log($"Two content packs want to load the '{assetName}' asset with the '{nameof(AssetLoadPriority.Exclusive)}' priority ({string.Join(" and ", modNames)}). Neither will be applied. You should remove one of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                            this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}");
                            break;

                        default:
                            this.Monitor.Log($"Multiple content packs want to load the '{assetName}' asset with the '{nameof(AssetLoadPriority.Exclusive)}' priority ({string.Join(", ", modNames)}). None will be applied. You should remove some of the content packs, or ask the authors about compatibility.", LogLevel.Error);
                            this.Monitor.Log($"Affected patches: {string.Join(", ", patchNames)}");
                            break;
                    }

                    loader = null;
                    break;
                }

                // else best match so far
                loader = candidate;
            }

            // apply selected load patch
            if (loader != null)
            {
                e.LoadFrom(
                    load: () => this.ApplyLoad<T>(loader, assetName)!, // only returns null when invalid, in which case there's no other way to abort
                    priority: AssetLoadPriority.Exclusive,
                    onBehalfOf: loader.ContentPack.Manifest.UniqueID
                );
            }

            // apply edit patches
            if (editors.Any())
            {
                List<List<IPatch>> editGroups = this.SortAndGroupEditPatches(editors);
                foreach (List<IPatch> group in editGroups)
                {
                    List<IPatch> patches = group; // avoid capturing foreach variable in the deferred callback
                    e.Edit(
                        apply: data => this.ApplyEdits<T>(patches, data),
                        priority: AssetEditPriority.Default,
                        onBehalfOf: patches[0].ContentPack.Manifest.UniqueID
                    );
                }
            }

            // log result
            if (this.Monitor.IsVerbose)
                this.Monitor.Log($"asset requested: can [{(loaders.Any() ? "X" : " ")}] load [{(editors.Any() ? "X" : " ")}] edit {assetName}");
        }

        /// <summary>Apply a load patch to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="patch">The patch to apply.</param>
        /// <param name="assetName">The asset name to load.</param>
        /// <returns>Returns the loaded asset data.</returns>
        private T? ApplyLoad<T>(LoadPatch patch, IAssetName assetName)
            where T : notnull
        {
            if (this.Monitor.IsVerbose)
                this.Monitor.Log($"Patch \"{patch.Path}\" loaded {assetName}.");

            try
            {
                // apply runtime migration
                {
                    T? data = default;
                    if (patch.Migrator.TryApplyLoadPatch<T>(patch, assetName, ref data, out string? error))
                    {
                        patch.IsApplied = true;
                        return data;
                    }

                    if (error != null)
                    {
                        this.Monitor.Log($"Can't apply patch {patch.Path} to {assetName}: {error}.", LogLevel.Error);
                        return default;
                    }
                }

                // else load normally
                {
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
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Can't apply patch {patch.Path} to {assetName}:\n{ex}.", LogLevel.Error);
                return default;
            }
        }

        /// <summary>Apply edit patches to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="patches">The patches to apply.</param>
        /// <param name="asset">The asset data to edit.</param>
        private void ApplyEdits<T>(ICollection<IPatch> patches, IAssetData asset)
            where T : notnull
        {
            foreach (IPatch patch in patches)
            {
                if (this.Monitor.IsVerbose)
                    this.Monitor.Log($"Applied patch \"{patch.Path}\" to {asset.Name}.");

                try
                {
                    // apply runtime migration
                    if (patch.Migrator.TryApplyEditPatch<T>(patch, asset, out string? error))
                        patch.IsApplied = true;
                    else if (error != null)
                        this.Monitor.Log($"Can't apply patch {patch.Path} to {asset.Name}: {error}.", LogLevel.Error);

                    // else apply normally
                    else
                    {
                        patch.Edit<T>(asset);
                        patch.IsApplied = true;
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Unhandled exception applying patch: {patch.Path}.\n{ex}", LogLevel.Error);
                    patch.IsApplied = false;
                }
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
        private HashSet<IPatch> GetPatchesToUpdate(IInvariantSet globalChangedTokens, ContextUpdateType updateType)
        {
            // add patches which depend on a changed token
            var patches = new HashSet<IPatch>(new ObjectReferenceComparer<IPatch>());
            foreach (string tokenName in globalChangedTokens)
            {
                if (this.PatchesAffectedByToken.TryGetValue(tokenName, out HashSet<IPatch>? affectedPatches))
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

        /// <summary>Sort a list of patches by priority and group sequential edits by mod.</summary>
        /// <param name="patches">The patches to group.</param>
        private List<List<IPatch>> SortAndGroupEditPatches(IEnumerable<IPatch> patches)
        {
            List<List<IPatch>> groups = new();

            string? lastModId = null;
            List<IPatch> group = new();
            foreach (IPatch patch in patches.OrderBy(p => p.Priority))
            {
                string modId = patch.ContentPack.Manifest.UniqueID;
                if (modId != lastModId)
                {
                    lastModId = modId;
                    if (group.Count > 0)
                    {
                        groups.Add(group);
                        group = new();
                    }
                }

                group.Add(patch);
            }

            if (group.Any())
                groups.Add(group);

            return groups;
        }

        /// <summary>Update the internal patch lookups for a patch's current values.</summary>
        /// <param name="patch">The patch to index.</param>
        /// <param name="modContext">The token context for the mod which owns the patch.</param>
        private void IndexPatch(IPatch patch, ModTokenContext modContext)
        {
            // get previous indexed values
            bool isNew = false;
            if (!this.IndexedPatchValues.TryGetValue(patch, out IndexedPatchValues? prevValues))
            {
                isNew = true;
                this.IndexedPatchValues[patch] = prevValues = new IndexedPatchValues();
            }

            // index by target
            {
                IAssetName? curTarget = patch.TargetAsset;
                IAssetName? prevTarget = prevValues.Target;

                if (isNew || (!prevTarget?.IsEquivalentTo(curTarget) ?? curTarget is not null))
                {
                    if (this.Monitor.IsVerbose)
                        this.Monitor.Log($"[patch index: by target] {patch.Path}: {(prevTarget is not null ? $"{prevTarget} > " : "")}{patch.TargetAsset}");

                    if (!isNew && prevTarget is not null)
                        this.PatchesByCurrentTarget[prevTarget].Remove(patch);

                    if (curTarget is not null)
                        this.GetPatchesByTarget(curTarget).Add(patch);
                }

                prevValues.Target = curTarget;
            }

            // index by tokens
            {
                IInvariantSet curRawTokens = patch.GetTokensUsed();
                IInvariantSet prevRawTokens = prevValues.RawTokens;

                if (isNew || !curRawTokens.SetEquals(prevRawTokens))
                {
                    IInvariantSet resolvedTokens = this.ResolveTokensUsed(curRawTokens, modContext);

                    if (this.Monitor.IsVerbose)
                        this.Monitor.Log($"[patch index: by tokens] {patch.Path}: ({string.Join(", ", prevValues.ResolvedTokens)}) > ({string.Join(", ", resolvedTokens)})");

                    if (!isNew)
                    {
                        foreach (string prevToken in prevValues.ResolvedTokens)
                        {
                            if (!resolvedTokens.Contains(prevToken))
                                this.PatchesAffectedByToken[prevToken].Remove(patch);
                        }
                    }

                    foreach (string tokenName in resolvedTokens)
                        this.GetPatchesAffectedByToken(tokenName).Add(patch);

                    prevValues.RawTokens = curRawTokens;
                    prevValues.ResolvedTokens = resolvedTokens;
                }
            }
        }

        /// <summary>Remove a patch from the internal indexes.</summary>
        /// <param name="patch">The patch to index.</param>
        private void DeIndexPatch(IPatch patch)
        {
            if (this.IndexedPatchValues.Remove(patch, out IndexedPatchValues? prevValues))
            {
                if (prevValues.Target is not null)
                    this.PatchesByCurrentTarget[prevValues.Target].Remove(patch);

                foreach (string tokenName in prevValues.ResolvedTokens)
                    this.PatchesAffectedByToken[tokenName].Remove(patch);
            }
        }

        /// <summary>Resolve token aliases and add indirect tokens to a list of used tokens.</summary>
        /// <param name="rawTokens">The raw tokens to resolve.</param>
        /// <param name="modContext">The token context for the mod which owns the patch.</param>
        private IInvariantSet ResolveTokensUsed(IInvariantSet rawTokens, ModTokenContext modContext)
        {
            MutableInvariantSet resolvedTokens = new();

            foreach (string rawToken in rawTokens)
            {
                // resolve token alias
                string resolved = modContext.ResolveAlias(rawToken);
                resolvedTokens.Add(resolved);

                // get indirect tokens
                foreach (string token in modContext.GetTokensWhichAffect(resolved))
                    resolvedTokens.Add(token);
            }

            return resolvedTokens.GetImmutable();
        }

        /// <summary>Get an entry from <see cref="PatchesByCurrentTarget"/>, adding it if needed.</summary>
        /// <param name="assetName">The target asset name.</param>
        private SortedSet<IPatch> GetPatchesByTarget(IAssetName assetName)
        {
            if (this.PatchesByCurrentTarget.TryGetValue(assetName, out SortedSet<IPatch>? set))
                return set;

            set = new SortedSet<IPatch>(PatchIndexComparer.Instance);
            this.PatchesByCurrentTarget[assetName] = set;
            return set;
        }

        /// <summary>Get an entry from <see cref="PatchesAffectedByToken"/>, adding it if needed.</summary>
        /// <param name="tokenName">The token name.</param>
        private HashSet<IPatch> GetPatchesAffectedByToken(string tokenName)
        {
            if (this.PatchesAffectedByToken.TryGetValue(tokenName, out HashSet<IPatch>? set))
                return set;

            set = new HashSet<IPatch>();
            this.PatchesAffectedByToken[tokenName] = set;
            return set;
        }
    }
}
