namespace Pathoschild.LookupAnything.Framework.Data
{
    /// <summary>Provides override metadata about a game item.</summary>
    public class ObjectData
    {
        /// <summary>The item name (if overridden).</summary>
        public string Name { get; set; }

        /// <summary>The item description (if overridden).</summary>
        public string Description { get; set; }

        /// <summary>The item type name.</summary>
        public string Type { get; set; }

        /// <summary>Whether the player can pick up this item.</summary>
        public bool? ShowInventoryFields { get; set; }
    }
}