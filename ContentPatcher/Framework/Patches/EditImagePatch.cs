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
        private readonly TokenRectangle FromArea;

        /// <summary>The sprite area to overwrite.</summary>
        private readonly TokenRectangle ToArea;

        /// <summary>Indicates how the image should be patched.</summary>
        private readonly PatchMode PatchMode;

        /// <summary>Whether the patch extended the last image asset it was applied to.</summary>
        private bool ResizedLastImage;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
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
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public EditImagePatch(LogPathBuilder path, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, TokenRectangle fromArea, TokenRectangle toArea, PatchMode patchMode, UpdateRate updateRate, ManagedContentPack contentPack, IPatch parentPatch, IMonitor monitor, Func<string, string> normalizeAssetName)
            : base(
                path: path,
                type: PatchType.EditImage,
                assetName: assetName,
                conditions: conditions,
                normalizeAssetName: normalizeAssetName,
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
            string errorPrefix = $"Can't apply image patch \"{this.Path}\" to {this.TargetAsset}";

            // validate
            if (typeof(T) != typeof(Texture2D))
            {
                this.Monitor.Log($"{errorPrefix}: this file isn't an image file (found {typeof(T)}).", LogLevel.Warn);
                return;
            }
            if (!this.FromAssetExists())
            {
                this.Monitor.Log($"{errorPrefix}: the {nameof(PatchConfig.FromFile)} file '{this.FromAsset}' doesn't exist.", LogLevel.Warn);
                return;
            }

            // fetch data
            IAssetDataForImage editor = asset.AsImage();
            Texture2D source = this.ContentPack.Load<Texture2D>(this.FromAsset);
            if (!this.TryReadArea(this.FromArea, 0, 0, source.Width, source.Height, out Rectangle sourceArea, out string error))
            {
                this.Monitor.Log($"{errorPrefix}: the source area is invalid: {error}.", LogLevel.Warn);
                return;
            }
            if (!this.TryReadArea(this.ToArea, 0, 0, sourceArea.Width, sourceArea.Height, out Rectangle targetArea, out error))
            {
                this.Monitor.Log($"{errorPrefix}: the target area is invalid: {error}.", LogLevel.Warn);
                return;
            }

            // validate error conditions
            if (sourceArea.X < 0 || sourceArea.Y < 0 || sourceArea.Width < 0 || sourceArea.Height < 0)
            {
                this.Monitor.Log($"{errorPrefix}: source area (X:{sourceArea.X}, Y:{sourceArea.Y}, Width:{sourceArea.Width}, Height:{sourceArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                return;
            }
            if (targetArea.X < 0 || targetArea.Y < 0 || targetArea.Width < 0 || targetArea.Height < 0)
            {
                this.Monitor.Log($"{errorPrefix}: target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                return;
            }
            if (targetArea.Right > editor.Data.Width)
            {
                this.Monitor.Log($"{errorPrefix}: target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) extends past the right edge of the image (Width:{editor.Data.Width}), which isn't allowed. Patches can only extend the tilesheet downwards.", LogLevel.Error);
                return;
            }
            if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
            {
                string sourceAreaLabel = this.FromArea != null ? $"{nameof(this.FromArea)}" : "source image";
                string targetAreaLabel = this.ToArea != null ? $"{nameof(this.ToArea)}" : "target image";
                this.Monitor.Log($"{errorPrefix}: {sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", LogLevel.Error);
                return;
            }

            // extend tilesheet if needed
            this.ResizedLastImage = editor.ExtendImage(editor.Data.Width, targetArea.Bottom);

            // apply source image
            editor.PatchImage(source, sourceArea, targetArea, this.PatchMode);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            if (this.ResizedLastImage)
                yield return "resized image";

            yield return "edited image";
        }
    }
}
