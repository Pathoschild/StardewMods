using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an asset that should be replaced with a content pack file.</summary>
    internal class LoadPatch : Patch
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="localAsset">The asset key to load from the content pack instead.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public LoadPatch(LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString localAsset, IEnumerable<Condition> conditions, UpdateRate updateRate, ManagedContentPack contentPack, IPatch parentPatch, Func<string, string> normalizeAssetName)
            : base(
                  path: path,
                  type: PatchType.Load,
                  assetName: assetName,
                  conditions: conditions,
                  updateRate: updateRate,
                  contentPack: contentPack,
                  parentPatch: parentPatch,
                  normalizeAssetName: normalizeAssetName,
                  fromAsset: localAsset
                )
        { }

        /// <inheritdoc />
        public override T Load<T>(IAssetInfo asset)
        {
            return this.ContentPack.Load<T>(this.FromAsset);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            yield return "replaced asset";
        }
    }
}
