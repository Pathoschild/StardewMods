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

    /// <summary>The number of chests to show in each row.</summary>
    private const int ChestsPerRow = 4;

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

    /// <summary>The pool of chest search result components.</summary>
    /// <remarks>Most code should use <see cref="VisibleChestCells"/> instead.</remarks>
    private readonly ChestSearchMenuCell[] ChestCellPool;

    /// <summary>The chests which match the current search.</summary>
    private readonly List<ManagedChest> VisibleChests;

    /// <summary>The chest cells which match the current search.</summary>
    /// <remarks>This is updated via <see cref="UpdateVisibleCells"/>.</remarks>
    private readonly List<ChestSearchMenuCell> VisibleChestCells = [];

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
            width: CellWidth * ChestsPerRow + Margin * 2,
            height: SearchBarHeight + CellHeight * RowCount + Margin * 2,
            showUpperRightCloseButton: true
        )
    {
        // init
        this.Chests = chests;
        this.VisibleChests = chests.ToList();
        this.PreviewKey = keys.SearchMenuPreviewChest;
        this.exitFunction = () => currentChest.OpenMenu();

        // get position
        Vector2 position = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
        this.xPositionOnScreen = (int)position.X;
        this.yPositionOnScreen = (int)position.Y;

        // create search result cells
        this.ChestCellPool = new ChestSearchMenuCell[RowCount * ChestsPerRow];
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ChestsPerRow; col++)
            {
                int i = (row * ChestsPerRow) + col;
                int myId = 100 + i;

                ChestSearchMenuCell chestSearchMenuCell = new(new Rectangle(0, 0, CellWidth, CellHeight), $"ChestSearchMenuCell_{row}_{col}", Margin + CellWidth * col, Margin + SearchBarHeight + CellHeight * row)
                {
                    myID = myId,
                    upNeighborID = row > 0 ? myId - ChestsPerRow : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                    upNeighborImmutable = true,
                    leftNeighborID = col > 0 ? myId - 1 : ClickableComponent.ID_ignore,
                    rightNeighborID = col < ChestsPerRow - 1 ? myId + 1 : ClickableComponent.ID_ignore,
                    downNeighborID =
                        row < RowCount - 1 ? myId + ChestsPerRow : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                    downNeighborImmutable = true,
                };
                chestSearchMenuCell.Reposition(this.xPositionOnScreen, this.yPositionOnScreen);
                this.ChestCellPool[i] = chestSearchMenuCell;
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
        this.ItemSearchBox.Clickable.downNeighborID = 101 + ChestsPerRow / 2;
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

        // set initial results
        this.UpdateVisibleCells();
    }

    /// <inheritdoc />
    public override void populateClickableComponentList()
    {
        this.allClickableComponents = [this.upperRightCloseButton, this.NameSearchBox.Clickable, this.ItemSearchBox.Clickable];
        this.allClickableComponents.AddRange(this.ChestCellPool);
    }

    /// <inheritdoc />
    public override void snapToDefaultClickableComponent()
    {
        this.currentlySnappedComponent = this.getComponentWithID(1001);
        base.snapToDefaultClickableComponent();
    }

    /// <inheritdoc />
    protected override void customSnapBehavior(int direction, int oldRegion, int oldId)
    {
        if (oldId is >= 100 and < 100 + ChestsPerRow)
        {
            if (!this.ScrollGrid(1))
            {
                this.currentlySnappedComponent = this.getComponentWithID(oldId < 101 + ChestsPerRow / 2 ? 1001 : 1002);
                this.snapCursorToCurrentSnappedComponent();
            }
        }
        else if (oldId >= 100 + ChestsPerRow * (RowCount - 1))
            this.ScrollGrid(-1);
        else
            base.customSnapBehavior(direction, oldRegion, oldId);
    }

    /// <inheritdoc />
    public override void update(GameTime time)
    {
        this.ApplySearchIfChanged();

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
        foreach (ChestSearchMenuCell cell in this.VisibleChestCells)
            cell.Draw(b);

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
        foreach (ChestSearchMenuCell cell in this.VisibleChestCells)
        {
            if (cell.bounds.Contains(x, y))
            {
                cell.Chest!.OpenMenu();
                return;
            }
        }

        // default behavior
        base.receiveLeftClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        // ignore text input if a search box is selected
        if (key != Keys.Escape && this.SearchBoxes.Any(searchBox => searchBox.TextBox.Selected))
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
            foreach (ChestSearchMenuCell cell in this.VisibleChestCells)
            {
                if (cell.bounds.Contains(x, y))
                {
                    if (this.MatchedItemsInChest.TryGetValue(cell.Chest!, out IList<Item?>? matchedItems))
                        this.HoverItems = matchedItems;
                    return;
                }
            }
        }
        else if (this.PreviewKey.IsDown())
        {
            foreach (ChestSearchMenuCell cell in this.VisibleChestCells)
            {
                if (cell.bounds.Contains(x, y))
                {
                    this.HoverItems = cell.Chest!.Container.Inventory;
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
        direction = Math.Clamp(direction, -1, 1);

        // scroll
        bool scrolled;
        {
            int oldIndex = this.ScrollIndex;
            int newRawIndex = oldIndex + (-direction * ChestsPerRow);

            int totalRows = (int)Math.Ceiling((this.VisibleChests.Count * 1f) / ChestsPerRow);
            int max = Math.Max(0, totalRows * ChestsPerRow - (RowCount * ChestsPerRow));

            this.ScrollIndex = Math.Clamp(newRawIndex, 0, max);

            scrolled = this.ScrollIndex != oldIndex;
        }

        // update on scroll
        if (scrolled)
        {
            Game1.playSound("shiny4");

            this.UpdateVisibleCells();
        }
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

    /// <summary>Update the search state and displayed results if the search text changed.</summary>
    private void ApplySearchIfChanged()
    {
        // get search boxes to apply
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
            this.VisibleChests.Clear();

            if (applySearchBoxes.Contains(this.ItemSearchBox))
                this.MatchedItemsInChest.Clear();

            if (applySearchBoxes.Any())
            {
                foreach (ManagedChest chest in this.Chests)
                {
                    if (applySearchBoxes.All(searchBox => searchBox.Matches(chest)))
                        this.VisibleChests.Add(chest);
                }

                this.UpdateVisibleCells();
            }
        }

        // update state
        bool newSearchState = applySearchBoxes.Any();
        if (this.HasAppliedSearch != newSearchState)
            this.ScrollIndex = 0;
        this.HasAppliedSearch = newSearchState;
    }

    /// <summary>Reset the cached <see cref="VisibleChestCells"/> to match the <see cref="VisibleChests"/> and <see cref="ScrollIndex"/>.</summary>
    private void UpdateVisibleCells()
    {
        this.VisibleChestCells.Clear();

        int slotIndex = 0;

        for (int chestIndex = this.ScrollIndex; chestIndex < this.VisibleChests.Count; chestIndex++)
        {
            if (slotIndex >= this.ChestCellPool.Length)
                break;

            ChestSearchMenuCell cell = this.ChestCellPool[slotIndex];
            cell.visible = true;
            cell.SetChest(this.VisibleChests[chestIndex]);
            this.VisibleChestCells.Add(cell);

            slotIndex++;
        }

        for (int i = slotIndex; i < this.ChestCellPool.Length; i++)
            this.ChestCellPool[i].visible = false;
    }
}
