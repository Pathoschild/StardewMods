using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patchers
{
    /// <summary>Metadata for an asset that should be patched with a new image.</summary>
    internal class EditImagePatch : IPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The asset key to load from the content pack instead.</summary>
        public string FromLocalAsset { get; }

        /// <summary>The sprite area from which to read an image.</summary>
        public Rectangle? FromArea { get; }

        /// <summary>The sprite area to overwrite.</summary>
        public Rectangle? ToArea { get; }



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="fromLocalAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The sprite area from which to read an image.</param>
        /// <param name="toArea">The sprite area to overwrite.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public EditImagePatch(IContentPack contentPack, string assetName, string fromLocalAsset, Rectangle fromArea, Rectangle toArea, IMonitor monitor)
        {
            // init
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.FromLocalAsset = fromLocalAsset;
            this.FromArea = fromArea != Rectangle.Empty ? fromArea : null as Rectangle?;
            this.ToArea = toArea != Rectangle.Empty ? toArea : null as Rectangle?;
            this.Monitor = monitor;
        }

        /// <summary>Apply the patch to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        public void Apply<T>(IAssetData asset)
        {
            // validate
            if (typeof(T) != typeof(Texture2D))
            {
                this.Monitor.Log($"Can't apply edit-image patch by {this.ContentPack.Manifest.Name} to {this.AssetName}: this file isn't an image file (found {typeof(T)}).", LogLevel.Warn);
                return;
            }

            // apply
            Texture2D source = this.ContentPack.LoadAsset<Texture2D>(this.FromLocalAsset);
            asset
                .AsImage()
                .PatchImage(source, this.FromArea, this.ToArea);
        }
    }
}
