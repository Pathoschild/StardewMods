using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which saves a copy of an asset.</summary>
    internal class ExportCommand : BaseCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public ExportCommand(IMonitor monitor)
            : base(monitor, "export") { }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch export
                   Usage: patch export ""<asset name>""
                   Saves a copy of an asset (including any changes from mods like Content Patcher) to the game folder. The asset name should be the target without the locale or extension, like ""Characters/Abigail"" if you want to export the value of 'Content/Characters/Abigail.xnb'.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            // get asset name
            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch export' command expects a single argument containing the target asset name. See 'patch help' for more info.", LogLevel.Error);
                return;
            }
            string assetName = args[0];

            // load asset
            object asset;
            try
            {
                asset = Game1.content.Load<object>(assetName);
            }
            catch (ContentLoadException ex)
            {
                this.Monitor.Log($"Can't load asset '{assetName}': {ex.Message}", LogLevel.Error);
                return;
            }

            // init export path
            string fullTargetPath = Path.Combine(StardewModdingAPI.Constants.ExecutionPath, "patch export", string.Join("_", assetName.Split(Path.GetInvalidFileNameChars())));
            Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath));

            // export
            if (asset is Texture2D texture)
            {
                fullTargetPath += ".png";

                texture = this.UnpremultiplyTransparency(texture);
                using (Stream stream = File.Create(fullTargetPath))
                    texture.SaveAsPng(stream, texture.Width, texture.Height);

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else if (this.IsDataAsset(asset))
            {
                fullTargetPath += ".json";

                File.WriteAllText(fullTargetPath, JsonConvert.SerializeObject(asset, Formatting.Indented));

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else
                this.Monitor.Log($"Can't export asset '{assetName}' of type {asset?.GetType().FullName ?? "null"}, expected image or data.", LogLevel.Error);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reverse premultiplication applied to an image asset by the XNA content pipeline.</summary>
        /// <param name="texture">The texture to adjust.</param>
        private Texture2D UnpremultiplyTransparency(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                Color pixel = data[i];
                if (pixel.A == 0)
                    continue;

                data[i] = new Color(
                    (byte)((pixel.R * 255) / pixel.A),
                    (byte)((pixel.G * 255) / pixel.A),
                    (byte)((pixel.B * 255) / pixel.A),
                    pixel.A
                ); // don't use named parameters, which are inconsistent between MonoGame (e.g. 'alpha') and XNA (e.g. 'a')
            }

            Texture2D result = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            result.SetData(data);
            return result;
        }

        /// <summary>Get whether an asset can be saved to JSON.</summary>
        /// <param name="asset">The asset to check.</param>
        private bool IsDataAsset(object asset)
        {
            if (asset == null)
                return false;

            Type type = asset.GetType();
            type = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            return type == typeof(Dictionary<,>) || type == typeof(List<>);
        }
    }
}
