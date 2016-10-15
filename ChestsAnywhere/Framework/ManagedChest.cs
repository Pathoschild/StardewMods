using System.Text.RegularExpressions;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ChestsAnywhere.Framework
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
        /// <summary>The chest instance.</summary>
        public Chest Chest { get; }

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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest instance.</param>
        /// <param name="location">The name of the location or building which contains the chest.</param>
        /// <param name="defaultName">The default name if it hasn't been customised.</param>
        public ManagedChest(Chest chest, string location, string defaultName = "Chest")
        {
            // save values
            this.Chest = chest;
            this.LocationName = location;
            this.Name = chest.Name != "Chest"
                ? chest.Name
                : defaultName;

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
                int order;
                if (int.TryParse(tag, out order))
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
            return new ItemGrabMenu(this.Chest.items, false, true, InventoryMenu.highlightAllItems, this.Chest.grabItemFromInventory, null, this.Chest.grabItemFromChest, false, true, true, true, true, 1);
        }

        /// <summary>Get whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            return this.Chest != null && this.Chest.Equals((obj as ManagedChest)?.Chest);
        }

        /// <summary>Serves as the default hash function.</summary>
        public override int GetHashCode()
        {
            return this.Chest.GetHashCode();
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

            this.Chest.name = name;
        }
    }
}