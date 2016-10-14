using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.LookupAnything.Common;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Fields;
using Pathoschild.LookupAnything.Framework.Models;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.LookupAnything.Components
{
    /// <summary>A UI which shows information about an item.</summary>
    internal class SearchMenu : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The  metadata.</summary>
        private readonly Metadata Metadata;

        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);

        /// <summary>The maximum pixels to scroll.</summary>
        private int MaxScroll;

        /// <summary>The number of pixels to scroll.</summary>
        private int CurrentScroll;

        private readonly ILookup<string, SearchResult> SearchLookup;

        /// <summary> Editable Search Query </summary>
        private readonly SearchTextBox SearchTextbox;

        private IEnumerable<SearchResultComponent> SearchResults = Enumerable.Empty<SearchResultComponent>();

        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="subject">The metadata to display.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public SearchMenu(Metadata metadata)
        {
            this.Metadata = metadata;
            this.SearchLookup = GameHelper.GetSearchLookup(this.Metadata);
            this.CalculateDimensions();
            this.SearchTextbox = new SearchTextBox(Game1.smallFont, Color.Black);
            this.SearchTextbox.Select();
            this.SearchTextbox.Changed += SearchTextbox_Changed;
        }

        private void SearchTextbox_Changed(object sender, string searchString)
        {
            // avoid searching empty strings for better performance
            if (string.IsNullOrWhiteSpace(searchString))
            {
                this.SearchResults = Enumerable.Empty<SearchResultComponent>();
                return;
            }

            searchString = searchString.ToLowerInvariant();
            var scoreMerger = new Func<string, string, double>((string haystack, string needle) =>
            {
                // prefer starts with to contains (e.g. 'brea' yields 'bread/bream' over 'complete breakfast')
                if (haystack.StartsWith(needle))
                {
                    if (haystack.Length == needle.Length)
                    {
                        return 0; // same string
                    }
                    return 0.5;
                }
                if (haystack.Contains(needle))
                {
                    return 1.5;
                }
                return Sift3.Compare(haystack, needle, 5); // (e.g. 'breaf' yields 1 for 'bread', 'bream')
            });

            var debugResults = this.SearchLookup
                .Select(sr => new { Result = sr, Score = scoreMerger(sr.Key, searchString) })
                .OrderBy(r => r.Score)
                .Where(r => r.Score < 3) // experimentally, results under this score have to be horrendous typos to be relavent since the corpus is small 
                .Take(20) // the above filter should be aggressive, if there are many results, most probably come from the pre-sifted constants (e.g. 'ore' and we want to include them)
                .ToArray();

            var results = debugResults
                .Select(r => r.Result.First()); // only the first instance of results with this exact name

            this.SearchResults = results.Select(result => new SearchResultComponent(result)).ToArray();
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
            foreach (var result in this.SearchResults)
            {
                if (result.containsPoint(x, y))
                {
                    ISubject subject = result.Result.Subject.Value;
                    Game1.activeClickableMenu = new LookupMenu(subject, this.Metadata);
                    Game1.playSound("coin");
                }
            }
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
            this.CurrentScroll -= direction; // down direction == increased scroll
        }

        /// <summary>The method called when the game window changes size.</summary>
        /// <param name="oldBounds">The former viewport.</param>
        /// <param name="newBounds">The new viewport.</param>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.CalculateDimensions();
        }

        /****
        ** Methods
        ****/
        /// <summary>Scroll up the menu content by the specified amount (if possible).</summary>
        /// <param name="amount">The number of pixels to scroll.</param>
        public void ScrollUp(int amount)
        {
            this.CurrentScroll -= amount;
        }

        /// <summary>Scroll down the menu content by the specified amount (if possible).</summary>
        /// <param name="amount">The number of pixels to scroll.</param>
        public void ScrollDown(int amount)
        {
            this.CurrentScroll += amount;
        }

        public override void receiveKeyPress(Keys key)
        {
            // Override to deliberately avoid calling base and letting another key close the menu
        }

        /// <summary>Render the UI.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public override void draw(SpriteBatch spriteBatch)
        {
            // calculate dimensions
            int x = this.xPositionOnScreen;
            int y = this.yPositionOnScreen;
            const int gutter = 15;
            float leftOffset = gutter;
            float topOffset = gutter;
            float contentWidth = this.width - gutter * 2;
            float contentHeight = this.height - gutter * 2;

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

                // draw fields
                float wrapWidth = this.width - leftOffset - gutter;
                {
                    // draw name & item type
                    {
                        Vector2 nameSize = contentBatch.DrawTextBlock(font, $"Search UI.", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                        Vector2 typeSize = contentBatch.DrawTextBlock(font, $"(pre-alpha)", new Vector2(x + leftOffset + nameSize.X + spaceWidth, y + topOffset), wrapWidth);
                        topOffset += Math.Max(nameSize.Y, typeSize.Y);

                        this.SearchTextbox.X = (int)(x + leftOffset);
                        this.SearchTextbox.Y = (int)(y + topOffset);
                        this.SearchTextbox.Width = (int)wrapWidth;
                        this.SearchTextbox.Draw(contentBatch);
                        topOffset += this.SearchTextbox.Height;

                        foreach (var result in this.SearchResults)
                        {
                            var objSize = result.draw(contentBatch, new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                            topOffset += objSize.Y;
                        }
                    }

                    // draw spacer
                    topOffset += lineHeight;
                }

                // update max scroll
                this.MaxScroll = Math.Max(0, (int)(topOffset - contentHeight + this.CurrentScroll));

                // draw scroll icons
                if (this.MaxScroll > 0 && this.CurrentScroll > 0)
                    contentBatch.DrawSprite(Sprites.Icons.Sheet, Sprites.Icons.UpArrow, x + gutter, y + contentHeight - Sprites.Icons.DownArrow.Height - gutter - Sprites.Icons.UpArrow.Height);
                if (this.MaxScroll > 0 && this.CurrentScroll < this.MaxScroll)
                    contentBatch.DrawSprite(Sprites.Icons.Sheet, Sprites.Icons.DownArrow, x + gutter, y + contentHeight - Sprites.Icons.DownArrow.Height);

                // draw mouse cursor
                this.drawMouse(contentBatch);

                // end draw
                contentBatch.End();
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Calculate the rendered dimensions based on the current game scale.</summary>
        private void CalculateDimensions()
        {
            this.width = Game1.tileSize * 14;
            this.height = (int)(this.AspectRatio.Y / this.AspectRatio.X * this.width);
            Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)origin.X;
            this.yPositionOnScreen = (int)origin.Y;
        }
    }
}
