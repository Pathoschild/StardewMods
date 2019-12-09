using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.Common.Integrations.JsonAssets
{
    /// <summary>The API provided by the Json Assets mod.</summary>
    public interface IJsonAssetsApi
    {
        /// <summary>Get the custom spritesheet for an in-game entity, and the source rectangle containing the current sprite for this entity.</summary>
        /// <param name="entity">The in-world entity (e.g. fruit tree).</param>
        /// <param name="texture">The custom sprite texture.</param>
        /// <param name="sourceRect">The custom area within the texture containing the spritesheet.</param>
        /// <returns>Returns true if the entity has a custom sprite, else false.</returns>
        /// <remarks>This returns a texture which matches the vanilla layout. For example, for a fruit tree this would return the area containing all the sprites for the tree's growth stages and states in the same layout as the vanilla tilesheet.</remarks>
        bool TryGetCustomSprite(object entity, out Texture2D texture, out Rectangle sourceRect);

        /// <summary>Get the custom spritesheet for an in-game entity, and the source rectangle containing all sprites for this entity.</summary>
        /// <param name="entity">The in-world entity (e.g. fruit tree).</param>
        /// <param name="texture">The custom sprite texture.</param>
        /// <param name="sourceRect">The custom area within the texture containing the spritesheet.</param>
        /// <returns>Returns true if the entity has a custom sprite, else false.</returns>
        /// <remarks>This returns a texture which matches the vanilla layout. For example, for a fruit tree this would return the area containing all the sprites for the tree's growth stages and states in the same layout as the vanilla tilesheet.</remarks>
        bool TryGetCustomSpriteSheet(object entity, out Texture2D texture, out Rectangle sourceRect);
    }
}
