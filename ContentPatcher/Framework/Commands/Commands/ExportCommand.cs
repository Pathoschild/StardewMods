using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Commands;
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

                   Usage: patch export ""<asset name>"" ""<C# type name>""
                   Same as the previous, but manually specify the C# data type for the asset. This must be the C# full type name. Example types:
                      - A string/string dictionary: System.Collections.Generic.Dictionary`2[[System.String],[System.String]]
                      - A number/string dictionary: System.Collections.Generic.Dictionary`2[[System.Int32],[System.String]]
                      - Movie reactions: System.Collections.Generic.List<StardewValley.GameData.Movies.MovieReaction>
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            // get asset name
            if (args.Length is < 1 or > 2)
            {
                this.Monitor.Log("The 'patch export' command expects one argument containing the target asset name, and an optional second argument for the data type. See 'patch help' for more info.", LogLevel.Error);
                return;
            }
            string assetName = args[0];

            // get type
            string typeName = args.Length > 1 ? args[1] : "System.Object";
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                this.Monitor.Log($"Could not load type '{typeName}'. This must be the C# full type name like System.Collections.Generic.Dictionary`2[[System.String],[System.String]].", LogLevel.Error);
                return;
            }

            // load asset
            object asset;
            try
            {
                asset = this.LoadAsset(assetName, type);
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

            Texture2D result = new Texture2D(texture.GraphicsDevice ?? Game1.graphics.GraphicsDevice, texture.Width, texture.Height);
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

            return type == typeof(Dictionary<,>) || type == typeof(List<>) || type == typeof(JArray);
        }

        /// <summary>Load an asset from a content manager using the given type.</summary>
        /// <param name="assetName">The asset name to load.</param>
        /// <param name="type">The asset type to load.</param>
        private object LoadAsset(string assetName, Type type)
        {
            return this
                .GetType()
                .GetMethod(nameof(this.LoadAssetImpl), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { assetName });
        }

        /// <summary>Load an asset from a content manager using the given type.</summary>
        /// <typeparam name="TAsset">The asset type to load.</typeparam>
        /// <param name="assetName">The asset name to load.</param>
        private TAsset LoadAssetImpl<TAsset>(string assetName)
        {
            // get from main content manager if it's already cached
            if (this.IsAssetLoaded(Game1.content, assetName))
                return Game1.content.Load<TAsset>(assetName);

            // If it's not already cached, use a temporary content manager
            // This avoids corrupting the cache with an invalid type if it doesn't match.
            using ContentManager contentManager = Game1.content.CreateTemporary();
            return contentManager.Load<TAsset>(assetName);
        }

        /// <summary>Get whether the given content manager has already loaded and cached the given asset.</summary>
        /// <param name="contentManager">The content manager to check.</param>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        private bool IsAssetLoaded(ContentManager contentManager, string assetName)
        {
            // get SMAPI's IsLoaded method
            Type type = contentManager.GetType();
            MethodInfo method = type.GetMethod("IsLoaded", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
                throw new InvalidOperationException("Can't access 'IsLoaded' method on the main content manager.");

            // get value
            return (bool)method.Invoke(contentManager, new object[] { assetName, LocalizedContentManager.CurrentLanguageCode });
        }
    }
}
