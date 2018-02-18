using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>A patch which can be applied to an asset.</summary>
    internal interface IPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>The content pack which requested the patch.</summary>
        IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        string AssetName { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Apply the patch to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        void Apply<T>(IAssetData asset);
    }
}
