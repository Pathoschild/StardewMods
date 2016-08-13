using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere.Framework
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

        /// <summary>The location which contains the chest.</summary>
        public GameLocation Location { get; }

        /// <summary>The chest's coordinates within the <see cref="Location"/>.</summary>
        public Vector2 Position { get; }

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
        /// <param name="location">The location which contains the chest.</param>
        /// <param name="position">The chest's coordinates within the <see cref="Location"/>.</param>
        public ManagedChest(Chest chest, GameLocation location, Vector2 position)
        {
            // save values
            this.Chest = chest;
            this.Location = location;
            this.Position = position;
            this.Name = this.Chest.Name != "Chest"
                ? this.Chest.Name
                : $"Chest({this.Position.X},{this.Position.Y})";

            // extract tags
            this.Name = Regex.Replace(this.Chest.Name, ManagedChest.TagGroupPattern, "").Trim();
            foreach (Match match in Regex.Matches(this.Chest.Name, ManagedChest.TagGroupPattern))
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