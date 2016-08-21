using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.LookupAnything.Components
{
    /// <summary>A UI which shows information about an item.</summary>
    internal class EncyclopediaMenu : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The encyclopedia subject.</summary>
        private ISubject Subject { get; set; }

        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public EncyclopediaMenu(ISubject subject)
        {
            this.Subject = subject;
            this.CalculateDimensions();
        }

        /// <summary>The method invoked when the player left-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true) { }

        /// <summary>The method invoked when the player right-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public override void draw(SpriteBatch sprites)
        {
            ISubject subject = this.Subject;

            // calculate dimensions
            this.CalculateDimensions();
            int x = this.xPositionOnScreen;
            int y = this.yPositionOnScreen;
            const int rightOffset = 15;
            int leftOffset = 15;
            int topOffset = 15;

            // get font
            SpriteFont font = Game1.smallFont;
            int blankLineHeight = (int)font.MeasureString("ABC").Y;
            int spaceSize = (int)font.MeasureString(" ").X;

            // draw background
            sprites.Draw(Sprites.Letter.Sheet, Sprites.Letter.Sprite, x, y, this.width, this.height, scale: this.width / (float)Sprites.Letter.Sprite.Width);

            // draw portrait
            if (subject.DrawPortrait(sprites, new Vector2(x + leftOffset, y + topOffset), new Vector2(50, 50)))
                leftOffset += 70;

            // draw text
            int wrapWidth = this.width - leftOffset - rightOffset;
            {
                // draw name & item type
                {
                    var nameSize = sprites.DrawStringBlock(font, $"{subject.Name}.", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                    var typeSize = sprites.DrawStringBlock(font, $"{subject.Type}.", new Vector2(x + leftOffset + nameSize.Width + spaceSize, y + topOffset), wrapWidth);
                    topOffset += Math.Max(nameSize.Height, typeSize.Height);
                }

                // draw description
                if (subject.Description != null)
                {
                    var size = sprites.DrawStringBlock(font, subject.Description, new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                    topOffset += size.Height;
                }

                // draw spacer
                topOffset += blankLineHeight;

                // draw price
                if (subject.SalePrice != null)
                {
                    var size = sprites.DrawStringBlock(font, $"Sells for {subject.SalePrice}", new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                    this.DrawCoin(sprites, x + leftOffset + size.Width + 1, y + topOffset);
                    topOffset += size.Height;
                }
                topOffset += blankLineHeight;

                // NPCs who like this item
                if (subject.GiftTastes?.Any() == true)
                {
                    var size = sprites.DrawStringBlock(font, "can be gifted:", new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                    topOffset += size.Height;
                    foreach (GiftTaste taste in new [] { GiftTaste.Love, GiftTaste.Like, GiftTaste.Neutral, GiftTaste.Dislike, GiftTaste.Hate })
                    {
                        if (!subject.GiftTastes.ContainsKey(taste))
                            continue;

                        string[] names = subject.GiftTastes[taste].Select(p => p.getName()).OrderBy(p => p).ToArray();
                        var labelSize = sprites.DrawStringBlock(Game1.tinyFont, $"{taste}:", new Vector2(x + leftOffset + spaceSize, y + topOffset), wrapWidth - spaceSize, scale: 0.5f);
                        var listSize = sprites.DrawStringBlock(Game1.tinyFont, $"{string.Join(", ", names)}.", new Vector2(labelSize.X + labelSize.Width + spaceSize, y + topOffset), wrapWidth - labelSize.Width - spaceSize, scale: 0.5f);
                        topOffset += Math.Max(labelSize.Height, listSize.Height);
                    }
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Calculate the rendered dimensions based on the current game scale.</summary>
        private void CalculateDimensions()
        {
            this.width = Game1.tileSize * 13;
            this.height = (int)(this.AspectRatio.Y / this.AspectRatio.X * this.width);
            Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)origin.X;
            this.yPositionOnScreen = (int)origin.Y;
        }

        /// <summary>Draw a coin icon at the specified position.</summary>
        /// <param name="sprites">The sprites to render.</param>
        /// <param name="x">The top-left X-position at which to draw the coin.</param>
        /// <param name="y">The top-left Y-position at which to draw the coin.</param>
        private void DrawCoin(SpriteBatch sprites, int x, int y)
        {
            const int coinSize = 6;
            sprites.Draw(Game1.debrisSpriteSheet, new Vector2(x, y), new Rectangle(5, 69, coinSize, coinSize), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
        }
    }
}
