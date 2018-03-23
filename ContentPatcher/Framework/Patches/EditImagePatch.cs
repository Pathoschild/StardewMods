using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an asset that should be patched with a new image.</summary>
    internal class EditImagePatch : Patch
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The asset key to load from the content pack instead.</summary>
        private readonly TokenString FromLocalAsset;

        /// <summary>The sprite area from which to read an image.</summary>
        private readonly Rectangle? FromArea;

        /// <summary>The sprite area to overwrite.</summary>
        private readonly Rectangle? ToArea;

        /// <summary>Indicates how the image should be patched.</summary>
        private readonly PatchMode PatchMode;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="assetLoader">Handles loading assets from content packs.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromLocalAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The sprite area from which to read an image.</param>
        /// <param name="toArea">The sprite area to overwrite.</param>
        /// <param name="patchMode">Indicates how the image should be patched.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="normaliseAssetName">Normalise an asset name.</param>
        public EditImagePatch(string logName, AssetLoader assetLoader, IContentPack contentPack, TokenString assetName, ConditionDictionary conditions, TokenString fromLocalAsset, Rectangle fromArea, Rectangle toArea, PatchMode patchMode, IMonitor monitor, Func<string, string> normaliseAssetName)
            : base(logName, PatchType.EditImage, assetLoader, contentPack, assetName, conditions, normaliseAssetName)
        {
            this.FromLocalAsset = fromLocalAsset;
            this.FromArea = fromArea != Rectangle.Empty ? fromArea : null as Rectangle?;
            this.ToArea = toArea != Rectangle.Empty ? toArea : null as Rectangle?;
            this.PatchMode = patchMode;
            this.Monitor = monitor;
        }

        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        public override bool UpdateContext(ConditionContext context)
        {
            bool localAssetChanged = this.FromLocalAsset.UpdateContext(context);
            return base.UpdateContext(context) || localAssetChanged;
        }

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        public override void Edit<T>(IAssetData asset)
        {
            // validate
            if (typeof(T) != typeof(Texture2D))
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\" to {this.AssetName}: this file isn't an image file (found {typeof(T)}).", LogLevel.Warn);
                return;
            }

            // fetch data
            Texture2D source = this.AssetLoader.Load<Texture2D>(this.ContentPack, this.FromLocalAsset.Value);
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

            // validate error conditions
            if (affectedArea.Right > editor.Data.Width)
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\": target area (X:{affectedArea.X}, Y:{affectedArea.Y}, Width:{affectedArea.Width}, Height:{affectedArea.Height}) extends past the right edge of the image (Width:{editor.Data.Width}), which isn't supported. Patches can only extend the tilesheet downwards.", LogLevel.Error);
                return;
            }
            if (this.FromArea != null && (this.FromArea.Value.Width != affectedArea.Width || this.FromArea.Value.Height != affectedArea.Height))
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\": source image size (Width:{affectedArea.Width}, Height:{affectedArea.Height}) doesn't match the target area size (Width:{affectedArea.Width}, Height:{affectedArea.Height}).", LogLevel.Error);
                return;
            }

            // apply source image
            editor.PatchImage(source, this.FromArea, this.ToArea, this.PatchMode);
        }

        /// <summary>Get the condition tokens used by this patch in its fields.</summary>
        public override IEnumerable<ConditionKey> GetTokensUsed()
        {
            return base.GetTokensUsed().Union(this.FromLocalAsset.ConditionTokens);
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
