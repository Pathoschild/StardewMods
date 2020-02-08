using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything;
using Pathoschild.Stardew.LookupAnything.Components;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.LookupAnything.Components
{
    /// <summary>A UI which lets the player search for subjects.</summary>
    internal class SearchMenu : IClickableMenu, IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>Show a lookup menu.</summary>
        private readonly Action<ISubject> ShowLookup;

        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);

        /// <summary>The maximum pixels to scroll.</summary>
        private int MaxScroll;

        /// <summary>The number of pixels to scroll.</summary>
        private int CurrentScroll;

        /// <summary>The subjects available for searching indexed by name.</summary>
        private readonly ILookup<string, SearchResult> SearchLookup;

        /// <summary>The search input box.</summary>
        private readonly SearchTextBox SearchTextbox;

        /// <summary>The current search results.</summary>
        private IEnumerable<SearchResultComponent> SearchResults = Enumerable.Empty<SearchResultComponent>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="showLookup">Show a lookup menu.</param>
        public SearchMenu(SubjectFactory codex, Action<ISubject> showLookup)
        {
            // save data
            this.ShowLookup = showLookup;
            this.SearchLookup = codex.GetSearchIndex();

            // initialise
            this.CalculateDimensions();
            this.SearchTextbox = new SearchTextBox(Game1.smallFont, Color.Black);
            this.SearchTextbox.Select();
            this.SearchTextbox.OnChanged += (sender, text) => this.ReceiveSearchTextboxChanged(text);
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
            SearchResultComponent match = this.SearchResults.FirstOrDefault(p => p.containsPoint(x, y));
            if (match != null)
            {
                // close search menu
                this.Dispose();

                // open lookup menu
                ISubject subject = match.Result.Subject.Value;
                this.ShowLookup(subject);
                Game1.playSound("coin");
            }
        }

        /// <summary>The method invoked when the player right-clicks on the lookup UI.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <param name="key">The pressed input.</param>
        public override void receiveKeyPress(Keys key)
        {
            // deliberately avoid calling base, which may let another key close the menu
            if (key.Equals(Keys.Escape))
                this.exitThisMenu();
        }

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
                    {
                        Vector2 nameSize = contentBatch.DrawTextBlock(font, "Search", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                        Vector2 typeSize = contentBatch.DrawTextBlock(font, "(Lookup Anything)", new Vector2(x + leftOffset + nameSize.X + spaceWidth, y + topOffset), wrapWidth);
                        topOffset += Math.Max(nameSize.Y, typeSize.Y);

                        this.SearchTextbox.X = (int)(x + leftOffset);
                        this.SearchTextbox.Y = (int)(y + topOffset);
                        this.SearchTextbox.Width = (int)wrapWidth;
                        this.SearchTextbox.Draw(contentBatch);
                        topOffset += this.SearchTextbox.Height;

                        int mouseX = Game1.getMouseX();
                        int mouseY = Game1.getMouseY();
                        foreach (SearchResultComponent result in this.SearchResults)
                        {
                            bool isHighlighted = result.containsPoint(mouseX, mouseY);
                            var objSize = result.Draw(contentBatch, new Vector2(x + leftOffset, y + topOffset), (int)wrapWidth, isHighlighted);
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

        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            this.SearchTextbox.Dispose();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player changes the search text.</summary>
        /// <param name="search">The new search text.</param>
        private void ReceiveSearchTextboxChanged(string search)
        {
            // nothing to search
            if (string.IsNullOrWhiteSpace(search))
            {
                this.SearchResults = Enumerable.Empty<SearchResultComponent>();
                return;
            }

            // get search results
            this.SearchResults =
                (
                    from entry in this.SearchLookup
                    let name = entry.Key
                    let score = this.GetSearchScore(name, search, 5)
                    where score < 3 // since the corpus is small, low values usually have little relevance
                    orderby score ascending
                    select new SearchResultComponent(entry.First()) // get first result for each name
                )
                .Take(20) // the above filter should be aggressive, if there are many results, most probably come from the pre-sifted constants (e.g. 'ore' and we want to include them)
                .ToArray();
        }

        /// <summary>Get a value indicating how closely two values match (higher is better).</summary>
        /// <param name="target">The potential search result.</param>
        /// <param name="search">The search string.</param>
        /// <param name="maxSiftOffset">The maximum number of characters to compare when sifting.</param>
        private double GetSearchScore(string target, string search, int maxSiftOffset)
        {
            // normalise
            search = search?.ToLowerInvariant();
            target = target?.ToLowerInvariant();

            // handle empty values
            if (string.IsNullOrEmpty(target))
                return string.IsNullOrEmpty(search) ? 0 : search.Length;
            if (string.IsNullOrEmpty(search))
                return target.Length;

            // exact match
            if (target == search)
                return 0;

            // prefer initial match (e.g. "brea" yields "bread" before "complete breakfast")
            if (target.StartsWith(search))
                return 0.5;
            if (target.Contains(search))
                return 1.5;

            // get fuzzy search score
            // This is an implementation of the Sift3 compare algorithm by Siderite; see https://siderite.blogspot.com/2007/04/super-fast-and-accurate-string-distance.html.
            {
                int indexA = 0, indexB = 0, longestCommonSubstring = 0;
                while (indexA < target.Length && indexB < search.Length)
                {
                    if (target[indexA] == search[indexB])
                        longestCommonSubstring++;
                    else
                    {
                        for (int i = 1; i < maxSiftOffset; i++)
                        {
                            if ((indexA + i < target.Length) && (target[indexA + i] == search[indexB]))
                            {
                                indexA += i;
                                break;
                            }

                            if ((indexB + i < search.Length) && (target[indexA] == search[indexB + i]))
                            {
                                indexB += i;
                                break;
                            }
                        }
                    }
                    indexA++;
                    indexB++;
                }
                return (target.Length + search.Length) / 2 - longestCommonSubstring;
            }
        }

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
