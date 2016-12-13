﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.LookupAnything.Components
{
    /// <summary>A UI which shows information about an item.</summary>
    internal class LookupMenu : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The subject metadata.</summary>
        private readonly ISubject Subject;

        /// <summary>Encapsulates logging and monitoring.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The data to display for this subject.</summary>
        private readonly ICustomField[] Fields;

        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The maximum pixels to scroll.</summary>
        private int MaxScroll;

        /// <summary>The number of pixels to scroll.</summary>
        private int CurrentScroll;

        /// <summary>Scroll amount configured by the user.</summary>
        private int ScrollAmount;

        /// <summary>Arrow icon locations for click functionality.</summary>
        private Rectangle UpIcon;
        private Rectangle DownIcon;

        /// <summary>Whether the game's draw mode has been validated for compatibility.</summary>
        private bool ValidatedDrawMode;


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="subject">The metadata to display.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        /// <param name="scroll">Scroll amount configured by the user.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public LookupMenu(ISubject subject, Metadata metadata, IMonitor monitor, int scroll, IReflectionHelper reflectionHelper)
        {
            this.Subject = subject;
            this.Fields = subject.GetData(metadata).Where(p => p.HasValue).ToArray();
            this.Monitor = monitor;
            this.ScrollAmount = scroll;
            this.Reflection = reflectionHelper;
            this.CalculateDimensions();
        }

        /****
        ** Events
        ****/
        /// <summary>The method invoked when the player left-clicks on the lookup UI.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            this.HandleCursorLeftClick(x, y);
        }

        /// <summary>The method invoked when the player right-clicks on the lookup UI.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        /// <summary>The method invoked when the player scrolls the mouse wheel on the lookup UI.</summary>
        /// <param name="direction">The scroll direction.</param>
        public override void receiveScrollWheelAction(int direction)
        {
            if (direction > 0)          // Positive number scrolls window content up
                this.ScrollUp();
            else
                this.ScrollDown();
        }

        /// <summary>The method called when the game window changes size.</summary>
        /// <param name="oldBounds">The former viewport.</param>
        /// <param name="newBounds">The new viewport.</param>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.CalculateDimensions();
        }

        /// <summary>The method called when the player presses a controller button.</summary>
        /// <param name="button">The controller button pressed.</param>
        public override void receiveGamePadButton(Buttons button)
        {
            switch (button)
            {
                case Buttons.A:
                    Point p = Game1.getMousePosition();
                    this.HandleCursorLeftClick(p.X, p.Y);
                    break;
                case Buttons.B:
                    this.exitThisMenu();
                    break;
                case Buttons.RightThumbstickUp:
                    this.ScrollUp();
                    break;
                case Buttons.RightThumbstickDown:
                    this.ScrollDown();
                    break;
                default:
                    base.receiveGamePadButton(button);
                    break;
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Scroll up the menu content by the specified amount (if possible).</summary>
        public void ScrollUp()
        {
            this.CurrentScroll -= this.ScrollAmount;
        }

        /// <summary>Scroll down the menu content by the specified amount (if possible).</summary>
        public void ScrollDown()
        {
            this.CurrentScroll += this.ScrollAmount;
        }

        /// <summary>Combines the Left Click action with the equivalent controller button. All left click actions should be handled here only.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        public void HandleCursorLeftClick(int x, int y)
        {
            // Close menu when clicking outside of it
            if (!this.isWithinBounds(x, y))
                this.exitThisMenu();

            // Add click action to scroll icons
            if (UpIcon.Contains(x, y))
                this.ScrollUp();
            if (DownIcon.Contains(x, y))
                this.ScrollDown();
        }

        /// <summary>Render the UI.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public override void draw(SpriteBatch spriteBatch)
        {
            this.Monitor.InterceptErrors("drawing the lookup info", () =>
            {
                ISubject subject = this.Subject;

                // disable when game is using immediate sprite sorting
                // (This prevents Lookup Anything from creating new sprite batches, which breaks its core rendering logic.
                // Fortunately this very rarely happens; the only known case is the Stardew Valley Fair, when the only thing
                // you can look up anyway is the farmer.)
                if (!this.ValidatedDrawMode)
                {
                    IPrivateField<SpriteSortMode> sortModeField =
                        this.Reflection.GetPrivateField<SpriteSortMode>(Game1.spriteBatch, "spriteSortMode", required: false) // XNA
                        ?? this.Reflection.GetPrivateField<SpriteSortMode>(Game1.spriteBatch, "_sortMode"); // MonoGame
                    if (sortModeField.GetValue() == SpriteSortMode.Immediate)
                    {
                        this.Monitor.Log("Aborted the lookup because the game's current rendering mode isn't compatible with the mod's UI. This only happens in rare cases (e.g. the Stardew Valley Fair).", LogLevel.Warn);
                        this.exitThisMenu(playSound: false);
                        return;
                    }
                    this.ValidatedDrawMode = true;
                }

                // calculate dimensions
                int x = this.xPositionOnScreen;
                int y = this.yPositionOnScreen;
                const int gutter = 15;
                float leftOffset = gutter;
                float topOffset = gutter;
                float contentWidth = this.width - gutter * 2;
                float contentHeight = this.height - gutter * 2;
                int tableBorderWidth = 1;

                // get font
                SpriteFont font = Game1.smallFont;
                float lineHeight = font.MeasureString("ABC").Y;
                float spaceWidth = DrawHelper.GetSpaceWidth(font);

                // draw background
                // (This uses a separate sprite batch because it needs to be drawn before the
                // foreground batch, and we can't use the foreground batch because the background is
                // outside the clipping area.)
                using (SpriteBatch backgroundBatch = new SpriteBatch(Game1.graphics.GraphicsDevice))
                {
                    backgroundBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                    backgroundBatch.DrawSprite(Sprites.Letter.Sheet, Sprites.Letter.Sprite, x, y, scale: this.width / (float)Sprites.Letter.Sprite.Width);
                    backgroundBatch.End();
                }

                // draw foreground
                // (This uses a separate sprite batch to set a clipping area for scrolling.)
                using (SpriteBatch contentBatch = new SpriteBatch(Game1.graphics.GraphicsDevice))
                {
                    // begin draw
                    GraphicsDevice device = Game1.graphics.GraphicsDevice;
                    device.ScissorRectangle = new Rectangle(x + gutter, y + gutter, (int)contentWidth, (int)contentHeight);
                    contentBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, new RasterizerState { ScissorTestEnable = true });

                    // scroll view
                    this.CurrentScroll = Math.Max(0, this.CurrentScroll); // don't scroll past top
                    this.CurrentScroll = Math.Min(this.MaxScroll, this.CurrentScroll); // don't scroll past bottom
                    topOffset -= this.CurrentScroll; // scrolled down == move text up

                    // draw portrait
                    if (subject.DrawPortrait(contentBatch, new Vector2(x + leftOffset, y + topOffset), new Vector2(70, 70)))
                        leftOffset += 72;

                    // draw fields
                    float wrapWidth = this.width - leftOffset - gutter;
                    {
                        // draw name & item type
                        {
                            Vector2 nameSize = contentBatch.DrawTextBlock(font, $"{subject.Name}.", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                            Vector2 typeSize = contentBatch.DrawTextBlock(font, $"{subject.Type}.", new Vector2(x + leftOffset + nameSize.X + spaceWidth, y + topOffset), wrapWidth);
                            topOffset += Math.Max(nameSize.Y, typeSize.Y);
                        }

                        // draw description
                        if (subject.Description != null)
                        {
                            Vector2 size = contentBatch.DrawTextBlock(font, subject.Description?.Replace(Environment.NewLine, " "), new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                            topOffset += size.Y;
                        }

                        // draw spacer
                        topOffset += lineHeight;

                        // draw custom fields
                        if (this.Fields.Any())
                        {
                            ICustomField[] fields = this.Fields;
                            float cellPadding = 3;
                            float labelWidth = fields.Where(p => p.HasValue).Max(p => font.MeasureString(p.Label).X);
                            float valueWidth = wrapWidth - labelWidth - cellPadding * 4 - tableBorderWidth;
                            foreach (ICustomField field in fields)
                            {
                                if (!field.HasValue)
                                    continue;

                                // draw label & value
                                Vector2 labelSize = contentBatch.DrawTextBlock(font, field.Label, new Vector2(x + leftOffset + cellPadding, y + topOffset + cellPadding), wrapWidth);
                                Vector2 valuePosition = new Vector2(x + leftOffset + labelWidth + cellPadding * 3, y + topOffset + cellPadding);
                                Vector2 valueSize =
                                    field.DrawValue(contentBatch, font, valuePosition, valueWidth)
                                    ?? contentBatch.DrawTextBlock(font, field.Value, valuePosition, valueWidth);
                                Vector2 rowSize = new Vector2(labelWidth + valueWidth + cellPadding * 4, Math.Max(labelSize.Y, valueSize.Y));

                                // draw table row
                                Color lineColor = Color.Gray;
                                contentBatch.DrawLine(x + leftOffset, y + topOffset, new Vector2(rowSize.X, tableBorderWidth), lineColor); // top
                                contentBatch.DrawLine(x + leftOffset, y + topOffset + rowSize.Y, new Vector2(rowSize.X, tableBorderWidth), lineColor); // bottom
                                contentBatch.DrawLine(x + leftOffset, y + topOffset, new Vector2(tableBorderWidth, rowSize.Y), lineColor); // left
                                contentBatch.DrawLine(x + leftOffset + labelWidth + cellPadding * 2, y + topOffset, new Vector2(tableBorderWidth, rowSize.Y), lineColor); // middle
                                contentBatch.DrawLine(x + leftOffset + rowSize.X, y + topOffset, new Vector2(tableBorderWidth, rowSize.Y), lineColor); // right

                                // update offset
                                topOffset += Math.Max(labelSize.Y, valueSize.Y);
                            }
                        }
                    }

                    // update max scroll
                    this.MaxScroll = Math.Max(0, (int)(topOffset - contentHeight + this.CurrentScroll));

                    // draw scroll icons
                    Rectangle Up = Sprites.Icons.UpArrow;       // Shorter name to make code cleaner
                    Rectangle Down = Sprites.Icons.DownArrow;   // Shorter name to make code cleaner
                    this.UpIcon = new Rectangle(x + gutter, (int)(y + contentHeight - Up.Height - gutter - Down.Height), Up.Height, Up.Width);
                    this.DownIcon = new Rectangle(x + gutter, (int)(y + contentHeight - Down.Height), Down.Height, Down.Width);
                    if (this.MaxScroll > 0 && this.CurrentScroll > 0)
                        contentBatch.DrawSprite(Sprites.Icons.Sheet, Sprites.Icons.UpArrow, this.UpIcon.X, this.UpIcon.Y);
                    if (this.MaxScroll > 0 && this.CurrentScroll < this.MaxScroll)
                        contentBatch.DrawSprite(Sprites.Icons.Sheet, Sprites.Icons.DownArrow, this.DownIcon.X, this.DownIcon.Y);

                    // end draw
                    contentBatch.End();
                }

                // draw cursor
                base.drawMouse(Game1.spriteBatch);
            }, this.OnDrawError);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Calculate the rendered dimensions based on the current game scale.</summary>
        private void CalculateDimensions()
        {
            this.width = Math.Min(Game1.tileSize * 14, Game1.viewport.Width);
            this.height = Math.Min((int)(this.AspectRatio.Y / this.AspectRatio.X * this.width), Game1.viewport.Height);

            Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)origin.X;
            this.yPositionOnScreen = (int)origin.Y;
        }

        /// <summary>The method invoked when an unhandled exception is intercepted.</summary>
        /// <param name="ex">The intercepted exception.</param>
        private void OnDrawError(Exception ex)
        {
            this.Monitor.InterceptErrors("handling an error in the lookup code", () => this.exitThisMenu());
        }
    }
}
