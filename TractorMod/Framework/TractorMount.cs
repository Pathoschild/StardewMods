using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The in-game tractor that can be ridden by the player.</summary>
    internal sealed class TractorMount : Horse
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The internal NPC name.</param>
        /// <param name="tileX">The initial tile X position.</param>
        /// <param name="tileY">The initial tile Y position.</param>
        /// <param name="content">The content helper with which to load the tractor sprite.</param>
        public TractorMount(string name, int tileX, int tileY, IContentHelper content)
            : base(tileX, tileY)
        {
            this.name = name;
            this.sprite = new AnimatedSprite(content.Load<Texture2D>(@"assets\tractor.png"), 0, 32, 32)
            {
                textureUsesFlippedRightForLeft = true,
                loop = true
            };
            this.faceDirection(3);
        }

        /// <summary>Get the bounding box for collision checks.</summary>
        public override Rectangle GetBoundingBox()
        {
            Rectangle boundingBox = base.GetBoundingBox();
            if ((this.facingDirection == 0 || this.facingDirection == 2))
                boundingBox.Inflate(-Game1.tileSize / 2 - Game1.pixelZoom, 0);
            return boundingBox;
        }
    }
}
