using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.DataMaps.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewValley;
using XRectangle = xTile.Dimensions.Rectangle;

namespace Pathoschild.Stardew.DataMaps.Overlays
{
    /// <summary>An on-screen menu that shows a legend of color => label mappings.</summary>
    internal class LegendComponent
    {
        /*********
        ** Properties
        *********/
        /// <summary>The values to show.</summary>
        private readonly Tuple<Color, string>[] Values;

        /// <summary>The UI bounds to draw.</summary>
        private Rectangle Bounds;

        /// <summary>The pixel size of a color box.</summary>
        private int ColorBoxSize;

        /// <summary>The maximum label width.</summary>
        private int ContentWidth;

        /// <summary>The combined height of the labels.</summary>
        private int ContentHeight;

        /// <summary>The number of pixels between the color box and its label.</summary>
        private readonly int ColorBoxPadding = 5;

        /// <summary>The size of the margin around the displayed legend.</summary>
        private readonly int Margin = 30;

        /// <summary>The padding between the border and content.</summary>
        private readonly int Padding = 5;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="values">The values to show.</param>
        public LegendComponent(params Tuple<Color, string>[] values)
        {
            this.Values = values;
            if (values.Any())
            {
                this.ColorBoxSize = (int)Game1.smallFont.MeasureString("X").Y;
                this.ContentWidth = this.ColorBoxSize + this.ColorBoxPadding + (int)values.Select(p => Game1.smallFont.MeasureString(p.Item2).X).Max();
                this.ContentHeight = values.Length * this.ColorBoxSize;
            }
            this.RecalculateDimensions(values, Game1.smallFont);
        }

        /// <summary>The method called when the game window changes size.</summary>
        /// <param name="oldBounds">The former viewport.</param>
        /// <param name="newBounds">The new viewport.</param>
        public void ReceiveWindowSizeChanged(XRectangle oldBounds, XRectangle newBounds)
        {
            this.RecalculateDimensions(this.Values, Game1.smallFont);
        }

        /// <summary>Render the UI.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!this.Values.Any())
                return;

            // calculate dimensions
            var bounds = this.Bounds;
            int cornerWidth = Sprites.Legend.TopLeft.Width * Game1.pixelZoom;
            int cornerHeight = Sprites.Legend.TopLeft.Height * Game1.pixelZoom;
            int innerWidth = bounds.Width - cornerWidth * 2;
            int innerHeight = bounds.Height - cornerHeight * 2;

            // draw scroll background
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left + cornerWidth, bounds.Top + cornerHeight, innerWidth, innerHeight), Sprites.Legend.Background, Color.White);

            // draw borders
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left + cornerWidth, bounds.Top, innerWidth, cornerHeight), Sprites.Legend.Top, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left + cornerWidth, bounds.Bottom - cornerHeight, innerWidth, cornerHeight), Sprites.Legend.Bottom, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left, bounds.Top + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Left, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Right - cornerWidth, bounds.Top + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Right, Color.White);

            // draw corners
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left, bounds.Top, cornerWidth, cornerHeight), Sprites.Legend.TopLeft, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left, bounds.Bottom - cornerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomLeft, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Right - cornerWidth, bounds.Top, cornerWidth, cornerHeight), Sprites.Legend.TopRight, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Right - cornerWidth, bounds.Bottom - cornerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomRight, Color.White);

            // draw text
            for (int i = 0; i < this.Values.Length; i++)
            {
                Tuple<Color, string> value = this.Values[i];
                int leftOffset = bounds.X + cornerWidth + this.Padding;
                int topOffset = bounds.Y + cornerHeight + this.Padding + i * this.ColorBoxSize;

                spriteBatch.DrawLine(leftOffset, topOffset, new Vector2(this.ColorBoxSize), value.Item1);
                spriteBatch.DrawString(Game1.smallFont, value.Item2, new Vector2(leftOffset + this.ColorBoxSize + this.ColorBoxPadding, topOffset + 2), Color.Black);
            }
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Recalculate the legend's position and dimensions.</summary>
        /// <param name="values">The values to display in the legend.</param>
        /// <param name="font">The font used to display text.</param>
        private void RecalculateDimensions(Tuple<Color, string>[] values, SpriteFont font)
        {
            // get corner dimensions
            var corner = Sprites.Legend.TopLeft;
            int cornerWidth = corner.Width * Game1.pixelZoom;
            int cornerHeight = corner.Height * Game1.pixelZoom;

            // calculate legend dimensions
            this.ColorBoxSize = (int)Game1.smallFont.MeasureString("X").Y;
            this.ContentWidth = this.ColorBoxSize + this.ColorBoxPadding + (int)values.Select(p => font.MeasureString(p.Item2).X).Max();
            this.ContentHeight = values.Length * this.ColorBoxSize;
            this.Bounds = new Rectangle(
                x: this.Margin,
                y: this.Margin,
                width: this.ContentWidth + (cornerWidth + this.Padding) * 2,
                height: this.ContentHeight + (cornerHeight + this.Padding) * 2
            );
        }
    }
}
