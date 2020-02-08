using System.Text.RegularExpressions;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The model for custom container data.</summary>
    internal class ContainerData
    {
        /*********
        ** Fields
        *********/
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";


        /*********
        ** Accessors
        *********/
        /// <summary>The default name for the container type, if any.</summary>
        public string DefaultInternalName { get; set; }

        /// <summary>The display name.</summary>
        public string Name { get; set; }

        /// <summary>The category name (if any).</summary>
        public string Category { get; set; }

        /// <summary>Whether the container should be ignored.</summary>
        public bool IsIgnored { get; set; }

        /// <summary>Whether Automate should take items from this container.</summary>
        public ContainerAutomatePreference AutomateTakeItems { get; set; } = ContainerAutomatePreference.Allow;

        /// <summary>Whether Automate should put items in this container.</summary>
        public ContainerAutomatePreference AutomateStoreItems { get; set; } = ContainerAutomatePreference.Allow;

        /// <summary>The sort value (if any).</summary>
        public int? Order { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        public ContainerData() { }

        /// <summary>Construct an empty instance.</summary>
        /// <param name="defaultInternalName">The game's default name for the container, if any.</param>
        public ContainerData(string defaultInternalName)
        {
            this.DefaultInternalName = defaultInternalName;
        }

        /// <summary>Parse a serialized name string.</summary>
        /// <param name="name">The serialized name string.</param>
        /// <param name="defaultDisplayName">The default display name for the container.</param>
        public static ContainerData ParseName(string name, string defaultDisplayName)
        {
            // construct instance
            ContainerData data = new ContainerData(defaultDisplayName);
            if (string.IsNullOrWhiteSpace(name) || name == defaultDisplayName)
                return data;

            // read |tags|
            foreach (Match match in Regex.Matches(name, ContainerData.TagGroupPattern))
            {
                string tag = match.Groups[1].Value;

                // ignore
                if (tag.ToLower() == "ignore")
                    data.IsIgnored = true;

                // category
                else if (tag.ToLower().StartsWith("cat:"))
                    data.Category = tag.Substring(4).Trim();

                // order
                else if (int.TryParse(tag, out int order))
                    data.Order = order;

                // Automate options
                else if (tag.ToLower() == "automate:no-store")
                    data.AutomateStoreItems = ContainerAutomatePreference.Disable;
                else if (tag.ToLower() == "automate:prefer-store")
                    data.AutomateStoreItems = ContainerAutomatePreference.Prefer;
                else if (tag.ToLower() == "automate:no-take")
                    data.AutomateTakeItems = ContainerAutomatePreference.Disable;
                else if (tag.ToLower() == "automate:prefer-take")
                    data.AutomateTakeItems = ContainerAutomatePreference.Prefer;
            }

            // read display name
            name = Regex.Replace(name, ContainerData.TagGroupPattern, "").Trim();
            data.Name = !string.IsNullOrWhiteSpace(name) && name != defaultDisplayName
                ? name
                : defaultDisplayName;

            return data;
        }

        /// <summary>Get a serialized name representation of the container data.</summary>
        public string ToName()
        {
            // name
            string internalName = !this.HasDefaultDisplayName() ? this.Name : this.DefaultInternalName;

            // order
            if (this.Order.HasValue && this.Order != 0)
                internalName += $" |{this.Order}|";

            // ignore
            if (this.IsIgnored)
                internalName += " |ignore|";

            // category
            if (!string.IsNullOrWhiteSpace(this.Category))
                internalName += $" |cat:{this.Category}|";

            // Automate input
            if (!this.AutomateStoreItems.IsAllowed())
                internalName += " |automate:no-store|";
            else if (this.AutomateStoreItems.IsPreferred())
                internalName += " |automate:prefer-store|";

            // Automate output
            if (!this.AutomateTakeItems.IsAllowed())
                internalName += " |automate:no-take|";
            else if (this.AutomateTakeItems.IsPreferred())
                internalName += " |automate:prefer-take|";

            return internalName;
        }

        /// <summary>Whether the container has the default display name.</summary>
        public bool HasDefaultDisplayName()
        {
            return string.IsNullOrWhiteSpace(this.Name) || this.Name == this.DefaultInternalName;
        }

        /// <summary>Get whether the container has any non-default data.</summary>
        public bool HasData()
        {
            return
                !this.HasDefaultDisplayName()
                || (this.Order.HasValue && this.Order != 0)
                || this.IsIgnored
                || !string.IsNullOrWhiteSpace(this.Category)
                || this.AutomateTakeItems != ContainerAutomatePreference.Allow
                || this.AutomateStoreItems != ContainerAutomatePreference.Allow;
        }

        /// <summary>Reset all container data to the default.</summary>
        public void Reset()
        {
            this.Name = this.DefaultInternalName;
            this.Order = null;
            this.IsIgnored = false;
            this.Category = null;
            this.AutomateTakeItems = ContainerAutomatePreference.Allow;
            this.AutomateStoreItems = ContainerAutomatePreference.Allow;
        }
    }
}
