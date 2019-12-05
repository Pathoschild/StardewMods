using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
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
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The sprite area from which to read an image.</summary>
        private readonly Rectangle? FromArea;

        /// <summary>The sprite area to overwrite.</summary>
        private readonly Rectangle? ToArea;

        /// <summary>Indicates how the image should be patched.</summary>
        private readonly PatchMode PatchMode;

        /// <summary>Whether the patch extended the last image asset it was applied to.</summary>
        private bool ResizedLastImage;

        /// <summary>Color used for blending.</summary>
        private Color? BlendColor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The sprite area from which to read an image.</param>
        /// <param name="toArea">The sprite area to overwrite.</param>
        /// <param name="patchMode">Indicates how the image should be patched.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public EditImagePatch(string logName, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, ITokenString fromAsset, Rectangle fromArea, Rectangle toArea, PatchMode patchMode, Color? blendColor, IMonitor monitor, Func<string, string> normalizeAssetName)
            : base(logName, PatchType.EditImage, contentPack, assetName, conditions, normalizeAssetName, fromAsset: fromAsset)
        {
            this.FromArea = fromArea != Rectangle.Empty ? fromArea : null as Rectangle?;
            this.ToArea = toArea != Rectangle.Empty ? toArea : null as Rectangle?;
            this.PatchMode = patchMode;
            this.BlendColor = blendColor;
            this.Monitor = monitor;
        }

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        public override void Edit<T>(IAssetData asset)
        {
            // validate
            if (typeof(T) != typeof(Texture2D))
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\" to {this.TargetAsset}: this file isn't an image file (found {typeof(T)}).", LogLevel.Warn);
                return;
            }
            if (!this.FromAssetExists())
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\" to {this.TargetAsset}: the {nameof(PatchConfig.FromFile)} file '{this.FromAsset}' doesn't exist.", LogLevel.Warn);
                return;
            }

            // fetch data
            Texture2D source = this.ContentPack.Load<Texture2D>(this.FromAsset);
            Rectangle sourceArea = this.FromArea ?? new Rectangle(0, 0, source.Width, source.Height);
            Rectangle targetArea = this.ToArea ?? new Rectangle(0, 0, sourceArea.Width, sourceArea.Height);
            IAssetDataForImage editor = asset.AsImage();

            // validate error conditions
            if (sourceArea.X < 0 || sourceArea.Y < 0 || sourceArea.Width < 0 || sourceArea.Height < 0)
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\": source area (X:{sourceArea.X}, Y:{sourceArea.Y}, Width:{sourceArea.Width}, Height:{sourceArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                return;
            }
            if (targetArea.X < 0 || targetArea.Y < 0 || targetArea.Width < 0 || targetArea.Height < 0)
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\": target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                return;
            }
            if (targetArea.Right > editor.Data.Width)
            {
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\": target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) extends past the right edge of the image (Width:{editor.Data.Width}), which isn't allowed. Patches can only extend the tilesheet downwards.", LogLevel.Error);
                return;
            }
            if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
            {
                string sourceAreaLabel = this.FromArea.HasValue ? $"{nameof(this.FromArea)}" : "source image";
                string targetAreaLabel = this.ToArea.HasValue ? $"{nameof(this.ToArea)}" : "target image";
                this.Monitor.Log($"Can't apply image patch \"{this.LogName}\": {sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", LogLevel.Error);
                return;
            }

            // extend tilesheet if needed
            if (targetArea.Bottom > editor.Data.Height)
            {
                Texture2D original = editor.Data;
                Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, original.Width, targetArea.Bottom);
                editor.ReplaceWith(texture);
                editor.PatchImage(original);
                this.ResizedLastImage = true;
            }
            else
                this.ResizedLastImage = false;

            // color blending
            if (this.BlendColor.HasValue)
            {
                Texture2D colorBlendedSource = ColorBlend(source, this.BlendColor.Value);

                // apply color blended source image
                editor.PatchImage(colorBlendedSource, sourceArea, this.ToArea, this.PatchMode);
            }
            else
            {
                // apply source image
                editor.PatchImage(source, sourceArea, this.ToArea, this.PatchMode);
            }
        }

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
        public override IEnumerable<string> GetChangeLabels()
        {
            if (this.ResizedLastImage)
                yield return "resized image";

            yield return "edited image";
        }

        // <summary>Color blending (multiplication).</summary>
        private Texture2D ColorBlend(Texture2D source, Color blendColor)
        {
            Color[] sourcePixels = new Color[source.Width * source.Height];
            source.GetData(sourcePixels);
            for (int i = 0; i < sourcePixels.Length; i++)
            {
                sourcePixels[i]
                    = new Color((byte)(sourcePixels[i].R * blendColor.R / 255),
                                (byte)(sourcePixels[i].G * blendColor.G / 255),
                                (byte)(sourcePixels[i].B * blendColor.B / 255),
                                (byte)(sourcePixels[i].A * blendColor.A / 255));
            }

            Texture2D blended = new Texture2D(Game1.graphics.GraphicsDevice, source.Width, source.Height);
            blended.SetData(sourcePixels);

            return blended;
        }
    }
}
