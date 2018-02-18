using StardewModdingAPI;

namespace ContentPatcher.Framework.Patchers
{
    /// <summary>Metadata for an asset that should be replaced with a content pack file.</summary>
    internal class LoadPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The language code to patch (or <c>null</c> for any language).</summary>
        public string Locale { get; }

        /// <summary>The asset key to load from the content pack instead.</summary>
        public string LocalAsset { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="locale">The language code to patch (or <c>null</c> for any language).</param>
        /// <param name="localAsset">The asset key to load from the content pack instead.</param>
        public LoadPatch(IContentPack contentPack, string assetName, string locale, string localAsset)
        {
            // init
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.Locale = locale;
            this.LocalAsset = localAsset;
        }
    }
}
