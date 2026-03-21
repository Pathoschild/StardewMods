using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using Pathoschild.Stardew.Common.Integrations.StardewAccess;
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

    /// <summary>The Stardew Access integration.</summary>
    private readonly StardewAccessIntegration StardewAccess;

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

    /// <summary>Whether the menu is on its first update tick.</summary>
    private bool IsFirstTick = true;

    /// <summary>The last focused element query narrated by the search menu.</summary>
    private string? LastFocusedQuery;

    /// <summary>The last focused header control above the search results.</summary>
    private ClickableComponent? LastHeaderComponent;

    /// <summary>The chest result queued to open after the current click/input cycle finishes.</summary>
    private ManagedChest? PendingChestToOpen;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="currentChest">The chest to open when the search menu is closed, if a different chest isn't selected manually.</param>
    /// <param name="chests"><inheritdoc cref="Chests" path="/summary"/></param>
    /// <param name="keys">The configured key bindings.</param>
    /// <param name="stardewAccess">The Stardew Access integration.</param>
    public ChestSearchMenu(ManagedChest currentChest, ManagedChest[] chests, ModConfigKeys keys, StardewAccessIntegration stardewAccess)
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
        this.StardewAccess = stardewAccess;
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
        this.upperRightCloseButton.ScreenReaderText = "Close";

        this.populateClickableComponentList();
        this.SetFocusedComponent(this.NameSearchBox.Clickable);
        this.LastHeaderComponent = this.NameSearchBox.Clickable;

        // enable controller navigation
        if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            this.snapToDefaultClickableComponent();

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
        this.SetFocusedComponent(this.NameSearchBox.Clickable);
        base.snapToDefaultClickableComponent();
    }

    /// <inheritdoc />
    protected override void customSnapBehavior(int direction, int oldRegion, int oldId)
    {
        if (oldId is >= 100 and < 100 + ChestsPerRow)
        {
            if (!this.ScrollGrid(1))
            {
                this.SetFocusedComponent(this.getComponentWithID(oldId < 101 + ChestsPerRow / 2 ? 1001 : 1002), snapCursor: true);
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
        if (this.PendingChestToOpen is { } pendingChest)
        {
            this.PendingChestToOpen = null;
            this.MoveCursorOffMenu();
            pendingChest.OpenMenu();
            return;
        }

        this.ApplySearchIfChanged();

        base.update(time);

        if (this.IsFirstTick)
        {
            this.IsFirstTick = false;
            this.FocusSearchBox(this.NameSearchBox, selectText: true, announce: true);
            return;
        }

        this.EnsureFocusedComponent();
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
        if (this.upperRightCloseButton.containsPoint(x, y))
        {
            this.SetFocusedComponent(this.upperRightCloseButton);
            this.DeselectSearchBoxes();
            this.exitThisMenu();
            return;
        }

        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            if (!searchBox.Clickable.bounds.Contains(x, y))
                continue;

            this.FocusSearchBox(searchBox, selectText: true, announce: true);
            return;
        }

        foreach (ChestSearchMenuCell cell in this.VisibleChestCells)
        {
            if (!cell.bounds.Contains(x, y))
                continue;

            this.SetFocusedComponent(cell);
            this.OpenChestResult(cell.Chest!);
            return;
        }

        base.receiveLeftClick(x, y, playSound);
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape)
        {
            this.DeselectSearchBoxes();
            this.exitThisMenu();
            return;
        }

        ChestSearchBox? selectedSearchBox = this.GetSelectedSearchBox();
        if (selectedSearchBox != null)
        {
            if (key == Keys.Enter)
            {
                this.DeselectSearchBoxes();
                this.SetFocusedComponent(selectedSearchBox.Clickable);
                this.TrySpeakFocusedComponent(force: true, interrupt: false);
            }

            return;
        }

        bool isMovementKey =
            Game1.options.doesInputListContain(Game1.options.moveUpButton, key)
            || Game1.options.doesInputListContain(Game1.options.moveRightButton, key)
            || Game1.options.doesInputListContain(Game1.options.moveDownButton, key)
            || Game1.options.doesInputListContain(Game1.options.moveLeftButton, key);
        if (isMovementKey)
        {
            this.EnsureFocusedComponent();
            if (this.TryHandleDirectionalNavigation(key))
                this.TrySpeakFocusedComponent(interrupt: true);
            return;
        }
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
            bool hasSearchFilters = applySearchBoxes.Any();
            if (this.HasAppliedSearch != hasSearchFilters)
                this.ScrollIndex = 0;

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
            }
            else
                this.VisibleChests.AddRange(this.Chests);

            this.UpdateVisibleCells();
        }

        // update state
        bool newSearchState = applySearchBoxes.Any();
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

    /// <summary>Open the currently snapped chest result, if any.</summary>
    private bool TryOpenSnappedChest()
    {
        if (this.currentlySnappedComponent is not ChestSearchMenuCell cell || !cell.visible || cell.Chest == null)
            return false;

        return this.OpenChestResult(cell.Chest);
    }

    /// <summary>Handle Stardew Access' left-click action against the currently snapped element.</summary>
    internal bool TryHandleStardewAccessLeftClick()
    {
        ClickableComponent? target = this.GetActionTargetAt(Game1.getMouseX(true), Game1.getMouseY(true)) ?? this.currentlySnappedComponent ?? this.GetSelectedSearchBox()?.Clickable;
        if (target == null)
            return false;

        this.SetFocusedComponent(target);

        if (target is ChestSearchMenuCell)
            return this.TryOpenSnappedChest();

        if (ReferenceEquals(target, this.NameSearchBox.Clickable))
        {
            this.FocusSearchBox(this.NameSearchBox, selectText: true, announce: true);
            return true;
        }

        if (ReferenceEquals(target, this.ItemSearchBox.Clickable))
        {
            this.FocusSearchBox(this.ItemSearchBox, selectText: true, announce: true);
            return true;
        }

        if (ReferenceEquals(target, this.upperRightCloseButton))
        {
            this.DeselectSearchBoxes();
            this.exitThisMenu();
            return true;
        }

        return false;
    }

    /// <summary>Focus a search box and optionally enter text editing.</summary>
    private void FocusSearchBox(ChestSearchBox searchBox, bool selectText, bool announce)
    {
        this.DeselectSearchBoxes();
        this.SetFocusedComponent(searchBox.Clickable);

        if (selectText)
            searchBox.TextBox.Select();

        if (announce)
            this.TrySpeakFocusedComponent(force: true);
    }

    /// <summary>Open a chest result while moving the cursor off the outgoing search menu first.</summary>
    private bool OpenChestResult(ManagedChest chest)
    {
        this.DeselectSearchBoxes();
        this.PendingChestToOpen = chest;
        return true;
    }

    /// <summary>Deselect every search box.</summary>
    private void DeselectSearchBoxes()
    {
        foreach (ChestSearchBox searchBox in this.SearchBoxes)
            searchBox.TextBox.Selected = false;
    }

    /// <summary>Ensure the menu still has a valid focused component after layout or search changes.</summary>
    private void EnsureFocusedComponent()
    {
        ChestSearchBox? selectedSearchBox = this.GetSelectedSearchBox();
        if (selectedSearchBox != null)
        {
            this.SetFocusedComponent(selectedSearchBox.Clickable);
            return;
        }

        if (this.currentlySnappedComponent is ChestSearchMenuCell cell && (!cell.visible || cell.Chest == null))
            this.currentlySnappedComponent = null;

        this.currentlySnappedComponent ??= this.NameSearchBox.Clickable;
    }

    /// <summary>Set the focused clickable component.</summary>
    private void SetFocusedComponent(ClickableComponent? component, bool snapCursor = true)
    {
        this.currentlySnappedComponent = component;
        if (component != null && this.IsHeaderComponent(component))
            this.LastHeaderComponent = component;

        if (snapCursor && component != null)
            this.snapCursorToCurrentSnappedComponent();
    }

    /// <summary>Move the cursor away from the search menu so the newly opened chest doesn't inherit a stale click target.</summary>
    private void MoveCursorOffMenu()
    {
        int x = Math.Max(0, this.xPositionOnScreen - Game1.tileSize);
        int y = Math.Max(0, this.yPositionOnScreen - Game1.tileSize);
        Game1.setMousePosition(x, y);
    }

    /// <summary>Handle a directional navigation input using a stable linear result list.</summary>
    private bool TryHandleDirectionalNavigation(Keys key)
    {
        bool upPressed = Game1.options.doesInputListContain(Game1.options.moveUpButton, key);
        bool downPressed = Game1.options.doesInputListContain(Game1.options.moveDownButton, key);
        bool leftPressed = Game1.options.doesInputListContain(Game1.options.moveLeftButton, key);
        bool rightPressed = Game1.options.doesInputListContain(Game1.options.moveRightButton, key);

        if (downPressed)
            return this.MoveFocusDown();
        if (upPressed)
            return this.MoveFocusUp();
        if (leftPressed)
            return this.MoveHeaderFocus(-1);
        if (rightPressed)
            return this.MoveHeaderFocus(1);

        return false;
    }

    /// <summary>Move focus down through the stable result list.</summary>
    private bool MoveFocusDown()
    {
        int focusedResultIndex = this.GetFocusedResultIndex();
        if (focusedResultIndex >= 0)
        {
            if (focusedResultIndex + 1 >= this.VisibleChests.Count)
                return false;

            this.FocusResult(focusedResultIndex + 1);
            return true;
        }

        if (this.VisibleChests.Count == 0)
            return false;

        this.FocusResult(0);
        return true;
    }

    /// <summary>Move focus up through the stable result list.</summary>
    private bool MoveFocusUp()
    {
        int focusedResultIndex = this.GetFocusedResultIndex();
        if (focusedResultIndex > 0)
        {
            this.FocusResult(focusedResultIndex - 1);
            return true;
        }

        if (focusedResultIndex == 0)
        {
            this.SetFocusedComponent(this.LastHeaderComponent ?? this.NameSearchBox.Clickable);
            return true;
        }

        return false;
    }

    /// <summary>Move focus left or right across the header controls.</summary>
    private bool MoveHeaderFocus(int direction)
    {
        if (this.currentlySnappedComponent is ChestSearchMenuCell)
            return false;

        ClickableComponent[] headerComponents = [this.NameSearchBox.Clickable, this.ItemSearchBox.Clickable, this.upperRightCloseButton];
        int currentIndex = Array.FindIndex(headerComponents, component => ReferenceEquals(component, this.currentlySnappedComponent));
        if (currentIndex < 0)
            currentIndex = 0;

        int newIndex = Math.Clamp(currentIndex + direction, 0, headerComponents.Length - 1);
        if (newIndex == currentIndex)
            return false;

        this.SetFocusedComponent(headerComponents[newIndex]);
        return true;
    }

    /// <summary>Focus a search result by its absolute index in the filtered chest list.</summary>
    private void FocusResult(int resultIndex)
    {
        if (resultIndex < 0 || resultIndex >= this.VisibleChests.Count)
            return;

        if (resultIndex < this.ScrollIndex || resultIndex >= this.ScrollIndex + this.ChestCellPool.Length)
        {
            this.ScrollIndex = Math.Clamp(resultIndex, 0, Math.Max(0, this.VisibleChests.Count - 1));
            this.UpdateVisibleCells();
        }

        int visibleIndex = resultIndex - this.ScrollIndex;
        if (visibleIndex < 0 || visibleIndex >= this.VisibleChestCells.Count)
            return;

        this.SetFocusedComponent(this.VisibleChestCells[visibleIndex]);
    }

    /// <summary>Get the absolute index of the currently focused result, if any.</summary>
    private int GetFocusedResultIndex()
    {
        if (this.currentlySnappedComponent is not ChestSearchMenuCell cell || cell.Chest == null)
            return -1;

        int visibleIndex = this.VisibleChestCells.IndexOf(cell);
        return visibleIndex >= 0
            ? this.ScrollIndex + visibleIndex
            : -1;
    }

    /// <summary>Get whether a component belongs to the header controls.</summary>
    private bool IsHeaderComponent(ClickableComponent component)
    {
        return ReferenceEquals(component, this.NameSearchBox.Clickable)
            || ReferenceEquals(component, this.ItemSearchBox.Clickable)
            || ReferenceEquals(component, this.upperRightCloseButton);
    }

    /// <summary>Get the currently selected search box, if any.</summary>
    private ChestSearchBox? GetSelectedSearchBox()
    {
        return this.SearchBoxes.FirstOrDefault(searchBox => searchBox.TextBox.Selected);
    }

    /// <summary>Get the actionable component currently under the cursor, if any.</summary>
    private ClickableComponent? GetActionTargetAt(int x, int y)
    {
        if (this.upperRightCloseButton.containsPoint(x, y))
            return this.upperRightCloseButton;

        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            if (searchBox.Clickable.bounds.Contains(x, y))
                return searchBox.Clickable;
        }

        foreach (ChestSearchMenuCell cell in this.VisibleChestCells)
        {
            if (cell.bounds.Contains(x, y))
                return cell;
        }

        return null;
    }

    /// <summary>Speak the currently focused element once.</summary>
    private void TrySpeakFocusedComponent(bool interrupt = true, bool force = false)
    {
        if (!this.StardewAccess.IsLoaded || this.currentlySnappedComponent == null)
            return;

        string? text = null;
        string? query = null;

        if (ReferenceEquals(this.currentlySnappedComponent, this.NameSearchBox.Clickable))
        {
            text = this.GetSearchBoxSpeech(this.NameSearchBox);
            query = $"search-box:name:{this.NameSearchBox.TextBox.Text}:{this.NameSearchBox.TextBox.Selected}";
        }
        else if (ReferenceEquals(this.currentlySnappedComponent, this.ItemSearchBox.Clickable))
        {
            text = this.GetSearchBoxSpeech(this.ItemSearchBox);
            query = $"search-box:item:{this.ItemSearchBox.TextBox.Text}:{this.ItemSearchBox.TextBox.Selected}";
        }
        else if (ReferenceEquals(this.currentlySnappedComponent, this.upperRightCloseButton))
        {
            text = "Close";
            query = "search-close";
        }
        else if (this.currentlySnappedComponent is ChestSearchMenuCell cell && cell.visible && cell.Chest != null)
        {
            text = cell.ScreenReaderText;
            query = $"search-result:{cell.Chest.DisplayCategory}:{cell.Chest.DisplayName}";
        }

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
            return;

        query = $"chests-search:{query}";
        if (!force && string.Equals(query, this.LastFocusedQuery, StringComparison.Ordinal))
            return;

        this.LastFocusedQuery = query;
        this.StardewAccess.SayWithMenuChecker(text, interrupt, query);
    }

    /// <summary>Get the spoken text for a search box.</summary>
    private string GetSearchBoxSpeech(ChestSearchBox searchBox)
    {
        string value = string.IsNullOrWhiteSpace(searchBox.TextBox.Text)
            ? "blank"
            : searchBox.TextBox.Text;

        return $"{searchBox.Label}, {value}";
    }
}
