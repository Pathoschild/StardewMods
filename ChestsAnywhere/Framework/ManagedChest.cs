using System.Text.RegularExpressions;
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

        /// <summary>The name of the location or building which contains the chest.</summary>
        public string Location { get; }

        /// <summary>The chest's display name.</summary>
        public string Name { get; }

        /// <summary>Whether the chest should be ignored.</summary>
        public bool IsIgnored { get; }

        /// <summary>The player's preferred chest order (if any).</summary>
        public int? Order { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest instance.</param>
        /// <param name="location">The name of the location or building which contains the chest.</param>
        /// <param name="defaultName">The default name if it hasn't been customised.</param>
        public ManagedChest(Chest chest, string location, string defaultName)
        {
            // save values
            this.Chest = chest;
            this.Location = location;
            this.Name = chest.Name != "Chest"
                ? chest.Name
                : defaultName;

            // extract tags
            this.Name = Regex.Replace(this.Name, ManagedChest.TagGroupPattern, "").Trim();
            foreach (Match match in Regex.Matches(this.Name, ManagedChest.TagGroupPattern))
            {
                string[] tags = match.Groups[1].Value.Split(' ');
                foreach (string tag in tags)
                {
                    // ignore
                    if (tag.ToLower() == "ignore")
                    {
                        this.IsIgnored = true;
                        continue;
                    }

                    // order
                    int order;
                    if (int.TryParse(tag, out order))
                        this.Order = order;
                }
            }
        }
    }
}