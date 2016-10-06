using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.LookupAnything.Components
{
    class SearchResultComponent
    {
        public SearchResult Result { get; private set; }

        private ClickableComponent panel;

        private int Width = 500;

        private int Height = 70;

        public SearchResultComponent(SearchResult result)
        {
            this.panel = new ClickableComponent(new Rectangle(0, 0, this.Width, this.Height), result.Name);
            this.Result = result;
        }

        public bool containsPoint(int x, int y)
        {
            return this.panel.containsPoint(x, y);
        }

        public Vector2 draw(SpriteBatch contentBatch, Vector2 position, float width)
        {
            contentBatch.DrawTextBlock(Game1.smallFont, $"({this.Result.TargetType}) {this.Result.Name}", position + new Vector2(70, this.Height / 2), width);

            contentBatch.DrawLine(position.X, position.Y, new Vector2(Width, 1), Color.Blue);
            contentBatch.DrawLine(position.X, position.Y, new Vector2(1, Height), Color.Red);
            contentBatch.DrawLine(position.X + Width, position.Y, new Vector2(1, Height), Color.Pink);
            contentBatch.DrawLine(position.X, position.Y + Height, new Vector2(Width, 1), Color.Green);

            this.panel = new ClickableComponent(new Rectangle((int)position.X, (int)position.Y, this.Width, this.Height), this.Result.Name);
            return new Vector2(this.Width, this.Height);
        }
    }
}
