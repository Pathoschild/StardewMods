using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.JsonAssets
{
    /// <summary>Handles the logic for integrating with the Json Assets mod.</summary>
    internal class JsonAssetsIntegration : BaseIntegration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's public API.</summary>
        private readonly IJsonAssetsApi ModApi;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public JsonAssetsIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Json Assets", "spacechase0.JsonAssets", "1.5.1", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;

            // get mod API
            this.ModApi = this.GetValidatedApi<IJsonAssetsApi>();
            this.IsLoaded = this.ModApi != null;
        }

        /// <summary>Get the custom spritesheet and source rectangle for an in-game entity.</summary>
        /// <param name="entity">The in-world entity (e.g. fruit tree).</param>
        /// <param name="texture">The custom sprite texture.</param>
        /// <param name="sourceRect">The custom area within the texture containing the spritesheet.</param>
        /// <param name="currentSpriteOnly">Whether to return a rectangle which only contains the entity's current sprite (e.g. for a crop's current phase of growth), instead of all its sprites.</param>
        /// <returns>Returns true if the entity has a custom sprite, else false.</returns>
        /// <remarks>This returns a texture which matches the vanilla layout. For example, for a fruit tree this would return the area containing all the sprites for the tree's growth stages and states in the same layout as the vanilla tilesheet.</remarks>
        public bool TryGetCustomSpriteSheet(object entity, out Texture2D texture, out Rectangle sourceRect, bool currentSpriteOnly = false)
        {
            return currentSpriteOnly
                ? this.ModApi.TryGetCustomSprite(entity, out texture, out sourceRect)
                : this.ModApi.TryGetCustomSpriteSheet(entity, out texture, out sourceRect);
        }
    }
}
