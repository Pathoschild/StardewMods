using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

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

        /// <summary>Handles the logic around loading assets from content packs.</summary>
        private readonly AssetLoader AssetLoader;

        /// <summary>The asset key to load from the content pack instead.</summary>
        private readonly string FromLocalAsset;

        /// <summary>The sprite area from which to read an image.</summary>
        private readonly Rectangle? FromArea;

        /// <summary>The sprite area to overwrite.</summary>
        private readonly Rectangle? ToArea;


        /*********
        ** Accessors
        *********/
        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The language code to patch (or <c>null</c> for any language).</summary>
        /// <remarks>This is handled by the main logic.</remarks>
        public string Locale { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="locale">The language code to patch (or <c>null</c> for any language).</param>
        /// <param name="fromLocalAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The sprite area from which to read an image.</param>
        /// <param name="toArea">The sprite area to overwrite.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="assetLoader">Handles the logic around loading assets from content packs.</param>
        public EditImagePatch(IContentPack contentPack, string assetName, string locale, string fromLocalAsset, Rectangle fromArea, Rectangle toArea, IMonitor monitor, AssetLoader assetLoader)
        {
            // init
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.Locale = locale;
            this.FromLocalAsset = fromLocalAsset;
            this.FromArea = fromArea != Rectangle.Empty ? fromArea : null as Rectangle?;
            this.ToArea = toArea != Rectangle.Empty ? toArea : null as Rectangle?;
            this.Monitor = monitor;
            this.AssetLoader = assetLoader;
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

            // fetch data
            Texture2D source = this.AssetLoader.Load<Texture2D>(this.ContentPack, this.FromLocalAsset);
            IAssetDataForImage editor = asset.AsImage();

            // extend tilesheet if needed
            Rectangle affectedArea = this.GetTargetArea(source, this.FromArea, this.ToArea);
            if (affectedArea.Bottom > editor.Data.Height)
            {
                Texture2D original = editor.Data;
                Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, original.Width, affectedArea.Bottom);
                editor.ReplaceWith(texture);
                editor.PatchImage(original);
            }

            // apply source image
            editor.PatchImage(source, this.FromArea, this.ToArea);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the area that will be affected by the patch.</summary>
        /// <param name="source">The source texture.</param>
        /// <param name="fromArea">The sprite area in the source asset from which to read an image.</param>
        /// <param name="toArea">The sprite area to overwrite.</param>
        private Rectangle GetTargetArea(Texture2D source, Rectangle? fromArea, Rectangle? toArea)
        {
            if (toArea.HasValue)
                return toArea.Value;

            return fromArea.HasValue
                ? new Rectangle(0, 0, fromArea.Value.Width, fromArea.Value.Height)
                : new Rectangle(0, 0, source.Width, source.Height);
        }
    }
}
