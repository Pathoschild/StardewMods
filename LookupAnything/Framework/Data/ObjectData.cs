namespace Pathoschild.LookupAnything.Framework.Data
{
    /// <summary>Provides override metadata about a game item.</summary>
    internal class ObjectData
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Identify object
        ****/
        /// <summary>The context in which to override the object.</summary>
        public ObjectContext Context { get; set; } = ObjectContext.Any;

        /// <summary>The sprite sheet used to draw the object. A given sprite ID can be duplicated between two sprite sheets.</summary>
        public ObjectSpriteSheet SpriteSheet { get; set; } = ObjectSpriteSheet.Object;

        /// <summary>The sprite IDs for this object.</summary>
        public int[] SpriteID { get; set; }

        /****
        ** Overrides
        ****/
        /// <summary>The overridden item name (if any).</summary>
        public string Name { get; set; }

        /// <summary>The overridden item description (if any).</summary>
        public string Description { get; set; }

        /// <summary>The overridden item type name (if any).</summary>
        public string Type { get; set; }

        /// <summary>Whether the player can pick up this item.</summary>
        public bool? ShowInventoryFields { get; set; }
    }
}