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

        /// <summary>Whether Automate should ignore this container.</summary>
        public bool ShouldAutomateIgnore { get; set; }

        /// <summary>Whether Automate should prefer this container for output.</summary>
        public bool ShouldAutomatePreferForOutput { get; set; }

        /// <summary>Whether Automate should allow getting items from this container.</summary>
        public bool ShouldAutomateNoInput { get; set; }

        /// <summary>Whether Automate should allow outputting items to this container.</summary>
        public bool ShouldAutomateNoOutput { get; set; }

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
                else if (tag.ToLower() == "automate:ignore")
                    data.ShouldAutomateIgnore = true;
                else if (tag.ToLower() == "automate:output")
                    data.ShouldAutomatePreferForOutput = true;
                else if (tag.ToLower() == "automate:noinput")
                    data.ShouldAutomateNoInput = true;
                else if (tag.ToLower() == "automate:nooutput")
                    data.ShouldAutomateNoOutput = true;
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
            string internalName = !this.HasDefaultDisplayName() ? this.Name : this.DefaultInternalName;
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
            if (this.ShouldAutomateNoInput)
                internalName += " |automate:noinput|";
            if (this.ShouldAutomateNoOutput)
                internalName += " |automate:nooutput|";

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
                || this.ShouldAutomateIgnore
                || this.ShouldAutomatePreferForOutput
                || this.ShouldAutomateNoInput
                || this.ShouldAutomateNoOutput;
        }

        /// <summary>Reset all container data to the default.</summary>
        public void Reset()
        {
            this.Name = this.DefaultInternalName;
            this.Order = null;
            this.IsIgnored = false;
            this.Category = null;
            this.ShouldAutomateIgnore = false;
            this.ShouldAutomatePreferForOutput = false;
            this.ShouldAutomateNoInput = false;
            this.ShouldAutomateNoOutput = false;
        }
    }
}
