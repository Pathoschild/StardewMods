using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for a <see cref="StorageFurniture"/> instance (e.g. a dresser).</summary>
    internal class StorageFurnitureContainer : IContainer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The in-game storage furniture.</summary>
        internal readonly StorageFurniture Furniture;

        /// <summary>The categories accepted by a dresser.</summary>
        private static HashSet<int> DresserCategories;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        public IList<Item> Inventory => this.Furniture.heldItems;

        /// <summary>The persisted data for this container.</summary>
        public ContainerData Data { get; }

        /// <summary>Whether Automate options can be configured for this chest.</summary>
        public bool CanConfigureAutomate { get; } = false; // Automate doesn't support storage containers


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="furniture">The in-game storage furniture.</param>
        public StorageFurnitureContainer(StorageFurniture furniture)
        {
            this.Furniture = furniture;
            this.Data = new ContainerData(furniture.modData);

            StorageFurnitureContainer.DresserCategories ??= new HashSet<int>(new ShopMenu(new List<ISalable>(), context: "Dresser").categoriesToSellHere);
        }

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        public bool CanAcceptItem(Item item)
        {
            return StorageFurnitureContainer.DresserCategories.Contains(item.Category);
        }

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="container">The other container.</param>
        public bool IsSameAs(IContainer container)
        {
            return container != null && this.IsSameAs(container.Inventory);
        }

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="inventory">The other container's inventory.</param>
        public bool IsSameAs(IList<Item> inventory)
        {
            return object.ReferenceEquals(this.Inventory, inventory);
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        /// <remarks>Derived from <see cref="StorageFurniture.checkForAction"/>.</remarks>
        public IClickableMenu OpenMenu()
        {
            Dictionary<ISalable, int[]> itemPriceAndStock = this.Furniture.heldItems.ToDictionary(item => (ISalable)item, _ => new[] { 0, 1 });
            return new ShopMenu(itemPriceAndStock, 0, null, this.Furniture.onDresserItemWithdrawn, this.Furniture.onDresserItemDeposited, this.Furniture.GetShopMenuContext())
            {
                source = this.Furniture,
                behaviorBeforeCleanup = menu => this.Furniture.mutex.ReleaseLock()
            };
        }

        /// <summary>Persist the container data.</summary>
        public void SaveData()
        {
            this.Data.ToModData(this.Furniture.modData);
        }
    }
}
