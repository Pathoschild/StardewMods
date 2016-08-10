using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    /// <summary>A chest with metadata.</summary>
    public class ManagedChest
    {
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
        public bool IsIgnored => this.Name.Contains("ignore");


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest instance.</param>
        /// <param name="location">The location which contains the chest.</param>
        /// <param name="position">The chest's coordinates within the <see cref="Location"/>.</param>
        public ManagedChest(Chest chest, GameLocation location, Vector2 position)
        {
            this.Chest = chest;
            this.Location = location;
            this.Position = position;
            this.Name = this.Chest.Name != "Chest"
                ? this.Chest.Name
                : $"Chest({this.Position.X},{this.Position.Y})";
        }
    }
}