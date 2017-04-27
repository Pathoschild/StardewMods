using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;

namespace TractorMod.Framework
{
    public class Tractor : Horse
    {
        /*********
        ** Public methods
        *********/
        public Tractor() : base() { }

        public Tractor(int tileX, int tileY) : base(tileX, tileY)
        {
            this.sprite = new AnimatedSprite(Game1.content.Load<Texture2D>("..\\Mods\\TractorMod\\assets\\tractor"), 0, 32, 32);
            this.sprite.textureUsesFlippedRightForLeft = true;
            this.sprite.loop = true;
            this.faceDirection(3);
        }

        public override Rectangle GetBoundingBox()
        {
            Rectangle boundingBox = base.GetBoundingBox();
            if ((this.facingDirection == 0 || this.facingDirection == 2))
                boundingBox.Inflate(-Game1.tileSize / 2 - Game1.pixelZoom, 0);
            return boundingBox;
        }
    }
}
