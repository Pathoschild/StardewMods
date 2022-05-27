using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an asset that should be patched with a new image.</summary>
    internal class EditImagePatch : Patch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The sprite area from which to read an image.</summary>
        private readonly TokenRectangle? FromArea;

        /// <summary>The sprite area to overwrite.</summary>
        private readonly TokenRectangle? ToArea;

        /// <summary>Indicates how the image should be patched.</summary>
        private readonly PatchImageMode PatchMode;

        /// <summary>Whether the patch extended the last image asset it was applied to.</summary>
        private bool ResizedLastImage;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The sprite area from which to read an image.</param>
        /// <param name="toArea">The sprite area to overwrite.</param>
        /// <param name="patchMode">Indicates how the image should be patched.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="parseAssetName">Parse an asset name.</param>
        public EditImagePatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, TokenRectangle? fromArea, TokenRectangle? toArea, PatchImageMode patchMode, UpdateRate updateRate, IContentPack contentPack, IPatch? parentPatch, IMonitor monitor, Func<string, IAssetName> parseAssetName)
            : base(
                indexPath: indexPath,
                path: path,
                type: PatchType.EditImage,
                assetName: assetName,
                conditions: conditions,
                parseAssetName: parseAssetName,
                fromAsset: fromAsset,
                updateRate: updateRate,
                contentPack: contentPack,
                parentPatch: parentPatch
            )
        {
            this.FromArea = fromArea;
            this.ToArea = toArea;
            this.PatchMode = patchMode;
            this.Monitor = monitor;

            this.Contextuals
                .Add(fromArea)
                .Add(toArea);
        }

        /// <inheritdoc />
        public override void Edit<T>(IAssetData asset)
        {
            // validate
            if (typeof(T) != typeof(Texture2D))
            {
                this.Warn($"this file isn't an image file (found {typeof(T)}).");
                return;
            }
            if (!this.FromAssetExists())
            {
                this.Warn($"the {nameof(PatchConfig.FromFile)} file '{this.FromAsset}' doesn't exist.");
                return;
            }

            // get editor
            IAssetDataForImage editor = asset.AsImage();

            // read source file
            IRawTextureData? rawSource = null;
            Texture2D? fullSource = null;
            int sourceWidth;
            int sourceHeight;
            if (string.Equals(System.IO.Path.GetExtension(this.FromAsset), ".xnb", StringComparison.OrdinalIgnoreCase))
            {
                fullSource = this.ContentPack.ModContent.Load<Texture2D>(this.FromAsset);
                sourceWidth = fullSource.Width;
                sourceHeight = fullSource.Height;
            }
            else
            {
                rawSource = this.ContentPack.ModContent.Load<IRawTextureData>(this.FromAsset);
                sourceWidth = rawSource.Width;
                sourceHeight = rawSource.Height;
            }

            // fetch data
            if (!this.TryReadArea(this.FromArea, 0, 0, sourceWidth, sourceHeight, out Rectangle sourceArea, out string? error))
            {
                this.Warn($"the source area is invalid: {error}.");
                return;
            }
            if (!this.TryReadArea(this.ToArea, 0, 0, sourceArea.Width, sourceArea.Height, out Rectangle targetArea, out error))
            {
                this.Warn($"the target area is invalid: {error}.");
                return;
            }

            // validate error conditions
            if (sourceArea.X < 0 || sourceArea.Y < 0 || sourceArea.Width < 0 || sourceArea.Height < 0)
            {
                this.Warn($"source area (X:{sourceArea.X}, Y:{sourceArea.Y}, Width:{sourceArea.Width}, Height:{sourceArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                return;
            }
            if (targetArea.X < 0 || targetArea.Y < 0 || targetArea.Width < 0 || targetArea.Height < 0)
            {
                this.Warn($"target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                return;
            }
            if (targetArea.Right > editor.Data.Width)
            {
                this.Warn($"target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) extends past the right edge of the image (Width:{editor.Data.Width}), which isn't allowed. Patches can only extend the tilesheet downwards.", LogLevel.Error);
                return;
            }
            if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
            {
                string sourceAreaLabel = this.FromArea != null ? $"{nameof(this.FromArea)}" : "source image";
                string targetAreaLabel = this.ToArea != null ? $"{nameof(this.ToArea)}" : "target image";
                this.Warn($"{sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", LogLevel.Error);
                return;
            }

            // extend tilesheet if needed
            this.ResizedLastImage = editor.ExtendImage(editor.Data.Width, targetArea.Bottom);

            // apply source image
            if (rawSource is not null)
                editor.PatchImage(rawSource, sourceArea, targetArea, (PatchMode)this.PatchMode);
            else
                editor.PatchImage(fullSource!, sourceArea, targetArea, (PatchMode)this.PatchMode);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            if (this.ResizedLastImage)
                yield return "resized image";

            yield return "edited image";
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Log a warning for an issue when applying the patch.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The message log level.</param>
        private void Warn(string message, LogLevel level = LogLevel.Warn)
        {
            this.Monitor.Log($"Can't apply image patch \"{this.Path}\" to {this.TargetAsset}: {message}", level);
        }
    }
}
