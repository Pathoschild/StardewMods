using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ChestsAnywhere.Menus.Components
{
    /// <summary>A drawable item slot.</summary>
    public class ItemSlot
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The bounds within which to draw the slot.</summary>
        public Rectangle Bounds { get; }

        /// <summary>The underlying inventory.</summary>
        public Inventory Inventory { get; }

        /// <summary>The slot's index in the underlying inventory.</summary>
        public int Index { get; }

        /// <summary>Whether items can be added to this slot. (For example, this is <c>false</c> for slots unavailable due to a missing backpack upgrade.)</summary>
        public bool IsUsable { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="bounds">The dimensions within which to draw the slot.</param>
        /// <param name="inventory">The underlying inventory.</param>
        /// <param name="index">The index in the underlying inventory.</param>
        /// <param name="isUsable">Whether items can be added to this slot.</param>
        public ItemSlot(Rectangle bounds, Inventory inventory, int index, bool isUsable)
        {
            this.Bounds = bounds;
            this.Inventory = inventory;
            this.Index = index;
            this.IsUsable = isUsable;
        }

        /// <summary>Get whether there's an item in this slot.</summary>
        public bool HasItem()
        {
            return this.Inventory.GetAt(this.Index) != null;
        }

        /// <summary>Get the item stack in this slot.</summary>
        public Item GetStack()
        {
            return this.Inventory.GetAt(this.Index);
        }

        /// <summary>Get whether the slot contains the viewport-relative coordinate.</summary>
        /// <param name="x">The X position to check.</param>
        /// <param name="y">The Y position to check.</param>
        public bool ContainsPoint(int x, int y)
        {
            return this.Bounds.Contains(x, y);
        }

        /// <summary>Draw the inventory slot to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="opacity">The opacity at which to draw the slot.</param>
        public void Draw(SpriteBatch batch, float opacity = 1f)
        {
            // slot
            batch.Draw(Sprites.Menu.Sheet, this.IsUsable ? Sprites.Menu.Slot : Sprites.Menu.SlotDisabled, this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height, Color.White * opacity);

            // item
            this.GetStack()?.drawInMenu(batch, new Vector2(this.Bounds.X, this.Bounds.Y), this.ContainsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f, opacity, 1f);
        }
    }
}