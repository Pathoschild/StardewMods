using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Search;

/// <summary>A menu which allows searching and previewing available chests.</summary>
internal sealed class ChestSearchMenu : IClickableMenu
{
    /*********
    ** Fields
    *********/
    /****
    ** Constants
    ****/
    /// <summary>The margin around the menu content.</summary>
    private const int Margin = 16;

    /// <summary>The number of chest rows to show at once.</summary>
    private const int RowCount = 7;

    /// <summary>The number of chest columns to show at once.</summary>
    private const int ColCount = 4;

    /// <summary>The pixel width of each chest result.</summary>
    private const int CellWidth = 312;

    /// <summary>The pixel height of each chest result.</summary>
    private const int CellHeight = 80;

    /// <summary>The pixel height of the search box row.</summary>
    private const int SearchBarHeight = 96;

    /// <summary>The number of item columns to show in the item preview tooltip.</summary>
    private const int HoverItemsCol = 12;

    /// <summary>The maximum number of items to show in the item preview tooltip.</summary>
    private const int HoverItemsMax = 36;

    /// <summary>The pixel area in <see cref="Game1.menuTexture"/> to draw as the menu background.</summary>
    private readonly Rectangle BackgroundSourceRect = new(0, 256, 60, 60);


    /****
    ** State
    ****/
    /// <summary>The chests available to search.</summary>
    private readonly ManagedChest[] Chests;

    /// <summary>The configured key bindings.</summary>
    private readonly KeybindList PreviewKey;

    /// <summary>The chest search result components.</summary>
    private readonly List<ChestSearchMenuCell> ChestCells = [];

    /// <summary>The chests which match the current search.</summary>
    private readonly List<ManagedChest> MatchedChests = [];

    /// <summary>The search box which matches chest names.</summary>
    private readonly ChestSearchBox NameSearchBox;

    /// <summary>The search box which matches items in the chests.</summary>
    private readonly ChestSearchBox ItemSearchBox;

    /// <summary>The search text boxes.</summary>
    private readonly ChestSearchBox[] SearchBoxes;

    /// <summary>The items which match the current search, indexed by chest.</summary>
    private readonly Dictionary<ManagedChest, IList<Item?>> MatchedItemsInChest = [];

    /// <summary>The items to show in the chest preview tooltip, if any.</summary>
    private IList<Item?>? HoverItems;

    /// <summary>Whether any search text is applied.</summary>
    private bool HasAppliedSearch;

    /// <summary>The index of the first chest result to show in the list to match the scroll offset.</summary>
    private int ScrollIndex;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="currentChest">The chest to open when the search menu is closed, if a different chest isn't selected manually.</param>
    /// <param name="chests"><inheritdoc cref="Chests" path="/summary"/></param>
    /// <param name="keys">The configured key bindings.</param>
    public ChestSearchMenu(ManagedChest currentChest, ManagedChest[] chests, ModConfigKeys keys)
        : base(
            x: Game1.viewport.X + 96,
            y: Game1.viewport.Y + 96,
            width: CellWidth * ColCount + Margin * 2,
            height: SearchBarHeight + CellHeight * RowCount + Margin * 2,
            showUpperRightCloseButton: true
        )
    {
        this.Chests = chests;
        this.PreviewKey = keys.SearchMenuPreviewChest;
        this.exitFunction = () => currentChest.OpenMenu();

        // prebake target chest icons
        foreach (ManagedChest chest in this.Chests)
            chest.Container.TryGetIcon(out _, out _, out _);

        // get position
        Vector2 position = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
        this.xPositionOnScreen = (int)position.X;
        this.yPositionOnScreen = (int)position.Y;

        // create search result cells
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                int myID = 100 + (row * ColCount) + col;
                ChestSearchMenuCell chestSearchMenuCell = new(new(0, 0, CellWidth, CellHeight), $"ChestSearchMenuCell_{row}_{col}", Margin + CellWidth * col, Margin + SearchBarHeight + CellHeight * row)
                {
                    myID = myID,
                    upNeighborID = row > 0 ? myID - ColCount : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                    upNeighborImmutable = true,
                    leftNeighborID = col > 0 ? myID - 1 : ClickableComponent.ID_ignore,
                    rightNeighborID = col < ColCount - 1 ? myID + 1 : ClickableComponent.ID_ignore,
                    downNeighborID =
                        row < RowCount - 1 ? myID + ColCount : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                    downNeighborImmutable = true,
                };
                chestSearchMenuCell.Reposition(this.xPositionOnScreen, this.yPositionOnScreen);
                this.ChestCells.Add(chestSearchMenuCell);
            }
        }

        // create search boxes
        int searchBoxWidth = this.width / 2 - Margin * 2;
        int searchBoxOffset = (int)Math.Max(Game1.smallFont.MeasureString(I18n.SearchBox_ChestName()).Y, Game1.smallFont.MeasureString(I18n.SearchBox_ItemName()).Y);

        this.NameSearchBox = new ChestSearchBox(
            I18n.SearchBox_ChestName(),
            new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|')
            {
                X = this.xPositionOnScreen + Margin,
                Y = this.yPositionOnScreen + Margin + searchBoxOffset,
                Width = searchBoxWidth,
            },
            (chest, term) => chest.DisplayName.ContainsIgnoreCase(term) || chest.DisplayCategory.ContainsIgnoreCase(term)
        );
        this.NameSearchBox.Clickable.myID = 1001;
        this.NameSearchBox.Clickable.downNeighborID = 101;
        this.NameSearchBox.Clickable.rightNeighborID = 1002;

        this.ItemSearchBox = new ChestSearchBox(
            I18n.SearchBox_ItemName(),
            new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|')
            {
                X = this.xPositionOnScreen + Margin + Margin / 2 + searchBoxWidth,
                Y = this.yPositionOnScreen + Margin + searchBoxOffset,
                Width = searchBoxWidth,
            },
            this.SearchItemsInChest
        );
        this.ItemSearchBox.Clickable.myID = 1002;
        this.ItemSearchBox.Clickable.downNeighborID = 101 + ColCount / 2;
        this.ItemSearchBox.Clickable.leftNeighborID = 1001;
        this.ItemSearchBox.Clickable.rightNeighborID = upperRightCloseButton_ID;

        this.SearchBoxes = [
            this.NameSearchBox,
            this.ItemSearchBox
        ];

        // add close button
        this.initializeUpperRightCloseButton();
        this.upperRightCloseButton.myID = upperRightCloseButton_ID;
        this.upperRightCloseButton.leftNeighborID = 1001;
        this.upperRightCloseButton.downNeighborID = 1001;

        // enable controller navigation
        if (Game1.options.snappyMenus && Game1.options.gamepadControls)
        {
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }
    }

    /// <inheritdoc />
    public override void populateClickableComponentList()
    {
        this.allClickableComponents = [this.upperRightCloseButton, this.NameSearchBox.Clickable, this.ItemSearchBox.Clickable];
        this.allClickableComponents.AddRange(this.ChestCells);
    }

    /// <inheritdoc />
    public override void snapToDefaultClickableComponent()
    {
        this.currentlySnappedComponent = this.getComponentWithID(1001);
        base.snapToDefaultClickableComponent();
    }

    /// <inheritdoc />
    protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
    {
        if (oldID is >= 100 and < 100 + ColCount)
        {
            if (!this.ScrollGrid(1))
            {
                this.currentlySnappedComponent = this.getComponentWithID(oldID < 101 + ColCount / 2 ? 1001 : 1002);
                this.snapCursorToCurrentSnappedComponent();
            }
        }
        else if (oldID >= 100 + ColCount * (RowCount - 1))
            this.ScrollGrid(-1);
        else
            base.customSnapBehavior(direction, oldRegion, oldID);
    }

    /// <inheritdoc />
    public override void update(GameTime time)
    {
        this.RecheckSearchState();
        base.update(time);
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        // draw background
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, this.BackgroundSourceRect, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);

        // draw search boxes
        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            Utility.drawTextWithShadow(
                b: b,
                text: searchBox.Label,
                font: Game1.smallFont,
                position: new Vector2(searchBox.TextBox.X, this.yPositionOnScreen + Margin),
                color: Game1.textColor
            );
            searchBox.TextBox.Draw(b);
        }

        // draw chest cells
        foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
            cell.Draw(b, chest);

        // draw item preview tooltip
        if (this.HoverItems != null)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            int count = Math.Min(HoverItemsMax, this.HoverItems.Count(item => item is not null));
            if (count == 0)
            {
                IClickableMenu.drawTextureBox(
                    b: b,
                    texture: Game1.menuTexture,
                    sourceRect: this.BackgroundSourceRect,
                    x: mouseX,
                    y: mouseY - 64 - Margin * 2,
                    width: 64 + Margin * 2,
                    height: 64 + Margin * 2,
                    color: Color.White
                );
            }
            else
            {
                int boxHeight = (int)MathF.Ceiling(count / (float)HoverItemsCol) * 64 + Margin * 2;

                IClickableMenu.drawTextureBox(
                    b: b,
                    texture: Game1.menuTexture,
                    sourceRect: this.BackgroundSourceRect,
                    x: mouseX,
                    y: mouseY - boxHeight,
                    width: Math.Min(count, HoverItemsCol) * 64 + Margin * 2,
                    height: boxHeight,
                    color: Color.White
                );

                Vector2 itemPos = new(mouseX + Margin, mouseY - boxHeight + Margin);
                for (int i = 0; i < count; i++)
                {
                    if (this.HoverItems[i] is { } item)
                    {
                        item.drawInMenu(b, itemPos, 1f);
                        if (i % HoverItemsCol == HoverItemsCol - 1)
                        {
                            itemPos.X = mouseX + Margin;
                            itemPos.Y += 64;
                        }
                        else
                            itemPos.X += 64;
                    }
                }
            }
        }

        // draw close button
        if (this.shouldDrawCloseButton())
            this.upperRightCloseButton?.draw(b);

        // draw mouse
        this.drawMouse(b, ignore_transparency: true);
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        // select search box
        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            if (searchBox.Clickable.bounds.Contains(x, y))
            {
                if (searchBox.TextBox.Selected)
                    searchBox.TextBox.Selected = false;
                else
                {
                    foreach (ChestSearchBox searchBox2 in this.SearchBoxes)
                        searchBox2.TextBox.Selected = false;

                    searchBox.TextBox.Select();
                    return;
                }
            }
        }

        // open chest
        foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
        {
            if (cell.bounds.Contains(x, y))
            {
                chest.OpenMenu();
                return;
            }
        }

        // default behavior
        base.receiveLeftClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        // ignore input if a search box is selected
        if (this.SearchBoxes.Any(searchBox => searchBox.TextBox.Selected))
            return;

        // default behavior
        base.receiveKeyPress(key);
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        // set items for item preview tooltip
        if (this.ItemSearchBox.HasValue)
        {
            foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
            {
                if (cell.bounds.Contains(x, y))
                {
                    if (this.MatchedItemsInChest.TryGetValue(chest, out IList<Item?>? matchedItems))
                        this.HoverItems = matchedItems;
                    return;
                }
            }
        }
        else if (this.PreviewKey.IsDown())
        {
            foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
            {
                if (cell.bounds.Contains(x, y))
                {
                    this.HoverItems = chest.Container.Inventory;
                    return;
                }
            }
        }
        this.HoverItems = null;

        // default behavior
        base.performHoverAction(x, y);
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        this.ScrollGrid(direction);
        base.receiveScrollWheelAction(direction);
    }

    /// <summary>Scroll the grid in the given direction.</summary>
    /// <param name="direction">The direction to scroll, where &gt;0 is up and &lt;0 is down.</param>
    private bool ScrollGrid(int direction)
    {
        bool scrolled = false;
        if (direction > 0 && this.ScrollIndex >= ColCount)
        {
            this.ScrollIndex -= ColCount;
            scrolled = true;
        }
        else if (direction < 0 && this.ScrollIndex < Math.Max(0, (this.HasAppliedSearch ? this.MatchedChests.Count : this.Chests.Length) - this.ChestCells.Count))
        {
            this.ScrollIndex += ColCount;
            scrolled = true;
        }

        if (scrolled)
            Game1.playSound("shiny4");

        return scrolled;
    }

    /// <summary>Get whether a chest contains items matching the search text, and update <see cref="MatchedItemsInChest"/> with the results.</summary>
    /// <param name="chest">The chest to search.</param>
    /// <param name="search">The search text to apply.</param>
    private bool SearchItemsInChest(ManagedChest chest, string search)
    {
        List<Item?> chestMatchedItems = [];
        foreach (Item? item in chest.Container.Inventory)
        {
            if (item?.DisplayName.ContainsIgnoreCase(search) ?? false)
                chestMatchedItems.Add(item);
        }

        this.MatchedItemsInChest[chest] = chestMatchedItems;

        return chestMatchedItems.Count > 0;
    }

    /// <summary>Update the search state based on whether any search boxes have search terms.</summary>
    private void RecheckSearchState()
    {
        // update search text
        bool searchChanged = false;
        List<ChestSearchBox> applySearchBoxes = [];
        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            searchChanged = searchBox.CheckChange() || searchChanged;
            if (searchBox.HasValue)
                applySearchBoxes.Add(searchBox);
        }

        // update matched chests
        if (searchChanged)
        {
            this.MatchedChests.Clear();

            if (applySearchBoxes.Contains(this.ItemSearchBox))
                this.MatchedItemsInChest.Clear();

            if (applySearchBoxes.Any())
            {
                foreach (ManagedChest chest in this.Chests)
                {
                    if (applySearchBoxes.All(searchBox => searchBox.Matches(chest)))
                        this.MatchedChests.Add(chest);
                }
            }
        }

        // update state
        bool newSearchState = applySearchBoxes.Any();
        if (this.HasAppliedSearch != newSearchState)
            this.ScrollIndex = 0;
        this.HasAppliedSearch = newSearchState;
    }

    /// <summary>Iterate the visible search results.</summary>
    private IEnumerable<(ChestSearchMenuCell, ManagedChest)> IterateVisibleChestCells()
    {
        IList<ManagedChest> chests;
        int length;
        if (this.HasAppliedSearch)
        {
            length = Math.Min(this.ChestCells.Count, this.MatchedChests.Count - this.ScrollIndex);
            chests = this.MatchedChests;
        }
        else
        {
            length = Math.Min(this.ChestCells.Count, this.Chests.Length - this.ScrollIndex);
            chests = this.Chests;
        }

        for (int i = 0; i < length; i++)
            yield return (this.ChestCells[i], chests[this.ScrollIndex + i]);
    }
}
