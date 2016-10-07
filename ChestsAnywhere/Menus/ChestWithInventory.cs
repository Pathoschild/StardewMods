using System;
using System.Collections.Generic;
using System.Linq;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ChestsAnywhere.Menus
{
    /// <summary>A UI which lets the player transfer items between a chest and their inventory.</summary>
    internal class ChestWithInventory : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The selected chest's inventory.</summary>
        private Inventory ChestItems;

        /// <summary>The player inventory.</summary>
        private readonly Inventory PlayerItems;

        /// <summary>The clickable item slots.</summary>
        private readonly List<ItemSlot> Slots = new List<ItemSlot>();

        /// <summary>The item under the player's cursor.</summary>
        private Item HoveredItem;

        /// <summary>The selected chest.</summary>
        protected ManagedChest SelectedChest { get; private set; }

        /// <summary>Whether the UI control is disabled, in which case it will no longer try to handle player interaction.</summary>
        protected bool IsDisabled { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ChestWithInventory()
        {
            this.PlayerItems = new Inventory(Game1.player.Items);
            this.ReinitialiseComponents();
        }

        /// <summary>Open a chest's inventory.</summary>
        /// <param name="chest">The chest to open.</param>
        public virtual void SelectChest(ManagedChest chest)
        {
            this.SelectedChest = chest;
            this.ChestItems = new Inventory(chest.Chest.items, avoidGaps: true);
            this.ReinitialiseComponents();
        }

        /// <summary>The method invoked when the player's cursor on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        public override void performHoverAction(int x, int y)
        {
            if (this.IsDisabled)
                return;

            // find hovered item
            this.HoveredItem = this.GetSlotAt(x, y)?.GetStack();
        }

        /// <summary>The method invoked when the game window is resized.</summary>
        /// <param name="oldBounds">The previous window dimensions.</param>
        /// <param name="newBounds">The new window dimensions.</param>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            this.ReinitialiseComponents();
        }

        /// <summary>The method invoked when the player left-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.IsDisabled)
                return;

            // get clicked slot
            ItemSlot clickedSlot = this.GetSlotAt(x, y);
            if (clickedSlot == null || !clickedSlot.HasItem())
                return;
            Item stack = clickedSlot.GetStack();

            // transfer stack
            Inventory from = clickedSlot.Inventory;
            Inventory to = from == this.ChestItems ? this.PlayerItems : this.ChestItems;
            from.ReduceStack(clickedSlot.Index, to.AcceptStack(stack));
            if (playSound)
                Game1.playSound("coin");
        }

        /// <summary>The method invoked when the player right-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (this.IsDisabled)
                return;

            // get clicked slot
            ItemSlot clickedSlot = this.GetSlotAt(x, y);
            if (clickedSlot == null || !clickedSlot.HasItem())
                return;
            Item stack = clickedSlot.GetStack();

            // get inventories
            Inventory from = clickedSlot.Inventory;
            Inventory to = from == this.ChestItems ? this.PlayerItems : this.ChestItems;

            // get stack to transfer
            Item newStack = stack.getOne();
            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                newStack.Stack = Math.Max(1, stack.Stack / 2); // transfer half stack when holding left shift

            // transfer
            from.ReduceStack(clickedSlot.Index, to.AcceptStack(newStack));
            if (playSound)
                Game1.playSound("coin");
        }

        /// <summary>Render the inventory UI.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        public override void draw(SpriteBatch batch)
        {
            // background
            batch.DrawMenuBackground(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 16, this.yPositionOnScreen + Game1.tileSize / 16, this.width - Game1.tileSize / 8, this.height - Game1.tileSize / 8), bisect: true);

            // slots
            foreach (ItemSlot slot in this.Slots)
                slot.Draw(batch);

            // tooltips
            if (this.HoveredItem != null)
            {
                string hoverText = this.HoveredItem.getHoverBoxText(this.HoveredItem);
                string hoverTitle = null;
                if (hoverText == null)
                {
                    hoverText = this.HoveredItem.getDescription();
                    hoverTitle = this.HoveredItem.Name;
                }

                IClickableMenu.drawToolTip(batch, hoverText, hoverTitle, this.HoveredItem);
            }
        }

        /// <summary>Trigger the chest item sort behaviour.</summary>
        public void SortChestItems()
        {
            this.ChestItems.Sort();
            Game1.playSound("Ship");
        }

        /// <summary>Trigger the inventory item sort behaviour.</summary>
        public void SortInventory()
        {
            this.PlayerItems.Sort();
            Game1.playSound("Ship");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the item slot at the specified coordinate.</summary>
        /// <param name="x">The X position to check.</param>
        /// <param name="y">The Y position to check.</param>
        private ItemSlot GetSlotAt(int x, int y)
        {
            return this.Slots.FirstOrDefault(slot => slot.ContainsPoint(x, y));
        }

        /// <summary>Initialise the inventory for rendering.</summary>
        private void ReinitialiseComponents()
        {
            // dimensions
            this.width = Game1.tileSize * 13;
            this.height = Game1.tileSize * 7 + Game1.tileSize / 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2 + Game1.tileSize;

            // slots
            this.Slots.Clear();
            this.Slots.AddRange(this.BuildSlots(this.ChestItems, new Vector2(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + Game1.tileSize / 2)));
            this.Slots.AddRange(this.BuildSlots(this.PlayerItems, new Vector2(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4), maxItems: Game1.player.maxItems));
        }

        /// <summary>Construct the clickable slots for an inventory.</summary>
        /// <param name="inventory">The inventory represented by the slots.</param>
        /// <param name="position">The top-left position from which to draw the slots.</param>
        /// <param name="maxItems">The maximum number of items that can be stored. Slots beyond that number will be grayed out and marked unusable.</param>
        private IEnumerable<ItemSlot> BuildSlots(Inventory inventory, Vector2 position, int? maxItems = null)
        {
            int slotSize = Constant.SlotSize;
            int index = 0;
            for (int row = 0; row < Constant.SlotRows; row++)
            {
                for (int col = 0; col < Constant.SlotColumns; col++)
                {
                    bool isUsable = maxItems == null || index < maxItems;
                    yield return new ItemSlot(new Rectangle((int)position.X + col * slotSize, (int)position.Y + row * slotSize, slotSize, slotSize), inventory, index, isUsable);
                    index++;
                }
            }
        }
    }
}
