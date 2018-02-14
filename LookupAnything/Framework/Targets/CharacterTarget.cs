using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about an NPC.</summary>
    internal class CharacterTarget : GenericTarget
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public CharacterTarget(TargetType type, NPC obj, Vector2? tilePosition, IReflectionHelper reflectionHelper)
            : base(type, obj, tilePosition)
        {
            this.Reflection = reflectionHelper;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetSpriteArea()
        {
            NPC npc = (NPC)this.Value;
            var boundingBox = npc.GetBoundingBox(); // the 'occupied' area at the NPC's feet

            // calculate y origin
            float yOrigin;
            if (npc is DustSpirit)
                yOrigin = boundingBox.Bottom;
            else if (npc is Bat)
                yOrigin = boundingBox.Center.Y;
            else if (npc is Bug)
                yOrigin = boundingBox.Top - npc.sprite.spriteHeight * Game1.pixelZoom + (float)(System.Math.Sin(Game1.currentGameTime.TotalGameTime.Milliseconds / 1000.0 * (2.0 * System.Math.PI)) * 10.0);
            else if (npc is SquidKid squidKid)
            {
                int yOffset = this.Reflection.GetField<int>(squidKid, "yOffset").GetValue();
                yOrigin = boundingBox.Bottom - npc.sprite.spriteHeight * Game1.pixelZoom + yOffset;
            }
            else
                yOrigin = boundingBox.Top;

            // get bounding box
            int height = npc.sprite.spriteHeight * Game1.pixelZoom;
            int width = npc.sprite.spriteWidth * Game1.pixelZoom;
            float x = boundingBox.Center.X - (width / 2);
            float y = yOrigin + boundingBox.Height - height + npc.yJumpOffset * 2;

            return new Rectangle((int)(x - Game1.viewport.X), (int)(y - Game1.viewport.Y), width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            NPC npc = (NPC)this.Value;

            // allow any part of the sprite area for monsters
            // (Monsters have complicated and inconsistent sprite behaviour which isn't really
            // worth reverse-engineering, and sometimes move around so much that a pixel-perfect
            // check is inconvenient anyway.)
            if (npc is Monster)
                return spriteArea.Contains((int)position.X, (int)position.Y);

            // check sprite for non-monster NPCs
            SpriteEffects spriteEffects = npc.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, npc.sprite.Texture, npc.sprite.sourceRect, spriteEffects);
        }
    }
}
