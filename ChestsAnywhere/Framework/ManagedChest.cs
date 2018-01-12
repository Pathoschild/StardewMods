using System.Text.RegularExpressions;
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
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";

        /// <summary>The default name to display if it hasn't been customised.</summary>
        private readonly string DefaultName;


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

        /// <summary>The location or building which contains the chest.</summary>
        public GameLocation Location { get; }

        /// <summary>The chest's tile position within its location or building.</summary>
        public Vector2 Tile { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="container">The storage container.</param>
        /// <param name="location">The location or building which contains the chest.</param>
        /// <param name="tile">The chest's tile position within its location or building.</param>
        /// <param name="defaultName">The default name to display if it hasn't been customised.</param>
        public ManagedChest(IContainer container, GameLocation location, Vector2 tile, string defaultName)
        {
            // save values
            this.Container = container;
            this.Location = location;
            this.Tile = tile;
            this.DefaultName = defaultName;

            // parse name
            if (container.HasDefaultName() || string.IsNullOrWhiteSpace(container.Name))
                this.Name = this.DefaultName;
            else
            {
                string name = !container.HasDefaultName() ? container.Name : defaultName;

                // read |tags|
                foreach (Match match in Regex.Matches(name, ManagedChest.TagGroupPattern))
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

                // read display name
                name = Regex.Replace(name, ManagedChest.TagGroupPattern, "").Trim();
                this.Name = !string.IsNullOrWhiteSpace(name) && name != this.Container.DefaultName
                    ? name
                    : this.DefaultName;
            }
            // normalise
            if (this.Category == null)
                this.Category = "";
        }

        /// <summary>Get the grouping category for a chest.</summary>
        public string GetGroup()
        {
            return !string.IsNullOrWhiteSpace(this.Category)
                ? this.Category
                : this.Location.Name;
        }

        /// <summary>Update the chest metadata.</summary>
        /// <param name="name">The chest's display name.</param>
        /// <param name="category">The category name (if any).</param>
        /// <param name="order">The sort value (if any).</param>
        /// <param name="ignored">Whether the chest should be ignored.</param>
        public void Update(string name, string category, int? order, bool ignored)
        {
            // update high-level metadata
            this.Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : this.DefaultName;
            this.Category = category?.Trim() ?? "";
            this.Order = order;
            this.IsIgnored = ignored;

            // build internal name
            string internalName = !this.HasDefaultName() ? this.Name : this.Container.DefaultName;
            if (this.Order.HasValue)
                internalName += $" |{this.Order}|";
            if (this.IsIgnored)
                internalName += " |ignore|";
            if (!string.IsNullOrWhiteSpace(this.Category) && this.Category != this.Location.Name)
                internalName += $" |cat:{this.Category}|";

            // update container
            this.Container.Name = !string.IsNullOrWhiteSpace(internalName)
                ? internalName
                : this.Container.DefaultName;
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        public ItemGrabMenu OpenMenu()
        {
            return this.Container.OpenMenu();
        }

        /// <summary>Get whether the container has its default name.</summary>
        public bool HasDefaultName()
        {
            return string.IsNullOrWhiteSpace(this.Name) || this.Name == this.DefaultName;
        }
    }
}
