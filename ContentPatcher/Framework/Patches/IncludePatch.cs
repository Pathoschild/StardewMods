using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for patches to load from another content file.</summary>
    internal class IncludePatch : Patch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The content pack for which to load patches.</summary>
        private readonly RawContentPack RawContentPack;

        /// <summary>Handles loading and unloading patches for content packs.</summary>
        private readonly PatchLoader PatchLoader;

        /// <summary>Whether the patch already tried loading the <see cref="Patch.FromAsset"/> asset for the current context. This doesn't necessarily means it succeeded (e.g. the file may not have existed).</summary>
        private bool AttemptedDataLoad;


        /*********
        ** Accessors
        *********/
        /// <summary>The patches that were loaded by the latest update, if any. This is cleared on the next update if no new patches were loaded.</summary>
        public IEnumerable<IPatch>? PatchesJustLoaded { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromFile">The normalized asset key from which to load entries (if applicable), including tokens.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="parseAssetName">Parse an asset name.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="patchLoader">Handles loading and unloading patches for content packs.</param>
        public IncludePatch(int[] indexPath, LogPathBuilder path, IManagedTokenString? assetName, IEnumerable<Condition> conditions, IManagedTokenString fromFile, UpdateRate updateRate, RawContentPack contentPack, IPatch? parentPatch, Func<string, IAssetName> parseAssetName, IMonitor monitor, PatchLoader patchLoader)
            : base(
                indexPath: indexPath,
                path: path,
                type: PatchType.Include,
                assetName: assetName,
                conditions: conditions,
                fromAsset: fromFile,
                updateRate: updateRate,
                parentPatch: parentPatch,
                contentPack: contentPack.ContentPack,
                parseAssetName: parseAssetName
            )
        {
            this.RawContentPack = contentPack;
            this.Monitor = monitor;
            this.PatchLoader = patchLoader;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            this.PatchesJustLoaded = null;

            // update context
            if (!this.UpdateContext(context, out string? previousFilePath))
                return false;

            // unload previous patches
            if (!string.IsNullOrWhiteSpace(previousFilePath))
                this.PatchLoader.UnloadPatchesLoadedBy(this, reindex: false);

            // load new patches
            this.AttemptedDataLoad = this.Conditions.All(p => p.IsMatch);
            this.IsApplied = false;
            if (this.AttemptedDataLoad)
            {
                string errorPrefix = $"Error loading patch '{this.Path}'";
                try
                {
                    // validate file existence
                    if (!this.FromAssetExists())
                    {
                        if (this.IsReady)
                            this.Monitor.Log($"{errorPrefix}: file '{this.FromAsset}' doesn't exist.", LogLevel.Warn);
                        return this.MarkUpdated();
                    }

                    // prevent circular reference
                    {
                        List<string> loopPaths = new List<string>();
                        for (IPatch? parent = this.ParentPatch; parent != null; parent = parent.ParentPatch)
                        {
                            if (parent.Type == PatchType.Include)
                            {
                                loopPaths.Add(parent.FromAsset!);
                                if (this.IsSameFilePath(parent.FromAsset!, this.FromAsset))
                                {
                                    loopPaths.Reverse();
                                    loopPaths.Add(this.FromAsset);

                                    this.Monitor.Log($"{errorPrefix}: patch skipped because it would cause an infinite loop ({string.Join(" > ", loopPaths)}).", LogLevel.Warn);
                                    return this.MarkUpdated();
                                }
                            }
                        }
                    }

                    // load raw file
                    var content = this.ContentPack.ModContent.Load<ContentConfig>(this.FromAsset);
                    if (!content.Changes.Any())
                    {
                        this.Monitor.Log($"{errorPrefix}: file '{this.FromAsset}' doesn't have anything in the {nameof(content.Changes)} field. Is the file formatted correctly?", LogLevel.Warn);
                        return this.MarkUpdated();
                    }

                    // validate fields
                    string[] invalidFields = this.GetInvalidFields(content).ToArray();
                    if (invalidFields.Any())
                    {
                        this.Monitor.Log($"{errorPrefix}: file contains fields which aren't allowed for a secondary file ({string.Join(", ", invalidFields.OrderByHuman())}).", LogLevel.Warn);
                        return this.MarkUpdated();
                    }

                    // load patches
                    this.PatchesJustLoaded = this.PatchLoader.LoadPatches(
                        contentPack: this.RawContentPack,
                        rawPatches: content.Changes,
                        rootIndexPath: this.IndexPath,
                        path: this.GetIncludedLogPath(this.FromAsset),
                        reindex: true,
                        parentPatch: this
                    );
                    this.IsApplied = true;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{errorPrefix}. Technical details:\n{ex}", LogLevel.Error);
                    return this.MarkUpdated();
                }
            }

            return this.MarkUpdated();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            yield break;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Update the underlying patch fields when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="previousFilePath">The file path that was previously loaded, if any.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        private bool UpdateContext(IContext context, out string? previousFilePath)
        {
            previousFilePath = this.AttemptedDataLoad && this.IsReady && this.FromAssetExists() ? this.FromAsset : null;
            return base.UpdateContext(context);
        }

        /// <summary>Get whether two include paths are equivalent.</summary>
        /// <param name="left">The first path to compare.</param>
        /// <param name="right">The second path to compare.</param>
        private bool IsSameFilePath(string? left, string? right)
        {
            if (left == right)
                return true;

            if (left == null || right == null)
                return false;

            left = PathUtilities.NormalizeAssetName(left);
            right = PathUtilities.NormalizeAssetName(right);
            return left.EqualsIgnoreCase(right);
        }

        /// <summary>Get the content fields which aren't allowed for a secondary file which were set.</summary>
        /// <param name="content">The content to validate.</param>
        private IEnumerable<string> GetInvalidFields(ContentConfig content)
        {
            foreach (PropertyInfo property in typeof(ContentConfig).GetProperties())
            {
                if (property.Name == nameof(ContentConfig.Changes))
                    continue;

                object? value = property.GetValue(content);
                bool hasValue = value is IEnumerable list
                    ? list.Cast<object>().Any()
                    : value != null;

                if (hasValue)
                    yield return property.Name;
            }
        }

        /// <summary>Get the root log path for included patches.</summary>
        /// <param name="filePath">The file path being loaded.</param>
        private LogPathBuilder GetIncludedLogPath(string filePath)
        {
            filePath = PathUtilities.NormalizeAssetName(filePath);

            string logName = PathUtilities.NormalizeAssetName(this.Path.Segments.Last());
            return logName.ContainsIgnoreCase(filePath)
                ? this.Path // no need to add file to path if it's already in the name
                : this.Path.With(filePath);
        }
    }
}
