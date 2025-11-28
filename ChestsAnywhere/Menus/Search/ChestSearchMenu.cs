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

/// <summary>Chest searching menu</summary>
internal sealed class ChestSearchMenu : IClickableMenu
{
    /*********
    ** Constants
    *********/
    /// <summary>Menu border margin</summary>
    private const int Margin = 16;
    /// <summary>Menu row count</summary>
    private const int RowCount = 7;
    /// <summary>Menu column count</summary>
    private const int ColCount = 4;
    /// <summary>Cell width</summary>
    private const int CellWidth = 312;
    /// <summary>Cell height</summary>
    private const int CellHeight = 80;
    /// <summary>Height of search bar row</summary>
    private const int SearchBarHeight = 96;
    /// <summary>Max column count of item preview tooltip</summary>
    private const int HoverItemsCol = 12;
    /// <summary>Max total item count of item preview tooltip</summary>
    private const int HoverItemsMax = 36;
    /// <summary>A source rect in <see cref="Game1.menuTexture"/> to draw as background of this menu</summary>
    private readonly Rectangle MenuRectBG = new(0, 256, 60, 60);

    /*********
    ** State
    *********/
    /// <summary>All known and managed chests</summary>
    private readonly ManagedChest[] Chests;
    /// <summary>The configured key bindings.</summary>
    private readonly KeybindList PreviewKey;

    /// <summary>Chest cells for click checking and gamepad</summary>
    private readonly List<ChestSearchMenuCell> ChestCells = [];
    /// <summary>Chests that match current search terms</summary>
    private readonly List<ManagedChest> MatchedChests = [];
    /// <summary>Search box for name</summary>
    private readonly ChestSearchBox SearchBox_Name;
    /// <summary>Search box for items</summary>
    private readonly ChestSearchBox SearchBox_Item;
    /// <summary>Search text boses</summary>
    private readonly ChestSearchBox[] SearchBoxes;
    /// <summary>Items that are matched during a chest search</summary>
    private readonly Dictionary<ManagedChest, IList<Item?>> MatchedItemsInChest = [];
    /// <summary>Items to display in preview tooltip</summary>
    private IList<Item?>? HoverItems = null;
    /// <summary>Whether there are search terms in the search boxes</summary>
    private bool SearchState = false;
    /// <summary>Current scroll idx</summary>
    private int ScrollIdx = 0;

    /// <summary>Construct a new chest search menu</summary>
    /// <param name="chest">Currently selected chest</param>
    /// <param name="chests">All known chests</param>
    /// <param name="config">Mod configs</param>
    /// <param name="keys">Mod config keybinds</param>
    internal ChestSearchMenu(ManagedChest currentChest, ManagedChest[] chests, ModConfigKeys keys) : base(
        Game1.viewport.X + 96,
        Game1.viewport.Y + 96,
        CellWidth * ColCount + Margin * 2,
        SearchBarHeight + CellHeight * RowCount + Margin * 2,
        showUpperRightCloseButton: true
    )
    {
        this.Chests = chests;
        this.PreviewKey = keys.SearchMenuPreviewChest;
        this.exitFunction = () => currentChest.OpenMenu();

        // prebake the render target chest icons
        foreach (ManagedChest chest in this.Chests)
        {
            chest.Container.TryGetIcon(out _, out _, out _);
        }

        Vector2 position = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
        this.xPositionOnScreen = (int)position.X;
        this.yPositionOnScreen = (int)position.Y;

        for (int i = 0; i < (ColCount * RowCount); i++)
        {
            int row = i / ColCount;
            int col = i % ColCount;
            int myID = 100 + i;
            ChestSearchMenuCell chestSearchMenuCell = new(new(0, 0, CellWidth, CellHeight), $"ChestSearchMenuCell_{i}", Margin + CellWidth * col, Margin + SearchBarHeight + CellHeight * row)
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

        int searchBoxWidth = this.width / 2 - Margin * 2;
        int searchBoxOffset = (int)Math.Max(Game1.smallFont.MeasureString(I18n.SearchBox_ChestName()).Y, Game1.smallFont.MeasureString(I18n.SearchBox_ItemName()).Y);

        this.SearchBox_Name = new ChestSearchBox(
            I18n.SearchBox_ChestName(),
            new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|')
            {
                X = this.xPositionOnScreen + Margin,
                Y = this.yPositionOnScreen + Margin + searchBoxOffset,
                Width = searchBoxWidth,
            },
            (chest, term) => chest.DisplayName.ContainsIgnoreCase(term) || chest.DisplayCategory.ContainsIgnoreCase(term)
        );
        this.SearchBox_Item = new ChestSearchBox(
            I18n.SearchBox_ItemName(),
            new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|')
            {
                X = this.xPositionOnScreen + Margin + Margin / 2 + searchBoxWidth,
                Y = this.yPositionOnScreen + Margin + searchBoxOffset,
                Width = searchBoxWidth,
            },
            this.SearchItemsInChest
        );
        this.SearchBoxes = [
            this.SearchBox_Name,
            this.SearchBox_Item
        ];

        this.initializeUpperRightCloseButton();
        this.upperRightCloseButton.myID = upperRightCloseButton_ID;

        this.SearchBox_Name.Clickable.myID = 1001;
        this.SearchBox_Name.Clickable.downNeighborID = 101;
        this.SearchBox_Name.Clickable.rightNeighborID = 1002;

        this.SearchBox_Item.Clickable.myID = 1002;
        this.SearchBox_Item.Clickable.downNeighborID = 101 + ColCount / 2;
        this.SearchBox_Item.Clickable.leftNeighborID = 1001;
        this.SearchBox_Item.Clickable.rightNeighborID = upperRightCloseButton_ID;

        this.upperRightCloseButton.leftNeighborID = 1001;
        this.upperRightCloseButton.downNeighborID = 1001;

        if (Game1.options.snappyMenus && Game1.options.gamepadControls)
        {
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }
    }

    /// <inheritdoc/>
    public override void populateClickableComponentList()
    {
        this.allClickableComponents = [this.upperRightCloseButton, this.SearchBox_Name.Clickable, this.SearchBox_Item.Clickable];
        this.allClickableComponents.AddRange(this.ChestCells);
    }

    /// <inheritdoc/>
    public override void snapToDefaultClickableComponent()
    {
        this.currentlySnappedComponent = this.getComponentWithID(1001);
        base.snapToDefaultClickableComponent();
    }

    protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
    {
        if (oldID >= 100 && oldID < 100 + ColCount)
        {
            if (!this.ScrollGrid(1))
            {
                this.currentlySnappedComponent = this.getComponentWithID(oldID < 101 + ColCount / 2 ? 1001 : 1002);
                this.snapCursorToCurrentSnappedComponent();
            }
            return;
        }
        else if (oldID >= 100 + ColCount * (RowCount - 1))
        {
            this.ScrollGrid(-1);
            return;
        }
        base.customSnapBehavior(direction, oldRegion, oldID);
    }

    /// <inheritdoc/>
    public override void update(GameTime time)
    {
        this.RecheckSearchState();
        base.update(time);
    }

    /// <inheritdoc/>
    public override void draw(SpriteBatch b)
    {
        drawTextureBox(
            b,
            Game1.menuTexture,
            this.MenuRectBG,
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            Color.White
        );

        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            Utility.drawTextWithShadow(
                b,
                searchBox.Label,
                Game1.smallFont,
                new(searchBox.Box.X, this.yPositionOnScreen + Margin),
                Game1.textColor
            );
            searchBox.Box.Draw(b);
        }
        foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
        {
            cell.Draw(b, chest);
        }
        if (this.HoverItems != null)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            int count = Math.Min(HoverItemsMax, this.HoverItems.Count(item => item is not null));
            if (count == 0)
            {
                drawTextureBox(
                    b,
                    Game1.menuTexture,
                    this.MenuRectBG,
                    mouseX,
                    mouseY - 64 - Margin * 2,
                    64 + Margin * 2,
                    64 + Margin * 2,
                    Color.White
                );
            }
            else
            {
                int boxHeight = (int)MathF.Ceiling(count / (float)HoverItemsCol) * 64 + Margin * 2;
                drawTextureBox(
                    b,
                    Game1.menuTexture,
                    this.MenuRectBG,
                    mouseX,
                    mouseY - boxHeight,
                    Math.Min(count, HoverItemsCol) * 64 + Margin * 2,
                    boxHeight,
                    Color.White
                );
                Vector2 itemPos = new(mouseX + Margin, mouseY - boxHeight + Margin);
                for (int i = 0; i < count; i++)
                {
                    if (this.HoverItems[i] is Item item)
                    {
                        item.drawInMenu(b, itemPos, 1f);
                        if (i % HoverItemsCol == HoverItemsCol - 1)
                        {
                            itemPos.X = mouseX + Margin;
                            itemPos.Y += 64;
                        }
                        else
                        {
                            itemPos.X += 64;
                        }
                    }
                }
            }
        }
        if (this.upperRightCloseButton != null && this.shouldDrawCloseButton())
        {
            this.upperRightCloseButton.draw(b);
        }
        this.drawMouse(b, ignore_transparency: true);
    }

    /// <inheritdoc/>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            if (searchBox.Clickable.bounds.Contains(x, y))
            {
                if (searchBox.Box.Selected)
                {
                    searchBox.Box.Selected = false;
                }
                else
                {
                    foreach (ChestSearchBox searchBox2 in this.SearchBoxes)
                    {
                        searchBox2.Box.Selected = false;
                    }
                    searchBox.Box.Select();
                    return;
                }
            }
        }
        foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
        {
            if (cell.bounds.Contains(x, y))
            {
                chest.OpenMenu();
                return;
            }
        }
        base.receiveLeftClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        if (this.SearchBoxes.Any(searchBox => searchBox.Box.Selected))
        {
            return;
        }
        base.receiveKeyPress(key);
    }

    /// <inheritdoc/>
    public override void performHoverAction(int x, int y)
    {
        if (this.SearchBox_Item.HasValue)
        {
            foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
            {
                if (cell.bounds.Contains(x, y))
                {
                    if (this.MatchedItemsInChest.TryGetValue(chest, out IList<Item?>? matchedItems))
                    {
                        this.HoverItems = matchedItems;
                    }
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
        base.performHoverAction(x, y);
    }

    /// <inheritdoc/>
    public override void receiveScrollWheelAction(int direction)
    {
        this.ScrollGrid(direction);
        base.receiveScrollWheelAction(direction);
    }

    /// <summary>Tries to scroll the grid menu</summary>
    /// <param name="direction">Scroll direction, >0 is up and <0 is down</param>
    /// <returns></returns>
    private bool ScrollGrid(int direction)
    {
        bool scrolled = false;
        if (direction > 0 && this.ScrollIdx >= ColCount)
        {
            this.ScrollIdx -= ColCount;
            scrolled = true;
        }
        else if (direction < 0 && this.ScrollIdx < Math.Max(0, (this.SearchState ? this.MatchedChests.Count : this.Chests.Length) - this.ChestCells.Count))
        {
            this.ScrollIdx += ColCount;
            scrolled = true;
        }
        if (scrolled)
        {
            Game1.playSound("shiny4");
        }
        return scrolled;
    }

    /// <summary>Perform search on items in chest, and update <see cref="this.MatchedItemsInChest"/> with results</summary>
    /// <param name="chest"></param>
    /// <param name="term"></param>
    /// <returns></returns>
    private bool SearchItemsInChest(ManagedChest chest, string term)
    {
        List<Item?> chestMatchedItems = [];
        foreach (Item? item in chest.Container.Inventory)
        {
            if (item?.DisplayName.ContainsIgnoreCase(term) ?? false)
            {
                chestMatchedItems.Add(item);
            }
        }
        this.MatchedItemsInChest[chest] = chestMatchedItems;
        return chestMatchedItems.Any();
    }

    /// <summary>Recheck the search boxes to determine if there are search terms</summary>
    /// <returns>true if there are search terms</returns>
    private void RecheckSearchState()
    {
        bool changed = false;
        List<ChestSearchBox> hasSearch = [];
        foreach (ChestSearchBox searchBox in this.SearchBoxes)
        {
            changed = searchBox.CheckChange() || changed;
            if (searchBox.HasValue)
            {
                hasSearch.Add(searchBox);
            }
        }
        if (changed)
        {
            this.MatchedChests.Clear();
            if (hasSearch.Contains(this.SearchBox_Item))
            {
                this.MatchedItemsInChest.Clear();
            }
            if (hasSearch.Any())
            {
                foreach (ManagedChest chest in this.Chests)
                {
                    if (hasSearch.All(searchBox => searchBox.Matches(chest)))
                    {
                        this.MatchedChests.Add(chest);
                    }
                }
            }
        }
        bool newSearchState = hasSearch.Any();
        if (this.SearchState != newSearchState)
        {
            this.ScrollIdx = 0;
        }
        this.SearchState = newSearchState;
    }

    /// <summary>Iterate the visible chest cells</summary>
    /// <returns></returns>
    private IEnumerable<(ChestSearchMenuCell, ManagedChest)> IterateVisibleChestCells()
    {
        IList<ManagedChest> chests;
        int length;
        if (this.SearchState)
        {
            length = Math.Min(this.ChestCells.Count, this.MatchedChests.Count - this.ScrollIdx);
            chests = this.MatchedChests;
        }
        else
        {
            length = Math.Min(this.ChestCells.Count, this.Chests.Length - this.ScrollIdx);
            chests = this.Chests;
        }
        for (int i = 0; i < length; i++)
        {
            yield return new(this.ChestCells[i], chests[this.ScrollIdx + i]);
        }
    }
}
