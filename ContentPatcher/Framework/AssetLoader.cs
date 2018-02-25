using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

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
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public void PreloadIfNeeded<T>(IContentPack contentPack, string key)
        {
            if (this.IsPng<T>(key))
            {
                string actualAssetKey = contentPack.GetActualAssetKey(key);
                if (!this.PngTextureCache.ContainsKey(actualAssetKey))
                    this.PngTextureCache[actualAssetKey] = contentPack.LoadAsset<Texture2D>(key);
            }
        }

        /// <summary>Get an asset from the content pack of <see cref="PngTextureCache"/>.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="key">The asset key.</param>
        public T Load<T>(IContentPack contentPack, string key)
        {
            // load from PNG cache if applicable
            if (this.IsPng<T>(key))
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
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="key">The asset key in the content pack.</param>
        private bool IsPng<T>(string key)
        {
            return
                typeof(T) == typeof(Texture2D)
                && key.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
