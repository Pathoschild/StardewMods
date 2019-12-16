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
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="localAsset">The asset key to load from the content pack instead.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public LoadPatch(string logName, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, ITokenString localAsset, Func<string, string> normalizeAssetName)
            : base(logName, PatchType.Load, contentPack, assetName, conditions, normalizeAssetName, fromAsset: localAsset) { }

        /// <summary>Load the initial version of the asset.</summary>
        /// <param name="asset">The asset to load.</param>
        public override T Load<T>(IAssetInfo asset)
        {
            return this.ContentPack.Load<T>(this.FromAsset);
        }

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
        public override IEnumerable<string> GetChangeLabels()
        {
            yield return "replaced asset";
        }
    }
}
