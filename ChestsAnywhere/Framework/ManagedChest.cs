using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A chest with metadata.</summary>
    internal class ManagedChest
    {
        /*********
        ** Fields
        *********/
        /// <summary>The default name to display if it hasn't been customized.</summary>
        private readonly string DefaultDisplayName;

        /// <summary>The default category to display if it hasn't been customized.</summary>
        private readonly string DefaultCategory;


        /*********
        ** Accessors
        *********/
        /// <summary>The storage container.</summary>
        public IContainer Container { get; }

        /// <summary>The location or building which contains the chest.</summary>
        public GameLocation Location { get; }

        /// <summary>The chest's tile position within its location or building.</summary>
        public Vector2 Tile { get; }

        /// <summary>Whether the player can customize the container data.</summary>
        public bool CanEdit => this.Container.IsDataEditable;

        /// <summary>Whether Automate options can be configured for this chest.</summary>
        public bool CanConfigureAutomate => this.Container.CanConfigureAutomate;

        /// <summary>The user-friendly display name.</summary>
        public string DisplayName => !this.Container.Data.HasDefaultDisplayName() ? this.Container.Data.Name : this.DefaultDisplayName;

        /// <summary>The user-friendly category name (if any).</summary>
        public string DisplayCategory => this.Container.Data.Category ?? this.DefaultCategory;

        /// <summary>Whether the container should be ignored.</summary>
        public bool IsIgnored => this.Container.Data.IsIgnored;

        /// <summary>Whether Automate should take items from this container.</summary>
        public ContainerAutomatePreference AutomateTakeItems => this.Container.Data.AutomateTakeItems;

        /// <summary>Whether Automate should put items in this container.</summary>
        public ContainerAutomatePreference AutomateStoreItems => this.Container.Data.AutomateStoreItems;

        /// <summary>The sort value (if any).</summary>
        public int? Order => this.Container.Data.Order;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="container">The storage container.</param>
        /// <param name="location">The location or building which contains the chest.</param>
        /// <param name="tile">The chest's tile position within its location or building.</param>
        /// <param name="defaultDisplayName">The default name to display if it hasn't been customized.</param>
        /// <param name="defaultCategory">The default category to display if it hasn't been customized.</param>
        public ManagedChest(IContainer container, GameLocation location, Vector2 tile, string defaultDisplayName, string defaultCategory)
        {
            this.Container = container;
            this.Location = location;
            this.Tile = tile;
            this.DefaultDisplayName = defaultDisplayName;
            this.DefaultCategory = defaultCategory;
        }

        /// <summary>Reset all data to the default.</summary>
        public void Reset()
        {
            this.Container.Data.Reset();
        }

        /// <summary>Update the chest metadata.</summary>
        /// <param name="name">The chest's display name.</param>
        /// <param name="category">The category name (if any).</param>
        /// <param name="order">The sort value (if any).</param>
        /// <param name="ignored">Whether the chest should be ignored.</param>
        /// <param name="automateStoreItems">Whether Automate should take items from this container.</param>
        /// <param name="automateTakeItems">Whether Automate should put items in this container.</param>
        public void Update(string name, string category, int? order, bool ignored, ContainerAutomatePreference automateStoreItems, ContainerAutomatePreference automateTakeItems)
        {
            ContainerData data = this.Container.Data;

            data.Name = !string.IsNullOrWhiteSpace(name) && name != this.DefaultDisplayName
                ? name.Trim()
                : null;
            data.Category = !string.IsNullOrWhiteSpace(category) && category != this.DefaultCategory
                ? category.Trim()
                : null;
            data.Order = order;
            data.IsIgnored = ignored;
            data.AutomateStoreItems = automateStoreItems;
            data.AutomateTakeItems = automateTakeItems;

            this.Container.SaveData();
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        public IClickableMenu OpenMenu()
        {
            return this.Container.OpenMenu();
        }

        /// <summary>Get whether the container has its default name.</summary>
        public bool HasDefaultName()
        {
            return this.Container.Data.HasDefaultDisplayName();
        }
    }
}
