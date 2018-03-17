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


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether this patch should be applied.</summary>
        /// <param name="context">The condition context.</param>
        bool IsMatch(ConditionContext context);

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
    }
}
