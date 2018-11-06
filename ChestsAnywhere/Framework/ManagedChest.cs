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
        ** Properties
        *********/
        /// <summary>The default name to display if it hasn't been customised.</summary>
        private readonly string DefaultDisplayName;


        /*********
        ** Accessors
        *********/
        /// <summary>The storage container.</summary>
        public IContainer Container { get; }

        /// <summary>The location or building which contains the chest.</summary>
        public GameLocation Location { get; }

        /// <summary>The chest's tile position within its location or building.</summary>
        public Vector2 Tile { get; }

        /// <summary>Whether the player can customise the container data.</summary>
        public bool CanEdit => this.Container.IsDataEditable;

        /// <summary>Whether Automate options can be configured for this chest.</summary>
        public bool CanConfigureAutomate => this.Container.CanConfigureAutomate;

        /// <summary>The user-friendly display name.</summary>
        public string DisplayName => this.Container.Data.Name ?? this.DefaultDisplayName;

        /// <summary>The user-friendly category name (if any).</summary>
        public string DisplayCategory => this.Container.Data.Name ?? this.Location.Name;

        /// <summary>Whether the container should be ignored.</summary>
        public bool IsIgnored => this.Container.Data.IsIgnored;

        /// <summary>Whether Automate should ignore this container.</summary>
        public bool ShouldAutomateIgnore => this.Container.Data.ShouldAutomateIgnore;

        /// <summary>Whether Automate should prefer this container for output.</summary>
        public bool ShouldAutomatePreferForOutput => this.Container.Data.ShouldAutomatePreferForOutput;

        /// <summary>The sort value (if any).</summary>
        public int? Order => this.Container.Data.Order;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="container">The storage container.</param>
        /// <param name="location">The location or building which contains the chest.</param>
        /// <param name="tile">The chest's tile position within its location or building.</param>
        /// <param name="defaultDisplayName">The default name to display if it hasn't been customised.</param>
        public ManagedChest(IContainer container, GameLocation location, Vector2 tile, string defaultDisplayName)
        {
            this.Container = container;
            this.Location = location;
            this.Tile = tile;
            this.DefaultDisplayName = defaultDisplayName;
        }

        /// <summary>Get the grouping category for a chest.</summary>
        public string GetGroup()
        {
            return !string.IsNullOrWhiteSpace(this.Container.Data.Category)
                ? this.Container.Data.Category
                : this.Location.Name;
        }

        /// <summary>Update the chest metadata.</summary>
        /// <param name="name">The chest's display name.</param>
        /// <param name="category">The category name (if any).</param>
        /// <param name="order">The sort value (if any).</param>
        /// <param name="ignored">Whether the chest should be ignored.</param>
        /// <param name="shouldAutomateIgnore">Whether Automate should ignore this chest.</param>
        /// <param name="shouldAutomatePreferForOutput">Whether Automate should prefer this chest for output.</param>
        public void Update(string name, string category, int? order, bool ignored, bool shouldAutomateIgnore, bool shouldAutomatePreferForOutput)
        {
            ContainerData data = this.Container.Data;

            data.Name = !string.IsNullOrWhiteSpace(name) && name != this.DefaultDisplayName
                ? name.Trim()
                : null;
            data.Category = !string.IsNullOrWhiteSpace(category) && category != this.Location.Name
                ? category.Trim()
                : null;
            data.Order = order;
            data.IsIgnored = ignored;
            data.ShouldAutomateIgnore = shouldAutomateIgnore;
            data.ShouldAutomatePreferForOutput = shouldAutomatePreferForOutput;

            this.Container.SaveData();
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        public ItemGrabMenu OpenMenu()
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
