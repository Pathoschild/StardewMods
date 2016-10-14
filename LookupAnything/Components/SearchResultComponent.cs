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

        public SearchResultComponent(SearchResult result)
        {
            this.panel = new ClickableComponent(Rectangle.Empty, result.Name);
            this.Result = result;
        }

        public bool containsPoint(int x, int y)
        {
            return this.panel.containsPoint(x, y);
        }

        public Vector2 draw(SpriteBatch contentBatch, Vector2 position, int width, bool highlight = false)
        {
            int height = 70;
            this.panel = new ClickableComponent(new Rectangle((int)position.X, (int)position.Y, width, height), this.Result.Name);

            // draw highlight
            if (highlight)
                contentBatch.DrawLine(position.X, position.Y, new Vector2(width, height), Color.Beige);

            // draw border
            contentBatch.DrawLine(position.X, position.Y, new Vector2(width, 2), Color.Black);

            // draw text
            contentBatch.DrawTextBlock(Game1.smallFont, $"({this.Result.TargetType}) {this.Result.Name}", position + new Vector2(70, height / 2), width);                                  

            // draw icon
            this.Result.Subject.Value.DrawPortrait(contentBatch, position, new Vector2(70, 70));
            return new Vector2(width, height);
        }
    }
}
