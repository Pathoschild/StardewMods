using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Migrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an asset that should be replaced with a content pack file.</summary>
    internal class LoadPatch : Patch
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="localAsset">The asset key to load from the content pack instead.</param>
        /// <param name="priority">The priority for this patch when multiple patches apply.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="migrator">The aggregate migration which applies for this patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="parseAssetName">Parse an asset name.</param>
        public LoadPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString localAsset, AssetLoadPriority priority, UpdateRate updateRate, IEnumerable<Condition> conditions, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, Func<string, IAssetName> parseAssetName)
            : base(
                indexPath: indexPath,
                path: path,
                type: PatchType.Load,
                assetName: assetName,
                priority: (int)priority,
                updateRate: updateRate,
                conditions: conditions,
                contentPack: contentPack,
                migrator: migrator,
                parentPatch: parentPatch,
                parseAssetName: parseAssetName,
                fromAsset: localAsset
            )
        { }

        /// <inheritdoc />
        public override T Load<T>(IAssetName assetName)
        {
            return this.ContentPack.ModContent.Load<T>(this.FromAsset!);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            return new[] { "replaced asset" };
        }
    }
}
