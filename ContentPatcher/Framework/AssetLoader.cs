using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using xTile;
using xTile.Tiles;

namespace ContentPatcher.Framework
{
    /// <summary>Handles loading assets from content packs.</summary>
    internal class AssetLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>The textures provided by content packs.</summary>
        /// <remarks>This field is used to circumvent an issue where PNGs can't be loaded while a draw is in progress. Since we can't predict when the game will ask for a texture, we preload them ahead of time.</remarks>
        private readonly IDictionary<string, Texture2D> PngTextureCache = new Dictionary<string, Texture2D>();


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a file exists in the content pack.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public bool FileExists(IContentPack contentPack, string key)
        {
            return this.GetRealPath(contentPack, key) != null;
        }

        /// <summary>Preload an asset from the content pack if necessary.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        /// <returns>Returns whether any assets were preloaded.</returns>
        public bool PreloadIfNeeded(IContentPack contentPack, string key)
        {
            key = this.GetRealPath(contentPack, key) ?? throw new FileNotFoundException($"The file '{key}' does not exist in the {contentPack.Manifest.Name} content patch folder.");
            bool anyLoaded = false;

            // PNG asset
            if (this.IsPngPath(key))
            {
                string actualAssetKey = contentPack.GetActualAssetKey(key);
                if (!this.PngTextureCache.ContainsKey(actualAssetKey))
                {
                    this.PngTextureCache[actualAssetKey] = contentPack.LoadAsset<Texture2D>(key);
                    anyLoaded = true;
                }
            }

            // map PNG tilesheets
            if (this.TryLoadMap(contentPack, key, out Map map))
            {
                string relativeRoot = contentPack.GetActualAssetKey(""); // warning: this depends on undocumented SMAPI implementation details
                foreach (TileSheet tilesheet in map.TileSheets)
                {
                    // ignore if not a PNG in the content pack
                    if (!tilesheet.ImageSource.StartsWith(relativeRoot) || Path.GetExtension(tilesheet.ImageSource).Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    // ignore if local file doesn't exist
                    string relativePath = this.GetRealPath(contentPack, tilesheet.ImageSource.Substring(relativeRoot.Length + 1));
                    if (relativePath == null)
                        continue;

                    // load asset
                    string actualAssetKey = contentPack.GetActualAssetKey(relativePath);
                    if (!this.PngTextureCache.ContainsKey(actualAssetKey))
                    {
                        this.PngTextureCache[actualAssetKey] = contentPack.LoadAsset<Texture2D>(relativePath);
                        anyLoaded = true;
                    }
                }
            }

            return anyLoaded;
        }

        /// <summary>Get an asset from the content pack of <see cref="PngTextureCache"/>.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public T Load<T>(IContentPack contentPack, string key)
        {
            key = this.GetRealPath(contentPack, key) ?? throw new FileNotFoundException($"The file '{key}' does not exist in the {contentPack.Manifest.Name} content patch folder.");

            // load from PNG cache if applicable
            if (typeof(T) == typeof(Texture2D) && this.IsPngPath(key))
            {
                string actualAssetKey = contentPack.GetActualAssetKey(key);
                if (this.PngTextureCache.TryGetValue(actualAssetKey, out Texture2D texture))
                    return (T)(object)texture;
            }

            // load from pack
            return contentPack.LoadAsset<T>(key);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the actual relative path within the content pack for a file, matched case-insensitively, or <c>null</c> if not found.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The case-insensitive asset key.</param>
        private string GetRealPath(IContentPack contentPack, string key)
        {
            // try file match first
            var exactMatch = new FileInfo(Path.Combine(contentPack.DirectoryPath, key));
            if (exactMatch.Exists)
                return exactMatch.FullName.Substring(contentPack.DirectoryPath.Length + 1);

            // search for a case-insensitive file match (Linux/Mac are case-sensitive)
            foreach (string path in Directory.EnumerateFiles(contentPack.DirectoryPath, "*", SearchOption.AllDirectories))
            {
                if (!path.StartsWith(contentPack.DirectoryPath))
                    throw new InvalidOperationException("File search failed, contained files aren't in the searched folder (???).");

                string relativePath = path.Substring(contentPack.DirectoryPath.Length + 1);
                if (relativePath.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return relativePath;
            }

            return null;
        }

        /// <summary>Get whether an asset is a unpacked PNG file.</summary>
        /// <param name="key">The asset key in the content pack.</param>
        private bool IsPngPath(string key)
        {
            return key.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Try to load an asset key as a map file.</summary>
        /// <param name="contentPack">The content pack to load.</param>
        /// <param name="key">The asset key in the content pack.</param>
        /// <param name="map">The loaded map.</param>
        /// <returns>Returns whether the map was successfully loaded.</returns>
        private bool TryLoadMap(IContentPack contentPack, string key, out Map map)
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
                map = contentPack.LoadAsset<Map>(key);
                return true;
            }
            catch
            {
                map = null;
                return false;
            }
        }
    }
}
