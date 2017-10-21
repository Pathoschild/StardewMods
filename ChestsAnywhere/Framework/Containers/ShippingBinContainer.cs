using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for the shipping bin.</summary>
    internal class ShippingBinContainer : IContainer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The farm containing the shipping bin.</summary>
        private readonly Farm Farm;

        /// <summary>A fake chest wrapped around the shipping bin to use chest behaviour.</summary>
        private readonly Chest FakeChest;

        /// <summary>Simplifies access to private game data.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        public List<Item> Inventory => this.Farm.shippingBin;

        /// <summary>The container's name.</summary>
        public string Name { get; set; }

        /// <summary>Whether the player can configure the container.</summary>
        public bool IsEditable { get; } = false;

        /// <summary>Whether to enable chest-specific UI.</summary>
        public bool IsChest { get; } = false;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farm">The farm whose shipping bin to manage.</param>
        /// <param name="reflection">Simplifies access to private game data.</param>
        public ShippingBinContainer(Farm farm, IReflectionHelper reflection)
        {
            this.Farm = farm;
            this.Reflection = reflection;
            this.FakeChest = new Chest(0, farm.shippingBin, Vector2.Zero);
        }

        /// <summary>Get whether the in-game container is open.</summary>
        public bool IsOpen()
        {
            TemporaryAnimatedSprite lid = this.Reflection.GetPrivateValue<TemporaryAnimatedSprite>(this.Farm, "shippingBinLid");
            return lid != null && lid.currentParentTileIndex != lid.initialParentTileIndex;
        }

        /// <summary>Get whether the container has its default name.</summary>
        public bool HasDefaultName()
        {
            return true; // name isn't editable
        }

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        public bool CanAcceptItem(Item item)
        {
            return Utility.highlightShippableObjects(item);
        }

        /// <summary>Add an item to the container from the player inventory.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        public void GrabItemFromInventory(Item item, SFarmer player)
        {
            // note: we deliberately use the chest logic here instead of Farm::shipItem, which does
            // some weird things that don't work well with a full chest UI.
            this.FakeChest.grabItemFromInventory(item, player);
        }

        /// <summary>Add an item to the player inventory from the container.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        public void GrabItemFromContainer(Item item, SFarmer player)
        {
            if (!player.couldInventoryAcceptThisItem(item))
                return;

            this.FakeChest.grabItemFromChest(item, player);
            if (item == this.Farm.lastItemShipped)
                this.Farm.lastItemShipped = this.Farm.shippingBin.LastOrDefault();
        }
    }
}
