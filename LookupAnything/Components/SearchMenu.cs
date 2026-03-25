using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.StardewAccess;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Themes;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components;

/// <summary>A UI which lets the player search for subjects.</summary>
internal class SearchMenu : BaseMenu, IScrollableMenu, IDisposable
{
    /*********
    ** Properties
    *********/
    /// <summary>The controller navigation ID for the search box.</summary>
    public const int SearchBoxId = 1;

    /// <summary>The controller navigation ID for the first search result.</summary>
    public const int FirstSearchResultId = 2;

    /// <summary>The spacing around the search result area.</summary>
    private const int SearchResultGutter = 15;

    /// <summary>The spacing around the scroll buttons.</summary>
    private const int ScrollButtonGutter = 15;

    /// <summary>Show a lookup menu.</summary>
    private readonly Action<ISubject> ShowLookup;

    /// <summary>Encapsulates logging and monitoring.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The theme to apply for the menu appearance.</summary>
    private readonly ThemeManager Theme;

    /// <summary>The Stardew Access mod integration.</summary>
    private readonly StardewAccessIntegration StardewAccess;

    /// <summary>The clickable 'scroll up' icon.</summary>
    private readonly ClickableTextureComponent ScrollUpButton;

    /// <summary>The clickable 'scroll down' icon.</summary>
    private readonly ClickableTextureComponent ScrollDownButton;

    /// <summary>The amount to scroll long content on each up/down scroll.</summary>
    private readonly int ScrollAmount;

    /// <summary>Whether the next update tick is the first one.</summary>
    private bool IsFirstTick = true;

    /// <summary>The maximum pixels to scroll.</summary>
    private int MaxScroll;

    /// <summary>The number of pixels to scroll.</summary>
    private int CurrentScroll;

    /// <summary>The last search text for which results were shown.</summary>
    private string SearchText = string.Empty;

    /// <summary>The subjects available for searching indexed by name.</summary>
    private readonly ILookup<string, ISubject> SearchLookup;

    /// <summary>The search input box.</summary>
    private readonly TextBox SearchTextbox;

    /// <summary>The clickable area representing the search textbox.</summary>
    private readonly ClickableComponent SearchTextboxClickableArea;

    /// <summary>The current search results.</summary>
    private SearchResultComponent[] SearchResults = [];

    /// <summary>The pixel area containing search results.</summary>
    private Rectangle SearchResultArea;

    /// <summary>Whether to snap to the selected component after the next draw tick.</summary>
    private bool SnapToSelectedComponent;

    /// <summary>Whether the on-screen keyboard was open on the last tick.</summary>
    private bool WasKeyboardOpen;


    /*********
    ** Public methods
    *********/
    /****
    ** Initialization
    ****/
    /// <summary>Construct an instance.</summary>
    /// <param name="searchSubjects">The subjects available to search.</param>
    /// <param name="showLookup">Show a lookup menu.</param>
    /// <param name="monitor">Encapsulates logging and monitoring.</param>
    /// <param name="theme">The theme to apply for the menu appearance.</param>
    /// <param name="scroll">The amount to scroll long content on each up/down scroll.</param>
    /// <param name="stardewAccess">The Stardew Access mod integration.</param>
    public SearchMenu(IEnumerable<ISubject> searchSubjects, Action<ISubject> showLookup, IMonitor monitor, ThemeManager theme, int scroll, StardewAccessIntegration stardewAccess)
    {
        // save data
        this.ShowLookup = showLookup;
        this.Monitor = monitor;
        this.Theme = theme;
        this.SearchLookup = searchSubjects.Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToLookup(p => p.Name, StringComparer.OrdinalIgnoreCase);
        this.ScrollAmount = scroll;
        this.StardewAccess = stardewAccess;

        // create components
        this.SearchTextbox = new TextBox(Sprites.Textbox.Sheet, null, Game1.smallFont, Color.Black);
        this.SearchTextboxClickableArea = new ClickableComponent(Rectangle.Empty, "SearchText")
        {
            upNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
            downNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
            myID = SearchMenu.SearchBoxId,
            ScreenReaderText = I18n.SearchMenu_ScreenReader_SearchEmpty()
        };
        this.ScrollUpButton = new ClickableTextureComponent(Rectangle.Empty, CommonSprites.Icons.Sheet, CommonSprites.Icons.UpArrow, 1);
        this.ScrollDownButton = new ClickableTextureComponent(Rectangle.Empty, CommonSprites.Icons.Sheet, CommonSprites.Icons.DownArrow, 1);

        // initialise
        this.UpdateLayout();
        this.SnapToSelectedComponent = true;
        this.StardewAccess.Say(I18n.SearchMenu_ScreenReader_Instructions(), true);
    }


    /****
    ** Events
    ****/
    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        // close button
        if (this.upperRightCloseButton.containsPoint(x, y))
            this.exitThisMenu();

        // search box
        else if (x >= this.SearchTextbox.X && x <= this.SearchTextbox.X + this.SearchTextbox.Width && y >= this.SearchTextbox.Y && y <= this.SearchTextbox.Y + this.SearchTextbox.Height)
            this.SelectSearchBox();

        // scroll up or down
        else if (this.ScrollUpButton.containsPoint(x, y))
            this.ScrollUp();
        else if (this.ScrollDownButton.containsPoint(x, y))
            this.ScrollDown();

        // search matches
        else if (this.SearchResultArea.Contains(x, y))
        {
            foreach (SearchResultComponent match in this.GetResultsPossiblyOnScreen())
            {
                if (match.containsPoint(x, y))
                {
                    this.ShowLookup(match.Subject);
                    Game1.playSound("coin");
                    return;
                }
            }
        }
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        // handle exit
        if (key == Keys.Escape)
        {
            if (this.SearchTextbox.Selected)
                this.SearchTextbox.Selected = false; // deselect search box first, to allow for key navigation
            else
                this.exitThisMenu();
            return;
        }

        // handle controller navigation
        // (Controller snap navigation is sent as key presses; see 'receiveKeyPress' in Game1.updateActiveMenu.)
        if (Game1.options.snappyMenus && Game1.options.gamepadControls && Game1.textEntry is null && !this.SearchTextbox.Selected)
        {
            bool isMovementKey =
                Game1.options.doesInputListContain(Game1.options.moveUpButton, key)
                || Game1.options.doesInputListContain(Game1.options.moveRightButton, key)
                || Game1.options.doesInputListContain(Game1.options.moveDownButton, key)
                || Game1.options.doesInputListContain(Game1.options.moveLeftButton, key);

            if (isMovementKey)
                base.receiveKeyPress(key);
        }

        // else deliberately avoid calling base, which may let another key close the menu
    }

    /// <inheritdoc />
    public override void receiveGamePadButton(Buttons button)
    {
        switch (button)
        {
            // exit
            case Buttons.B:
                this.exitThisMenu();
                break;

            // scroll up
            case Buttons.RightThumbstickUp:
                this.ScrollUp();
                break;

            // scroll down
            case Buttons.RightThumbstickDown:
                this.ScrollDown();
                break;

            default:
                base.receiveGamePadButton(button);
                break;
        }
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

        // If the player scrolls in gamepad mode, set result under the cursor as the active component so navigating
        // doesn't snap them back to the old scroll position.
        if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse && (this.currentlySnappedComponent is null || !this.currentlySnappedComponent.containsPoint(x, y)))
        {
            if (this.SearchTextboxClickableArea.containsPoint(x, y))
                this.setCurrentlySnappedComponentTo(SearchMenu.SearchBoxId);
            else
            {
                foreach (SearchResultComponent result in this.SearchResults)
                {
                    if (result.containsPoint(x, y))
                    {
                        this.setCurrentlySnappedComponentTo(result.myID);
                        break;
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        if (direction > 0)    // positive number scrolls content up
            this.ScrollUp();
        else
            this.ScrollDown();
    }

    /// <inheritdoc />
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.UpdateLayout();
    }

    /****
    ** Methods
    ****/
    /// <inheritdoc />
    public override void update(GameTime time)
    {
        base.update(time);

        // select text box
        if (this.IsFirstTick)
        {
            this.SelectSearchBox();
            this.IsFirstTick = false;
        }

        // handle search
        if (this.SearchText != this.SearchTextbox.Text)
        {
            this.SearchText = this.SearchTextbox.Text;
            this.ReceiveSearchTextboxChanged(this.SearchText);
        }

        // handle on-screen keyboard
        bool keyboardOpen = Game1.textEntry is not null;
        if (keyboardOpen != this.WasKeyboardOpen)
        {
            if (!keyboardOpen)
            {
                // cursor was moved by keyboard menu, snap back to the search box so player can navigate down to results
                if (this.currentlySnappedComponent?.myID != SearchBoxId)
                {
                    this.currentlySnappedComponent = this.SearchTextboxClickableArea;
                    this.SnapToSelectedComponent = true;
                }

                // narrate instructions
                this.StardewAccess.SayMenuElement(this.SearchTextboxClickableArea, interrupt: false);
            }

            this.WasKeyboardOpen = keyboardOpen;
        }
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        // calculate dimensions
        int x = this.xPositionOnScreen;
        int y = this.yPositionOnScreen;
        const int gutter = SearchMenu.SearchResultGutter;
        const float leftOffset = gutter;
        float topOffset = gutter;
        float contentHeight = this.SearchResultArea.Height;

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
            backgroundBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            this.Theme.Background.Draw(backgroundBatch, x, y, this.width, this.height);
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
                device.ScissorRectangle = this.SearchResultArea;
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

                    this.SearchTextbox.X = x + (int)leftOffset;
                    this.SearchTextbox.Y = y + (int)topOffset;
                    this.SearchTextbox.Width = (int)wrapWidth;
                    this.SearchTextboxClickableArea.bounds = new Rectangle(this.SearchTextbox.X, this.SearchTextbox.Y, this.SearchTextbox.Width, this.SearchTextbox.Height);

                    this.SearchTextbox.Draw(contentBatch);
                    topOffset += this.SearchTextbox.Height;

                    int mouseX = Game1.getMouseX();
                    int mouseY = Game1.getMouseY();
                    bool reachedViewport = false;
                    bool reachedBottomOfViewport = false;
                    bool isCursorInSearchArea =
                        this.SearchResultArea.Contains(mouseX, mouseY)
                        && !this.ScrollUpButton.containsPoint(mouseX, mouseY)
                        && !this.ScrollDownButton.containsPoint(mouseX, mouseY);
                    foreach (SearchResultComponent result in this.SearchResults)
                    {
                        if (!reachedViewport || !reachedBottomOfViewport)
                        {
                            if (this.IsResultPossiblyOnScreen(result))
                            {
                                reachedViewport = true;
                                bool isHighlighted = isCursorInSearchArea && result.containsPoint(mouseX, mouseY);
                                result.Draw(contentBatch, new Vector2(x + leftOffset, y + topOffset), (int)wrapWidth, isHighlighted);
                            }
                            else if (reachedViewport)
                                reachedBottomOfViewport = true;
                        }

                        topOffset += SearchResultComponent.FixedHeight;
                    }

                    // draw spacer
                    topOffset += lineHeight;
                }

                // update max scroll
                this.MaxScroll = Math.Max(0, (int)(topOffset - contentHeight + this.CurrentScroll));

                // draw scroll icons
                if (this.MaxScroll > 0 && this.CurrentScroll > 0)
                    this.ScrollUpButton.draw(b);
                if (this.MaxScroll > 0 && this.CurrentScroll < this.MaxScroll)
                    this.ScrollDownButton.draw(b);

                // end draw
                contentBatch.End();

                // move cursor to selected component if needed
                if (this.SnapToSelectedComponent)
                {
                    this.SnapToSelectedComponent = false;
                    this.snapCursorToCurrentSnappedComponent();
                }
            }
            catch (ArgumentException ex) when (!BaseMenu.UseSafeDimensions && ex.ParamName == "value" && ex.StackTrace?.Contains("Microsoft.Xna.Framework.Graphics.GraphicsDevice.set_ScissorRectangle") == true)
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

        // draw close button
        this.upperRightCloseButton.draw(b);

        // draw mouse cursor
        this.drawMouse(Game1.spriteBatch);
    }

    /// <inheritdoc />
    public override void populateClickableComponentList()
    {
        base.populateClickableComponentList();

        this.allClickableComponents.Add(this.SearchTextboxClickableArea);
        this.allClickableComponents.AddRange(this.SearchResults);
    }

    /// <inheritdoc />
    public void ScrollUp(int? amount = null)
    {
        this.Scroll(-(amount ?? this.ScrollAmount));
    }

    /// <inheritdoc />
    public void ScrollDown(int? amount = null)
    {
        this.Scroll(amount ?? this.ScrollAmount);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.SearchTextbox.Selected = false;
    }


    /*********
    ** Protected methods
    *********/
    /// <inheritdoc />
    protected override void customSnapBehavior(int direction, int oldRegion, int oldId)
    {
        // snap to next component
        ClickableComponent? prevSnapped = this.currentlySnappedComponent;
        switch (oldId)
        {
            // from top-right close button
            case IClickableMenu.upperRightCloseButton_ID:
                switch (direction)
                {
                    case Game1.down:
                        this.setCurrentlySnappedComponentTo(SearchMenu.SearchBoxId);
                        break;
                }
                break;

            // from search box
            case SearchMenu.SearchBoxId:
                switch (direction)
                {
                    case Game1.up:
                        this.setCurrentlySnappedComponentTo(IClickableMenu.upperRightCloseButton_ID);
                        break;

                    case Game1.down when this.SearchResults.Length > 0:
                        this.setCurrentlySnappedComponentTo(SearchMenu.FirstSearchResultId);
                        break;
                }
                break;

            // from search result
            case >= SearchMenu.FirstSearchResultId:
                switch (direction)
                {
                    case Game1.up:
                        if (oldId == SearchMenu.FirstSearchResultId)
                            this.setCurrentlySnappedComponentTo(SearchMenu.SearchBoxId);
                        else
                            this.setCurrentlySnappedComponentTo(oldId - 1);
                        break;

                    case Game1.down:
                        this.setCurrentlySnappedComponentTo(oldId + 1);
                        this.currentlySnappedComponent ??= this.SearchResults.Last();
                        break;
                }
                break;
        }

        // scroll into view if needed
        if (this.currentlySnappedComponent != null && !object.ReferenceEquals(prevSnapped, this.currentlySnappedComponent))
        {
            if (this.ScrollIntoView(this.currentlySnappedComponent))
                this.SnapToSelectedComponent = true;
        }

        // toggle textbox selection
        if (this.SearchTextbox.Selected && this.currentlySnappedComponent?.myID != SearchBoxId)
            this.SearchTextbox.Selected = false;
    }

    /// <summary>Set the cursor in the search box, and show the on-screen keyboard if needed.</summary>
    /// <remarks>Derived from <see cref="TextBox.Update"/>, but doesn't require that the cursor be over the field.</remarks>
    private void SelectSearchBox()
    {
        this.SearchTextbox.Selected = true;

        if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
            Game1.showTextEntry(this.SearchTextbox);
    }

    /// <summary>Scroll the menu content by the given amount.</summary>
    /// <param name="amount">The scroll amount to apply, where negative values scroll up and positive values scroll down.</param>
    /// <returns>Returns whether the content view was scrolled.</returns>
    private bool Scroll(int amount)
    {
        int prevScroll = this.CurrentScroll;

        this.CurrentScroll += amount;
        if (this.CurrentScroll < 0)
            this.CurrentScroll = 0;

        return this.CurrentScroll != prevScroll;
    }

    /// <summary>Scroll until the given component is fully visible within the content area.</summary>
    /// <param name="component">The search result.</param>
    /// <returns>Returns whether the content view was scrolled.</returns>
    private bool ScrollIntoView(ClickableComponent component)
    {
        // special case: search box includes the label above it
        if (component == this.SearchTextboxClickableArea)
        {
            int oldScroll = this.CurrentScroll;
            this.CurrentScroll = 0;
            return oldScroll != 0;
        }

        // else check bounds
        int minVisibleY = this.SearchResultArea.Y;
        int maxVisibleY = this.SearchResultArea.Bottom;
        Rectangle bounds = component.bounds;

        if (bounds.Y < minVisibleY)
            return this.Scroll(-(minVisibleY - bounds.Y));
        if (bounds.Bottom > maxVisibleY)
            return this.Scroll(bounds.Bottom - maxVisibleY);

        return false;
    }

    /// <summary>Get the search results that may be on screen.</summary>
    private IEnumerable<SearchResultComponent> GetResultsPossiblyOnScreen()
    {
        bool reachedViewport = false;
        foreach (var result in this.SearchResults)
        {
            if (!this.IsResultPossiblyOnScreen(result))
            {
                if (reachedViewport)
                    yield break;
                continue;
            }

            reachedViewport = true;
            yield return result;
        }
    }

    /// <summary>Get whether a search result may be on screen.</summary>
    /// <param name="result">The search result.</param>
    private bool IsResultPossiblyOnScreen(SearchResultComponent result)
    {
        // This is a simple approximation to optimize large lists. It doesn't need to be
        // precise, as long as it can't have false positives.
        const int resultHeight = SearchResultComponent.FixedHeight;
        int index = result.Index;
        int minY = (index - 3) * resultHeight;
        int maxY = (index + 3) * resultHeight;
        return
            maxY > this.CurrentScroll
            && minY < this.CurrentScroll + this.height;
    }

    /// <summary>The method invoked when the player changes the search text.</summary>
    /// <param name="search">The new search text.</param>
    private void ReceiveSearchTextboxChanged(string? search)
    {
        // get search words
        string[] words = (search ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!words.Any())
        {
            this.SearchResults = [];
            return;
        }

        // get matches
        this.SearchResults = this.SearchLookup
            .Where(entry => words.All(word => entry.Key.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0))
            .SelectMany(entry => entry)
            .OrderBy(subject => subject.Name, StringComparer.OrdinalIgnoreCase)
            .Select((subject, index) =>
            {
                SearchResultComponent result = new SearchResultComponent(subject, index, componentId: SearchMenu.FirstSearchResultId + index);
                result.ScreenReaderText = result.DisplayText;
                return result;
            })
            .ToArray();

        // update screen reader text
        ClickableComponent searchBox = this.SearchTextboxClickableArea;
        if (string.IsNullOrWhiteSpace(search))
            searchBox.ScreenReaderText = I18n.SearchMenu_ScreenReader_SearchEmpty();
        else if (this.SearchResults.Length == 0)
            searchBox.ScreenReaderText = I18n.SearchMenu_ScreenReader_SearchNoResults(search: search);
        else
            searchBox.ScreenReaderText = I18n.SearchMenu_ScreenReader_SearchResults(search: search, count: this.SearchResults.Length);

        // reset controller snap elements
        this.StardewAccess.SayMenuElement(searchBox);

        this.populateClickableComponentList();
    }

    /// <summary>Update the layout dimensions based on the current game scale.</summary>
    private void UpdateLayout()
    {
        Point viewport = this.GetViewportSize();

        // update size
        this.width = Math.Min(Game1.tileSize * 14, viewport.X);
        this.height = Math.Min((int)(this.Theme.Background.AspectRatio * this.width), viewport.Y);

        // update position
        Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
        int x = this.xPositionOnScreen = (int)origin.X;
        int y = this.yPositionOnScreen = (int)origin.Y;
        const int searchGutter = SearchMenu.SearchResultGutter;
        float contentWidth = this.width - searchGutter * 2;
        float contentHeight = this.height - searchGutter * 2;

        // update scissor rectangle for search result area
        this.SearchResultArea = new Rectangle(x + searchGutter, y + searchGutter, (int)contentWidth, (int)contentHeight);

        // update up/down buttons
        const int scrollGutter = SearchMenu.ScrollButtonGutter;
        this.ScrollUpButton.bounds = new Rectangle(x + scrollGutter, (int)(y + contentHeight - CommonSprites.Icons.UpArrow.Height - scrollGutter - CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.UpArrow.Height, CommonSprites.Icons.UpArrow.Width);
        this.ScrollDownButton.bounds = new Rectangle(x + scrollGutter, (int)(y + contentHeight - CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.DownArrow.Height, CommonSprites.Icons.DownArrow.Width);

        // add close button
        this.initializeUpperRightCloseButton();
        this.upperRightCloseButton.myID = IClickableMenu.upperRightCloseButton_ID;
        this.upperRightCloseButton.downNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR;
    }
}
