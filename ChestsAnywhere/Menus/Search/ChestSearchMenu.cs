using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Search;

internal sealed class ChestSearchMenu : IClickableMenu
{
    private const int RowCount = 8;
    private const int ColCount = 4;
    private const int Margin = 16;
    private const int CellWidth = 320;
    private const int CellHeight = 80;
    private const int SearchBarHeight = 96;
    private const int HoverItemsCol = 12;
    private const int HoverItemsMax = 36;
    private readonly Rectangle MenuRectBG = new(0, 256, 60, 60);

    private readonly ManagedChest[] Chests;
    private readonly IClickableMenu? FromMenu;
    private readonly KeybindList PreviewButton;

    private readonly List<ChestSearchMenuCell> ChestCells = [];
    private readonly List<ManagedChest> MatchedChests = [];
    private readonly SearchBox[] SearchBoxes;
    private IList<Item?>? HoverItems = null;

    private abstract record SearchBox(string Label, ValidatedTextBox Box)
    {
        protected string LastValue = string.Empty;
        internal virtual bool CheckChange()
        {
            if (!this.Box.Text.EqualsIgnoreCase(this.LastValue))
            {
                this.LastValue = this.Box.Text;
                return true;
            }
            return false;
        }

        internal bool HasValue() => !string.IsNullOrEmpty(this.LastValue);

        internal abstract bool Matches(ManagedChest chest);
    }

    private sealed record ChestNameSearchBox(string Label, ValidatedTextBox Box) : SearchBox(Label, Box)
    {
        internal override bool Matches(ManagedChest chest)
        {
            return chest.DisplayName.ContainsIgnoreCase(this.LastValue) || chest.DisplayCategory.ContainsIgnoreCase(this.LastValue);
        }
    }

    private sealed record ChestItemSearchBox(string Label, ValidatedTextBox Box) : SearchBox(Label, Box)
    {
        internal readonly Dictionary<ManagedChest, IList<Item?>> MatchedItems = [];

        internal override bool CheckChange()
        {
            if (base.CheckChange())
            {
                this.MatchedItems.Clear();
                return true;
            }
            return false;
        }

        internal override bool Matches(ManagedChest chest)
        {
            List<Item?> chestMatchedItems = [];
            foreach (Item? item in chest.Container.Inventory)
            {
                if (item?.DisplayName.ContainsIgnoreCase(this.LastValue) ?? false)
                {
                    chestMatchedItems.Add(item);
                }
            }
            this.MatchedItems[chest] = chestMatchedItems;
            return chestMatchedItems.Any();
        }
    }

    internal ChestSearchMenu(ManagedChest[] chests, IClickableMenu fromMenu, KeybindList PreviewButton) : base(
            Game1.viewport.X + 96,
            Game1.viewport.Y + 96,
            CellWidth * ColCount + Margin * 2,
            SearchBarHeight + CellHeight * RowCount + Margin * 2,
            showUpperRightCloseButton: false
        )
    {
        this.Chests = chests;
        this.FromMenu = fromMenu;
        this.PreviewButton = PreviewButton;

        Vector2 position = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
        this.xPositionOnScreen = (int)position.X;
        this.yPositionOnScreen = (int)position.Y;

        for (int i = 0; i < (ColCount * RowCount); i++)
        {
            int row = i / ColCount;
            int col = i % ColCount;
            int myID = 100 + i;
            ChestSearchMenuCell chestSearchMenuCell = new(new(0, 0, CellWidth, CellHeight), $"ChestSearchMenuCell_{i}")
            {
                BaseX = Margin + CellWidth * col,
                BaseY = Margin + SearchBarHeight + CellHeight * row,
                myID = myID,
                upNeighborID = row > 0 ? myID - ColCount : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                leftNeighborID = col > 0 ? myID - 1 : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = col < ColCount - 1 ? myID + 1 : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                downNeighborID =
                         row < RowCount - 1 ? myID + ColCount : ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
            };
            chestSearchMenuCell.Reposition(this.xPositionOnScreen, this.yPositionOnScreen);
            this.ChestCells.Add(chestSearchMenuCell);
        }

        int searchBoxWidth = (this.width - Margin) / 2;
        int searchBoxOffset = (int)Math.Max(Game1.smallFont.MeasureString(I18n.SearchBox_ChestName()).Y, Game1.smallFont.MeasureString(I18n.SearchBox_ItemName()).Y);
        this.SearchBoxes = [
            new ChestNameSearchBox(
                I18n.SearchBox_ChestName(),
                new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|')
                {
                    X = this.xPositionOnScreen + Margin,
                    Y = this.yPositionOnScreen + Margin + searchBoxOffset,
                    Width = searchBoxWidth,
                }
            ),
            new ChestItemSearchBox(
                I18n.SearchBox_ItemName(),
                new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|')
                {
                    X = this.xPositionOnScreen + Margin + searchBoxWidth,
                    Y = this.yPositionOnScreen + Margin + searchBoxOffset,
                    Width = searchBoxWidth,
                }
            )
        ];
    }

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

        foreach (SearchBox searchBox in this.SearchBoxes)
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
            int count = Math.Min(this.HoverItems.Count, this.HoverItems.Count(item => item is not null));
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
        this.drawMouse(b, ignore_transparency: true);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        foreach (SearchBox searchBox in this.SearchBoxes)
        {
            searchBox.Box.Selected = false;
        }
        foreach (SearchBox searchBox in this.SearchBoxes)
        {
            if (searchBox.Box.GetBounds().Contains(x, y))
            {
                searchBox.Box.Select();
                return;
            }
        }
        foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
        {
            if (cell.bounds.Contains(x, y))
            {
                if (this.FromMenu is IDisposable disposable && !this.FromMenu.HasDependencies())
                {
                    disposable.Dispose();
                }
                chest.OpenMenu();
                return;
            }
        }
        base.receiveLeftClick(x, y, playSound);
    }

    public override void performHoverAction(int x, int y)
    {
        ChestItemSearchBox itemSearchBox = (this.SearchBoxes[1] as ChestItemSearchBox)!;
        if (itemSearchBox.HasValue())
        {
            foreach ((ChestSearchMenuCell cell, ManagedChest chest) in this.IterateVisibleChestCells())
            {
                if (cell.bounds.Contains(x, y))
                {
                    if (itemSearchBox.MatchedItems.TryGetValue(chest, out IList<Item?>? matchedItems))
                    {
                        this.HoverItems = matchedItems;
                    }
                    return;
                }
            }
        }
        else if (this.PreviewButton.IsDown())
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

    protected override void cleanupBeforeExit()
    {
        if (this.FromMenu != null)
        {
            Game1.nextClickableMenu.Add(this.FromMenu);
        }
        base.cleanupBeforeExit();
    }

    private bool RecheckSearchState()
    {
        bool changed = false;
        List<SearchBox> hasSearch = [];
        foreach (SearchBox searchBox in this.SearchBoxes)
        {
            changed = searchBox.CheckChange() || changed;
            if (searchBox.HasValue())
            {
                hasSearch.Add(searchBox);
            }
        }
        if (changed)
        {
            this.MatchedChests.Clear();
            if (hasSearch.Any())
            {
                foreach (ManagedChest chest in this.Chests)
                {
                    foreach (SearchBox searchBox in hasSearch)
                    {
                        if (searchBox.Matches(chest))
                        {
                            this.MatchedChests.Add(chest);
                        }
                    }
                }
            }
        }
        return hasSearch.Any();
    }

    private IEnumerable<(ChestSearchMenuCell, ManagedChest)> IterateVisibleChestCells()
    {
        if (this.RecheckSearchState())
        {
            for (int i = 0; i < Math.Min(this.ChestCells.Count, this.MatchedChests.Count); i++)
            {
                yield return new(this.ChestCells[i], this.MatchedChests[i]);
            }
        }
        else
        {
            for (int i = 0; i < Math.Min(this.ChestCells.Count, this.Chests.Length); i++)
            {
                yield return new(this.ChestCells[i], this.Chests[i]);
            }
        }
    }
}
