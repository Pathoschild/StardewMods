using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework
{
    /// <summary>Renders debug information to the screen.</summary>
    internal class DebugOverlay : BaseOverlay
    {
        /*********
        ** Properties
        *********/
        /// <summary>The size of the margin around the displayed legend.</summary>
        private readonly int Margin = 30;

        /// <summary>The padding between the border and content.</summary>
        private readonly int Padding = 5;

        /// <summary>The content helper from which to read textures.</summary>
        private readonly IContentHelper Content;

        /// <summary>The spritesheets to render.</summary>
        private readonly string[] TextureNames;

        /// <summary>The current spritesheet to display.</summary>
        private string CurrentName;

        /// <summary>The current texture to display.</summary>
        private Texture2D CurrentTexture;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentHelper">The content helper from which to read textures.</param>
        public DebugOverlay(IContentHelper contentHelper)
        {
            this.Content = contentHelper;
            this.TextureNames = this.GetTextureNames(contentHelper).OrderBy(p => p).ToArray();
            this.NextTexture();
        }

        /// <summary>Switch to the next texture.</summary>
        public void NextTexture()
        {
            int index = Array.IndexOf(this.TextureNames, this.CurrentName) + 1;
            if (index >= this.TextureNames.Length)
                index = 0;
            this.CurrentName = this.TextureNames[index];
            this.CurrentTexture = this.Content.Load<Texture2D>(this.CurrentName, ContentSource.GameContent);
        }

        /// <summary>Switch to the previous data map.</summary>
        public void PrevTexture()
        {
            int index = Array.IndexOf(this.TextureNames, this.CurrentName) - 1;
            if (index < 0)
                index = this.TextureNames.Length - 1;
            this.CurrentName = this.TextureNames[index];
            this.CurrentTexture = this.Content.Load<Texture2D>(this.CurrentName, ContentSource.GameContent);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Draw to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        protected override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 labelSize = Game1.smallFont.MeasureString(this.CurrentName);
            int contentWidth = (int)Math.Max(labelSize.X, this.CurrentTexture?.Width ?? 0);

            this.DrawScroll(spriteBatch, this.Margin, this.Margin, contentWidth, (int)labelSize.Y + this.Padding + (this.CurrentTexture?.Height ?? (int)labelSize.Y), out Vector2 contentPos, out Rectangle scrollBounds);
            spriteBatch.DrawString(Game1.smallFont, this.CurrentName, new Vector2(contentPos.X + ((contentWidth - labelSize.X) / 2), contentPos.Y), Color.Black);

            if (this.CurrentTexture != null)
                spriteBatch.Draw(this.CurrentTexture, contentPos + new Vector2(0, labelSize.Y + this.Padding), Color.White);
            else
                spriteBatch.DrawString(Game1.smallFont, "(null)", contentPos + new Vector2(0, labelSize.Y + this.Padding), Color.Black);
        }

        /// <summary>Draw a scroll background.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="x">The top-left X pixel coordinate at which to draw the scroll.</param>
        /// <param name="y">The top-left Y pixel coordinate at which to draw the scroll.</param>
        /// <param name="contentWidth">The scroll content's pixel width.</param>
        /// <param name="contentHeight">The scroll content's pixel height.</param>'
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The scroll's outer bounds.</param>
        private void DrawScroll(SpriteBatch spriteBatch, int x, int y, int contentWidth, int contentHeight, out Vector2 contentPos, out Rectangle bounds)
        {
            Rectangle corner = Sprites.Legend.TopLeft;
            int cornerWidth = corner.Width * Game1.pixelZoom;
            int cornerHeight = corner.Height * Game1.pixelZoom;
            int innerWidth = contentWidth + this.Padding * 2;
            int innerHeight = contentHeight + this.Padding * 2;
            int outerWidth = innerWidth + cornerWidth * 2;
            int outerHeight = innerHeight + cornerHeight * 2;

            // draw scroll background
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth, y + cornerHeight, innerWidth, innerHeight), Sprites.Legend.Background, Color.White);

            // draw borders
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth, y, innerWidth, cornerHeight), Sprites.Legend.Top, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth, y + cornerHeight + innerHeight, innerWidth, cornerHeight), Sprites.Legend.Bottom, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x, y + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Left, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Right, Color.White);

            // draw corners
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x, y, cornerWidth, cornerHeight), Sprites.Legend.TopLeft, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomLeft, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth + innerWidth, y, cornerWidth, cornerHeight), Sprites.Legend.TopRight, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomRight, Color.White);

            // set out params
            contentPos = new Vector2(x + cornerWidth + this.Padding, y + cornerHeight + this.Padding);
            bounds = new Rectangle(x, y, outerWidth, outerHeight);
        }

        /// <summary>Get all texture asset names in the given content helper.</summary>
        /// <param name="contentHelper">The content helper to search.</param>
        private IEnumerable<string> GetTextureNames(IContentHelper contentHelper)
        {
            // get all texture keys from the content helper (this is such a hack)
            IList<string> textureKeys = new List<string>();
            contentHelper.InvalidateCache(asset =>
            {
                if (asset.DataType == typeof(Texture2D) && !asset.AssetName.Contains("..") && !asset.AssetName.StartsWith(Constants.ExecutionPath))
                    textureKeys.Add(asset.AssetName);
                return false;
            });
            return textureKeys;
        }
    }
}
