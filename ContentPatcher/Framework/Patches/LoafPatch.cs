using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an image that should be replaced with bread.</summary>
    internal class LoafPatch : Patch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Bread bread bread bread bread bread.</summary>
        private readonly Texture2D Bread;

        /// <summary>Whether the bread has been left out in the rain for a bit.</summary>
        private readonly bool Soggy;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromAsset">The asset key to load from the content pack instead.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="bread">Bread bread bread bread bread bread.</param>
        /// <param name="soggy">Whether the bread has been left out in the rain for a bit.</param>
        public LoafPatch(string logName, ManagedContentPack contentPack, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, Func<string, string> normalizeAssetName, IMonitor monitor, Texture2D bread, bool soggy)
            : base(logName, PatchType.Loaf, contentPack, assetName, conditions, normalizeAssetName, fromAsset: fromAsset)
        {
            this.Monitor = monitor;
            this.Bread = bread;
            this.Soggy = soggy;
        }

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        public override void Edit<T>(IAssetData asset)
        {
            string errorPrefix = $"Can't apply loaf patch \"{this.LogName}\" to {this.TargetAsset}";

            // validate
            if (typeof(T) != typeof(Texture2D))
            {
                this.Monitor.Log($"{errorPrefix}: this file isn't an image file (found {typeof(T)}).", LogLevel.Warn);
                return;
            }

            // resize and apply bread
            IAssetDataForImage editor = asset.AsImage();

            editor.ReplaceWith(this.Resize(this.Bread, editor.Data.Width, editor.Data.Height));
        }

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
        public override IEnumerable<string> GetChangeLabels()
        {
            yield return "replaced image with " + (this.Soggy ? "soggy " : "") + "bread";
        }

        /// <summary>Resize an image to a target width and height using a nearest neighbor algorithm.</summary>
        private Texture2D Resize(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D target = new Texture2D(Game1.graphics.GraphicsDevice, targetWidth, targetHeight);
            Color[] targetData = new Color[target.Width * target.Height];

            Color[] sourceData = new Color[source.Width * source.Height];

            source.GetData(sourceData);

            for (int x = 0; x < target.Width; x++)
            {
                for (int y = 0; y < target.Height; y++)
                {
                    int nearestX = (int)((((float)x) / target.Width) * source.Width);
                    int nearestY = (int)((((float)y) / target.Height) * source.Height);

                    targetData[x + y * target.Width] = sourceData[nearestX + nearestY * source.Width];
                }
            }

            target.SetData(targetData);

            return target;
        }
    }
}
