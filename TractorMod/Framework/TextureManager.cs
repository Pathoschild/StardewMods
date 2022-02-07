using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Manages textures loaded for the tractor and garage.</summary>
    internal class TextureManager : IDisposable
    {
        /*********
        ** Fields
        *********/
        /// <summary>The absolute path to the Tractor Mod folder.</summary>
        private readonly string DirectoryPath;

        /// <summary>The base path for assets loaded through the game's content pipeline so other mods can edit them.</summary>
        private readonly string PublicAssetBasePath;

        /// <summary>The content helper from which to load assets.</summary>
        private readonly IModContentHelper ContentHelper;

        /// <summary>The monitor with which to log errors.</summary>
        private readonly IMonitor Monitor;

        /// <summary>A case-insensitive list of files in the <c>assets</c> folder.</summary>
        private readonly IDictionary<string, string> AssetMap;


        /*********
        ** Accessors
        *********/
        /// <summary>The buff icon texture.</summary>
        public Texture2D? BuffIconTexture { get; private set; }

        /// <summary>The garage texture to apply.</summary>
        public Texture2D? GarageTexture { get; private set; }

        /// <summary>The tractor texture to apply.</summary>
        public Texture2D? TractorTexture { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="directoryPath">The absolute path to the Tractor Mod folder.</param>
        /// <param name="publicAssetBasePath">The base path for assets loaded through the game's content pipeline so other mods can edit them.</param>
        /// <param name="contentHelper">The content helper from which to load assets.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        public TextureManager(string directoryPath, string publicAssetBasePath, IModContentHelper contentHelper, IMonitor monitor)
        {
            this.DirectoryPath = directoryPath;
            this.PublicAssetBasePath = publicAssetBasePath;
            this.ContentHelper = contentHelper;
            this.Monitor = monitor;

            this.AssetMap = this.BuildAssetMap(directoryPath);
        }

        /// <summary>Update the textures if needed.</summary>
        public void UpdateTextures()
        {
            // tractor
            if (this.TryLoadFromContent("tractor", out Texture2D? texture, out string? error))
                this.TractorTexture = texture;
            else
                this.Monitor.Log(error, LogLevel.Error);

            // garage
            if (this.TryLoadFromContent("garage", out texture, out error))
                this.GarageTexture = texture;
            else
                this.Monitor.Log(error, LogLevel.Error);

            // buff icon
            if (this.TryLoadFromContent("buffIcon", out texture, out error))
                this.BuffIconTexture = texture;
            else
                this.Monitor.Log(error, LogLevel.Error);
        }

        /// <summary>Apply the mod textures to the given stable, if applicable.</summary>
        /// <param name="horse">The horse to change.</param>
        /// <param name="isTractor">Get whether a horse is a tractor.</param>
        public void ApplyTextures(Horse? horse, Func<Horse?, bool> isTractor)
        {
            if (this.TractorTexture != null && isTractor(horse))
                horse!.Sprite.spriteTexture = this.TractorTexture;
        }

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="e">The event data.</param>
        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
            // Allow for garages from older versions that didn't get normalized correctly.
            // This can be removed once support for legacy data is dropped.
            if (e.NameWithoutLocale.IsEquivalentTo("Buildings/TractorGarage") && this.GarageTexture != null)
                e.LoadFrom(() => this.GarageTexture, AssetLoadPriority.Low);

            // load tractor, garage, or buff texture
            if (e.NameWithoutLocale.IsEquivalentTo($"{this.PublicAssetBasePath}/Tractor") || e.NameWithoutLocale.IsEquivalentTo($"{this.PublicAssetBasePath}/Garage") || e.NameWithoutLocale.IsEquivalentTo($"{this.PublicAssetBasePath}/BuffIcon"))
            {
                string key = PathUtilities.GetSegments(e.NameWithoutLocale.Name).Last();
                e.LoadFrom(
                    () => this.TryLoadFromFile(key, out Texture2D? texture, out string? error)
                        ? texture
                        : throw new InvalidOperationException(error),
                    AssetLoadPriority.Exclusive
                );
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.GarageTexture?.Dispose();
            this.TractorTexture?.Dispose();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Try to load the asset for a texture from the game's content folder so other mods can apply edits.</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'Tractor' or 'Garage').</param>
        /// <param name="texture">The loaded texture, if found.</param>
        /// <param name="error">A human-readable error to show to the user if texture wasn't found.</param>
        private bool TryLoadFromContent(string spritesheet, [NotNullWhen(true)] out Texture2D? texture, [NotNullWhen(false)] out string? error)
        {
            try
            {
                texture = Game1.content.Load<Texture2D>($"{this.PublicAssetBasePath}/{spritesheet}");
                error = null;
            }
            catch (Exception ex)
            {
                texture = null;
                error = ex.ToString();
                return false;
            }

            return texture != null;
        }

        /// <summary>Try to load the asset for a texture from the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        /// <param name="texture">The loaded texture, if found.</param>
        /// <param name="error">A human-readable error to show to the user if texture wasn't found.</param>
        private bool TryLoadFromFile(string spritesheet, [NotNullWhen(true)] out Texture2D? texture, [NotNullWhen(false)] out string? error)
        {
            texture = this.TryGetTextureKey(spritesheet, out string? key, out error)
                ? this.ContentHelper.Load<Texture2D>(key)
                : null;

            return texture != null;
        }

        /// <summary>Try to get the asset key for a texture from the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        /// <param name="key">The asset key to use, if found.</param>
        /// <param name="error">A human-readable error to show to the user if texture wasn't found.</param>
        private bool TryGetTextureKey(string spritesheet, [NotNullWhen(true)] out string? key, [NotNullWhen(false)] out string? error)
        {
            string seasonalKey = $"{Game1.currentSeason}_{spritesheet}.png";
            string defaultKey = $"{spritesheet}.png";

            foreach (string possibleKey in new[] { seasonalKey, defaultKey })
            {
                if (this.AssetMap.TryGetValue(possibleKey, out string? actualKey) && File.Exists(Path.Combine(this.DirectoryPath, $"assets/{actualKey}")))
                {
                    key = $"assets/{actualKey}";
                    error = null;
                    return true;
                }
            }

            key = null;
            error = $"Couldn't find file '{defaultKey}' in the mod folder. This mod isn't installed correctly; try reinstalling the mod to fix it.";
            return false;
        }

        /// <summary>Initialize a case-insensitive list of files in the <c>assets</c> folder.</summary>
        /// <param name="directoryPath">The absolute path to the mod folder.</param>
        private IDictionary<string, string> BuildAssetMap(string directoryPath)
        {
            var assetMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // get assets folder
            DirectoryInfo dir = new(Path.Combine(directoryPath, "assets"));
            if (!dir.Exists)
            {
                this.Monitor.Log("Tractor Mod's 'assets' folder is missing. The mod will not work correctly. Try reinstalling the mod to fix this.", LogLevel.Error);
                return assetMap;
            }

            // create map
            foreach (FileInfo file in dir.GetFiles())
                assetMap[file.Name] = file.Name;

            return assetMap;
        }
    }
}
