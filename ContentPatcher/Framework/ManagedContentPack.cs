using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using xTile;
using xTile.Tiles;

namespace ContentPatcher.Framework
{
    /// <summary>Handles loading assets from content packs.</summary>
    internal class ManagedContentPack
    {
        /*********
        ** Properties
        *********/
        /// <summary>A cache of PNG textures needed by the mod.</summary>
        /// <remarks>This is needed to avoid errors when a texture is loaded during the draw loop (e.g. farmhouse textures).</remarks>
        private readonly IDictionary<string, Texture2D> PngCache = new InvariantDictionary<Texture2D>();


        /*********
        ** Accessors
        *********/
        /// <summary>The managed content pack.</summary>
        public IContentPack Pack { get; }

        /// <summary>The content pack's manifest.</summary>
        public IManifest Manifest => this.Pack.Manifest;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="pack">The content pack to manage.</param>
        public ManagedContentPack(IContentPack pack)
        {
            this.Pack = pack;
        }

        /// <summary>Get whether a file exists in the content pack.</summary>
        /// <param name="key">The asset key.</param>
        public bool FileExists(string key)
        {
            return this.GetRealPath(key) != null;
        }

        /// <summary>Get an asset from the content pack.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="key">The asset key.</param>
        public T Load<T>(string key)
        {
            key = this.GetRealPath(key) ?? throw new FileNotFoundException($"The file '{key}' does not exist in the {this.Pack.Manifest.Name} content patch folder.");

            // get from PNG cache
            if (typeof(T) == typeof(Texture2D))
            {
                string cacheKey = this.Pack.GetActualAssetKey(key);
                if (this.PngCache.TryGetValue(cacheKey, out Texture2D texture))
                    return (T)(object)texture;
            }

            // load from content pack
            return this.Pack.LoadAsset<T>(key);
        }

        /// <summary>Read a JSON file from the content pack folder.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the content pack directory.</param>
        /// <returns>Returns the deserialised model, or <c>null</c> if the file doesn't exist or is empty.</returns>
        public TModel ReadJsonFile<TModel>(string path) where TModel : class
        {
            return this.Pack.ReadJsonFile<TModel>(path);
        }

        /// <summary>Get the raw absolute path for a path within the content pack.</summary>
        /// <param name="relativePath">The path relative to the content pack folder.</param>
        public string GetFullPath(string relativePath)
        {
            return Path.Combine(this.Pack.DirectoryPath, relativePath);
        }

        /// <summary>Preload a texture if needed to avoid errors later.</summary>
        /// <param name="relativePath">The path relative to the content pack folder.</param>
        public void PreloadIfNeeded(string relativePath)
        {
            string key = this.GetRealPath(relativePath) ?? throw new FileNotFoundException($"The file '{relativePath}' does not exist in the {this.Manifest.Name} content pack folder.");

            // PNG asset
            if (this.IsPngPath(key))
            {
                string actualAssetKey = this.Pack.GetActualAssetKey(key);
                if (!this.PngCache.ContainsKey(actualAssetKey))
                    this.PngCache[actualAssetKey] = this.Pack.LoadAsset<Texture2D>(key);
                return;
            }

            // map PNG tilesheets
            if (this.TryLoadMap(key, out Map map))
            {
                string relativeRoot = this.Pack.GetActualAssetKey(""); // warning: this depends on undocumented SMAPI implementation details
                foreach (TileSheet tilesheet in map.TileSheets)
                {
                    // ignore if not a PNG in the content pack
                    if (!tilesheet.ImageSource.StartsWith(relativeRoot) || Path.GetExtension(tilesheet.ImageSource).Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    // ignore if local file doesn't exist
                    string relativeImageSource = this.GetRealPath(tilesheet.ImageSource.Substring(relativeRoot.Length + 1));
                    if (relativeImageSource == null)
                        continue;

                    // load asset
                    string actualAssetKey = this.Pack.GetActualAssetKey(relativeImageSource);
                    if (!this.PngCache.ContainsKey(actualAssetKey))
                        this.PngCache[actualAssetKey] = this.Pack.LoadAsset<Texture2D>(relativeImageSource);
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether an asset is a unpacked PNG file.</summary>
        /// <param name="key">The asset key in the content pack.</param>
        private bool IsPngPath(string key)
        {
            return key.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Try to load an asset key as a map file.</summary>
        /// <param name="key">The asset key in the content pack.</param>
        /// <param name="map">The loaded map.</param>
        /// <returns>Returns whether the map was successfully loaded.</returns>
        private bool TryLoadMap(string key, out Map map)
        {
            // ignore if we know it's not a map
            if (!key.EndsWith(".tbin", StringComparison.InvariantCultureIgnoreCase) && !key.EndsWith(".xnb", StringComparison.InvariantCultureIgnoreCase))
            {
                map = null;
                return false;
            }

            // try to load map
            try
            {
                map = this.Pack.LoadAsset<Map>(key);
                return true;
            }
            catch
            {
                map = null;
                return false;
            }
        }

        /// <summary>Get the actual relative path within the content pack for a file, matched case-insensitively, or <c>null</c> if not found.</summary>
        /// <param name="key">The case-insensitive asset key.</param>
        private string GetRealPath(string key)
        {
            // try file match first
            var exactMatch = new FileInfo(Path.Combine(this.Pack.DirectoryPath, key));
            if (exactMatch.Exists)
                return exactMatch.FullName.Substring(this.Pack.DirectoryPath.Length + 1);

            // search for a case-insensitive file match (Linux/Mac are case-sensitive)
            foreach (string path in Directory.EnumerateFiles(this.Pack.DirectoryPath, "*", SearchOption.AllDirectories))
            {
                if (!path.StartsWith(this.Pack.DirectoryPath))
                    throw new InvalidOperationException("File search failed, contained files aren't in the searched folder (???).");

                string relativePath = path.Substring(this.Pack.DirectoryPath.Length + 1);
                if (relativePath.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return relativePath;
            }

            return null;
        }
    }
}
