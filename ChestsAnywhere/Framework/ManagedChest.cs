using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A chest with metadata.</summary>
    internal class ManagedChest
    {
        /*********
        ** Properties
        *********/
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";


        /*********
        ** Accessors
        *********/
        /// <summary>The storage container.</summary>
        public IContainer Container { get; }

        /// <summary>The chest's display name.</summary>
        public string Name { get; private set; }

        /// <summary>The category name (if any).</summary>
        public string Category { get; private set; }

        /// <summary>Whether the chest should be ignored.</summary>
        public bool IsIgnored { get; private set; }

        /// <summary>The sort value (if any).</summary>
        public int? Order { get; private set; }

        /// <summary>The name of the location or building which contains the chest.</summary>
        public string LocationName { get; }

        /// <summary>The chest's tile position within its location or building.</summary>
        public Vector2 Tile { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="container">The storage container.</param>
        /// <param name="location">The name of the location or building which contains the chest.</param>
        /// <param name="tile">The chest's tile position within its location or building.</param>
        /// <param name="defaultName">The default name to display if it hasn't been customised.</param>
        public ManagedChest(IContainer container, string location, Vector2 tile, string defaultName)
        {
            // save values
            this.Container = container;
            this.LocationName = location;
            this.Tile = tile;
            this.Name = !container.HasDefaultName() ? container.Name : defaultName;

            // extract tags
            foreach (Match match in Regex.Matches(this.Name, ManagedChest.TagGroupPattern))
            {
                string tag = match.Groups[1].Value;

                // ignore
                if (tag.ToLower() == "ignore")
                {
                    this.IsIgnored = true;
                    continue;
                }

                // category
                if (tag.ToLower().StartsWith("cat:"))
                {
                    this.Category = tag.Substring(4).Trim();
                    continue;
                }

                // order
                if (int.TryParse(tag, out int order))
                    this.Order = order;
            }
            this.Name = Regex.Replace(this.Name, ManagedChest.TagGroupPattern, "").Trim();

            // normalise
            if (this.Category == null)
                this.Category = "";
        }

        /// <summary>Get the grouping category for a chest.</summary>
        public string GetGroup()
        {
            return !string.IsNullOrWhiteSpace(this.Category)
                ? this.Category
                : this.LocationName;
        }

        /// <summary>Update the chest metadata.</summary>
        /// <param name="name">The chest's display name.</param>
        /// <param name="category">The category name (if any).</param>
        /// <param name="order">The sort value (if any).</param>
        /// <param name="ignored">Whether the chest should be ignored.</param>
        public void Update(string name, string category, int? order, bool ignored)
        {
            this.Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : this.Name;
            this.Category = category?.Trim() ?? "";
            this.Order = order;
            this.IsIgnored = ignored;

            this.Update();
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        /// <remarks>Derived from <see cref="StardewValley.Objects.Chest.updateWhenCurrentLocation"/>.</remarks>
        public IClickableMenu OpenMenu()
        {
            return new ItemGrabMenu(
                inventory: this.Container.Inventory,
                reverseGrab: false,
                showReceivingMenu: true,
                highlightFunction: InventoryMenu.highlightAllItems,
                behaviorOnItemSelectFunction: this.Container.GrabItemFromInventory,
                message: null,
                behaviorOnItemGrab: this.Container.GrabItemFromContainer,
                canBeExitedWithKey: true,
                showOrganizeButton: true,
                source: ItemGrabMenu.source_chest
            );
        }

        /// <summary>Get whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            return this.Container != null && this.Container.Equals((obj as ManagedChest)?.Container);
        }

        /// <summary>Serves as the default hash function.</summary>
        public override int GetHashCode()
        {
            return this.Container.GetHashCode();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Update the chest metadata.</summary>
        private void Update()
        {
            string name = this.Name;
            if (this.Order.HasValue)
                name += $" |{this.Order}|";
            if (this.IsIgnored)
                name += " |ignore|";
            if (!string.IsNullOrWhiteSpace(this.Category))
                name += $" |cat:{this.Category}|";

            this.Container.Name = name;
        }
    }
}
