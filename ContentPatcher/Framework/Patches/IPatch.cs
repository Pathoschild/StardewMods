using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A patch which can be applied to an asset.</summary>
    internal interface IPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>The patch type.</summary>
        PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        string AssetName { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        ConditionDictionary Conditions { get; }

        /// <summary>Whether this patch should be applied in the latest context.</summary>
        bool MatchesContext { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        bool UpdateContext(ConditionContext context);

        /// <summary>Load the initial version of the asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to load.</param>
        /// <exception cref="System.NotSupportedException">The current patch type doesn't support loading assets.</exception>
        T Load<T>(IAssetInfo asset);

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        /// <exception cref="System.NotSupportedException">The current patch type doesn't support editing assets.</exception>
        void Edit<T>(IAssetData asset);

        /// <summary>Get the condition tokens used by this patch in its fields.</summary>
        IEnumerable<ConditionKey> GetTokensUsed();
    }
}
