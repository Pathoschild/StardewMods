using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an asset that should be replaced with a content pack file.</summary>
    internal class LoadPatch : Patch
    {
        /*********
        ** Properties
        *********/
        /// <summary>The asset key to load from the content pack instead.</summary>
        public TokenString LocalAsset { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="localAsset">The asset key to load from the content pack instead.</param>
        /// <param name="normaliseAssetName">Normalise an asset name.</param>
        public LoadPatch(string logName, ManagedContentPack contentPack, TokenString assetName, ConditionDictionary conditions, TokenString localAsset, Func<string, string> normaliseAssetName)
            : base(logName, PatchType.Load, contentPack, assetName, conditions, normaliseAssetName)
        {
            this.LocalAsset = localAsset;
        }

        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        public override bool UpdateContext(ConditionContext context)
        {
            bool localAssetChanged = this.LocalAsset.UpdateContext(context);
            return base.UpdateContext(context) || localAssetChanged;
        }

        /// <summary>Load the initial version of the asset.</summary>
        /// <param name="asset">The asset to load.</param>
        public override T Load<T>(IAssetInfo asset)
        {
            return this.ContentPack.Load<T>(this.LocalAsset.Value);
        }

        /// <summary>Get the condition tokens used by this patch in its fields.</summary>
        public override IEnumerable<ConditionKey> GetTokensUsed()
        {
            return base.GetTokensUsed().Union(this.LocalAsset.ConditionTokens);
        }
    }
}
