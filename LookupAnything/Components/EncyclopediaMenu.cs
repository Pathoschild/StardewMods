using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Fields;
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
        private ISubject Subject { get; }

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
            float leftOffset = 15;
            float topOffset = 15;

            // get font
            SpriteFont font = Game1.smallFont;
            float blankLineHeight = font.MeasureString("ABC").Y;
            float spaceWidth = Sprites.GetSpaceWidth(font);

            // draw background
            sprites.DrawBlock(Sprites.Letter.Sheet, Sprites.Letter.Sprite, x, y, scale: this.width / (float)Sprites.Letter.Sprite.Width);

            // draw portrait
            if (subject.DrawPortrait(sprites, new Vector2(x + leftOffset, y + topOffset), new Vector2(70, 70)))
                leftOffset += 72;

            // draw text
            float wrapWidth = this.width - leftOffset - rightOffset;
            {
                // draw name & item type
                {
                    Vector2 nameSize = sprites.DrawStringBlock(font, $"{subject.Name}.", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                    Vector2 typeSize = sprites.DrawStringBlock(font, $"{subject.Type}.", new Vector2(x + leftOffset + nameSize.X + spaceWidth, y + topOffset), wrapWidth);
                    topOffset += Math.Max(nameSize.Y, typeSize.Y);
                }

                // draw description
                if (subject.Description != null)
                {
                    Vector2 size = sprites.DrawStringBlock(font, subject.Description, new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                    topOffset += size.Y;
                }

                // draw spacer
                topOffset += blankLineHeight;

                // draw custom fields
                ICustomField[] fields = this.Subject.CustomFields;
                if (fields != null && fields.Any())
                {
                    float cellPadding = 3;
                    float labelWidth = fields.Max(p => font.MeasureString(p.Label).X);
                    float valueWidth = wrapWidth - labelWidth - cellPadding * 4;
                    foreach (ICustomField field in fields)
                    {
                        if (!field.HasValue)
                            continue;

                        // draw label & value
                        Vector2 labelSize = sprites.DrawStringBlock(font, field.Label, new Vector2(x + leftOffset + cellPadding, y + topOffset + cellPadding), wrapWidth);
                        Vector2 valuePosition = new Vector2(x + leftOffset + labelWidth + cellPadding * 3, y + topOffset + cellPadding);
                        Vector2 valueSize =
                            field.DrawValue(sprites, font, valuePosition, valueWidth)
                            ?? sprites.DrawStringBlock(font, field.Value, valuePosition, valueWidth);
                        Vector2 rowSize = new Vector2(labelWidth + valueWidth + cellPadding * 4, Math.Max(labelSize.Y, valueSize.Y));

                        // draw table row
                        Color lineColor = Color.Gray;
                        int borderWidth = 1;
                        sprites.DrawLine(x + leftOffset, y + topOffset, new Vector2(rowSize.X, borderWidth), lineColor); // top
                        sprites.DrawLine(x + leftOffset, y + topOffset + rowSize.Y, new Vector2(rowSize.X, borderWidth), lineColor); // bottom
                        sprites.DrawLine(x + leftOffset, y + topOffset, new Vector2(borderWidth, rowSize.Y), lineColor); // left
                        sprites.DrawLine(x + leftOffset + labelWidth + cellPadding * 2, y + topOffset, new Vector2(borderWidth, rowSize.Y), lineColor); // middle
                        sprites.DrawLine(x + leftOffset + rowSize.X, y + topOffset, new Vector2(borderWidth, rowSize.Y), lineColor); // right

                        // update offset
                        topOffset += valueSize.Y;
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
    }
}
