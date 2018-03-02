using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using xTile;
using xTile.Tiles;

namespace ContentPatcher.Framework
{
    /// <summary>Handles the logic around loading assets from content packs.</summary>
    internal class AssetLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>The textures provided by content packs.</summary>
        /// <remarks>This field is used to circumvent an issue where PNGs can't be loaded while a draw is in progress. Since we can't predict when the game will ask for a texture, we preload them ahead of time.</remarks>
        private readonly IDictionary<string, Texture2D> PngTextureCache = new Dictionary<string, Texture2D>();

        ///// <summary>The maps provided by content packs.</summary>
        //private readonly IDictionary<string, Map> MapCache = new Dictionary<string, Map>();


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the given asset key exists in the content pack files.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public bool AssetExists(IContentPack contentPack, string key)
        {
            return File.Exists(Path.Combine(contentPack.DirectoryPath, key));
        }

        /// <summary>Preload an asset from the content pack if necessary.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public void PreloadIfNeeded(IContentPack contentPack, string key)
        {
            // PNG asset
            if (this.IsPngPath(key))
            {
                string actualAssetKey = contentPack.GetActualAssetKey(key);
                if (!this.PngTextureCache.ContainsKey(actualAssetKey))
                    this.PngTextureCache[actualAssetKey] = contentPack.LoadAsset<Texture2D>(key);
            }

            // map PNG tilesheets
            if (this.TryLoadMap(contentPack, key, out Map map))
            {
                string relativeRoot = contentPack.GetActualAssetKey(""); // warning: this depends on undocumented SMAPI implementation details
                foreach (TileSheet tilesheet in map.TileSheets)
                {
                    if (!tilesheet.ImageSource.StartsWith(relativeRoot) || Path.GetExtension(tilesheet.ImageSource).Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                        continue; // not a content pack PNG

                    string relativePath = tilesheet.ImageSource.Substring(relativeRoot.Length + 1);
                    string actualAssetKey = contentPack.GetActualAssetKey(relativePath);
                    if (!this.PngTextureCache.ContainsKey(actualAssetKey) && this.AssetExists(contentPack, relativePath))
                        this.PngTextureCache[actualAssetKey] = contentPack.LoadAsset<Texture2D>(relativePath);
                }
            }
        }

        /// <summary>Get an asset from the content pack of <see cref="PngTextureCache"/>.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public T Load<T>(IContentPack contentPack, string key)
        {
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
