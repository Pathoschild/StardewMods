using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Components
{
    /// <summary>A UI which lets the player search for subjects.</summary>
    internal class SearchMenu : BaseMenu, IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>Show a lookup menu.</summary>
        private readonly Action<ISubject> ShowLookup;

        /// <summary>Encapsulates logging and monitoring.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);

        /// <summary>The maximum pixels to scroll.</summary>
        private int MaxScroll;

        /// <summary>The number of pixels to scroll.</summary>
        private int CurrentScroll;

        /// <summary>The subjects available for searching indexed by name.</summary>
        private readonly ILookup<string, ISubject> SearchLookup;

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
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        public SearchMenu(SubjectFactory codex, Action<ISubject> showLookup, IMonitor monitor)
        {
            // save data
            this.ShowLookup = showLookup;
            this.Monitor = monitor;
            this.SearchLookup = codex.GetSearchSubjects().ToLookup(p => p.Name, StringComparer.OrdinalIgnoreCase);

            // initialise
            this.UpdateLayout();
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
            // search box
            if (this.SearchTextbox.Bounds.Contains(x, y))
            {
                this.SearchTextbox.Select();
                return;
            }

            // search matches
            foreach (SearchResultComponent match in this.SearchResults)
            {
                if (match.containsPoint(x, y))
                {
                    this.ShowLookup(match.Subject);
                    Game1.playSound("coin");
                    return;
                }
            }
        }

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
            this.UpdateLayout();
        }

        /****
        ** Methods
        ****/
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
                GraphicsDevice device = Game1.graphics.GraphicsDevice;
                Rectangle prevScissorRectangle = device.ScissorRectangle;
                try
                {
                    // begin draw
                    device.ScissorRectangle = new Rectangle(x + gutter, y + gutter, (int)contentWidth, (int)contentHeight);
                    contentBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, new RasterizerState { ScissorTestEnable = true });

                    // scroll view
                    this.CurrentScroll = Math.Max(0, this.CurrentScroll); // don't scroll past top
                    this.CurrentScroll = Math.Min(this.MaxScroll, this.CurrentScroll); // don't scroll past bottom
                    topOffset -= this.CurrentScroll; // scrolled down == move text up

                    // draw fields
                    float wrapWidth = this.width - leftOffset - gutter;
                    {
                        Vector2 nameSize = contentBatch.DrawTextBlock(font, "Search", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                        Vector2 typeSize = contentBatch.DrawTextBlock(font, "(Lookup Anything)", new Vector2(x + leftOffset + nameSize.X + spaceWidth, y + topOffset), wrapWidth);
                        topOffset += Math.Max(nameSize.Y, typeSize.Y);

                        this.SearchTextbox.Bounds = new Rectangle(x: x + (int)leftOffset, y: y + (int)topOffset, width: (int)wrapWidth, height: this.SearchTextbox.Bounds.Height);
                        this.SearchTextbox.Draw(contentBatch);
                        topOffset += this.SearchTextbox.Bounds.Height;

                        int mouseX = Game1.getMouseX();
                        int mouseY = Game1.getMouseY();
                        foreach (SearchResultComponent result in this.SearchResults)
                        {
                            bool isHighlighted = result.containsPoint(mouseX, mouseY);
                            var objSize = result.Draw(contentBatch, new Vector2(x + leftOffset, y + topOffset), (int)wrapWidth, isHighlighted);
                            topOffset += objSize.Y;
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

                    // end draw
                    contentBatch.End();
                }
                catch (ArgumentException ex) when (!BaseMenu.UseSafeDimensions && ex.ParamName == "value" && ex.StackTrace.Contains("Microsoft.Xna.Framework.Graphics.GraphicsDevice.set_ScissorRectangle"))
                {
                    this.Monitor.Log("The viewport size seems to be inaccurate. Enabling compatibility mode; lookup menu may be misaligned.", LogLevel.Warn);
                    this.Monitor.Log(ex.ToString());
                    BaseMenu.UseSafeDimensions = true;
                    this.UpdateLayout();
                }
                finally
                {
                    device.ScissorRectangle = prevScissorRectangle;
                }
            }

            // draw mouse cursor
            this.drawMouse(Game1.spriteBatch);
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
            // get search words
            string[] words = (search ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!words.Any())
            {
                this.SearchResults = Enumerable.Empty<SearchResultComponent>();
                return;
            }

            // get matches
            this.SearchResults =
                (
                    from entry in this.SearchLookup
                    where words.All(word => entry.Key.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    orderby entry.Key
                    select new SearchResultComponent(entry.First()) // first result for each name
                )
                .Take(50)
                .ToArray();
        }

        /// <summary>Update the layout dimensions based on the current game scale.</summary>
        private void UpdateLayout()
        {
            Point viewport = this.GetViewportSize();

            // update size
            this.width = Math.Min(Game1.tileSize * 14, viewport.X);
            this.height = Math.Min((int)(this.AspectRatio.Y / this.AspectRatio.X * this.width), viewport.Y);

            // update position
            Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)origin.X;
            this.yPositionOnScreen = (int)origin.Y;
        }
    }
}
