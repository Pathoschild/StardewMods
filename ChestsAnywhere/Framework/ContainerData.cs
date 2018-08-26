using System.Text.RegularExpressions;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The model for custom container data.</summary>
    internal class ContainerData
    {
        /*********
        ** Properties
        *********/
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";


        /*********
        ** Accessors
        *********/
        /// <summary>The default name for the container type, if any.</summary>
        public string DefaultDisplayName { get; set; }

        /// <summary>The display name.</summary>
        public string Name { get; set; }

        /// <summary>The category name (if any).</summary>
        public string Category { get; set; }

        /// <summary>Whether the container should be ignored.</summary>
        public bool IsIgnored { get; set; }

        /// <summary>Whether Automate should ignore this container.</summary>
        public bool ShouldAutomateIgnore { get; set; }

        /// <summary>Whether Automate should prefer this container for output.</summary>
        public bool ShouldAutomatePreferForOutput { get; set; }

        /// <summary>The sort value (if any).</summary>
        public int? Order { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        public ContainerData() { }

        /// <summary>Construct an empty instance.</summary>
        /// <param name="defaultDisplayName">The game's default name for the container, if any.</param>
        public ContainerData(string defaultDisplayName)
        {
            this.DefaultDisplayName = defaultDisplayName;
        }

        /// <summary>Parse a serialised name string.</summary>
        /// <param name="name">The serialised name string.</param>
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
                else if (tag.ToLower() == "automate:ignore")
                    data.ShouldAutomateIgnore = true;
                else if (tag.ToLower() == "automate:output")
                    data.ShouldAutomatePreferForOutput = true;
            }

            // read display name
            name = Regex.Replace(name, ContainerData.TagGroupPattern, "").Trim();
            data.Name = !string.IsNullOrWhiteSpace(name) && name != defaultDisplayName
                ? name
                : defaultDisplayName;

            return data;
        }

        /// <summary>Get a serialised name representation of the container data.</summary>
        public string ToName()
        {
            string internalName = !this.HasDefaultDisplayName() ? this.Name : this.DefaultDisplayName;
            if (this.Order.HasValue && this.Order != 0)
                internalName += $" |{this.Order}|";
            if (this.IsIgnored)
                internalName += " |ignore|";
            if (!string.IsNullOrWhiteSpace(this.Category))
                internalName += $" |cat:{this.Category}|";
            if (this.ShouldAutomateIgnore)
                internalName += " |automate:ignore|";
            if (this.ShouldAutomatePreferForOutput)
                internalName += " |automate:output|";

            return internalName;
        }

        /// <summary>Whether the container has the default display name.</summary>
        public bool HasDefaultDisplayName()
        {
            return string.IsNullOrWhiteSpace(this.Name) || this.Name == this.DefaultDisplayName;
        }

        /// <summary>Get whether the container has any non-default data.</summary>
        public bool HasData()
        {
            return
                !this.HasDefaultDisplayName()
                || (this.Order.HasValue && this.Order != 0)
                || this.IsIgnored
                || !string.IsNullOrWhiteSpace(this.Category)
                || this.ShouldAutomateIgnore
                || this.ShouldAutomateIgnore;
        }
    }
}
