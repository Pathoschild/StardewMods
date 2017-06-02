using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace TractorMod.Framework
{
    internal class Tractor : Horse
    {
        /*********
        ** Public methods
        *********/
        public Tractor(string name, int tileX, int tileY, IContentHelper content)
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

        public override Rectangle GetBoundingBox()
        {
            Rectangle boundingBox = base.GetBoundingBox();
            if ((this.facingDirection == 0 || this.facingDirection == 2))
                boundingBox.Inflate(-Game1.tileSize / 2 - Game1.pixelZoom, 0);
            return boundingBox;
        }
    }
}
