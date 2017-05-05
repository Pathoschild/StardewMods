using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace TractorMod.Framework
{
    internal class TractorHouse : Building
    {
        /*********
        ** Public methods
        *********/
        public TractorHouse(IContentHelper content)
        {
            buildingType = "Tractor House";
            humanDoor = new Point(-1, -1);
            animalDoor = new Point(-2, -1);
            indoors = null;
            nameOfIndoors = "";
            baseNameOfIndoors = "";
            nameOfIndoorsWithoutUnique = "";
            magical = false;
            tileX = 0;
            tileY = 0;
            maxOccupants = 0;
            tilesWide = 4;
            tilesHigh = 2;
            texture = content.Load<Texture2D>(@"assets\TractorHouse.png", ContentSource.ModFolder);
            daysOfConstructionLeft = 1;
        }

        public TractorHouse SetDaysOfConstructionLeft(int input)
        {
            daysOfConstructionLeft = input;
            return this;
        }

        public override bool intersects(Rectangle boundingBox)
        {
            if (daysOfConstructionLeft > 0)
                return base.intersects(boundingBox);
            if (!base.intersects(boundingBox))
                return false;
            if (boundingBox.X >= (this.tileX + 1) * Game1.tileSize && boundingBox.Right < (this.tileX + 3) * Game1.tileSize)
                return boundingBox.Y <= (this.tileY + 1) * Game1.tileSize;
            return true;
        }

        public override void draw(SpriteBatch b)
        {
            if (this.daysOfConstructionLeft > 0)
            {
                this.drawInConstruction(b);
            }
            else
            {
                this.drawShadow(b);
                b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX * Game1.tileSize, this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize)), this.texture.Bounds, this.color * this.alpha, 0.0f, new Vector2(0.0f, this.texture.Bounds.Height), 4f, SpriteEffects.None, (this.tileY + this.tilesHigh - 1) * Game1.tileSize / 10000f);
            }
        }

        public override Rectangle getSourceRectForMenu()
        {
            return new Rectangle(0, 0, this.texture.Bounds.Width, this.texture.Bounds.Height);
        }
    }
}
