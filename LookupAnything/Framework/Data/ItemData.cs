using System.Collections.Generic;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Provides override metadata about a game item.</summary>
    internal record ItemData
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Identify object
        ****/
        /// <summary>The context in which to override the object.</summary>
        public ObjectContext Context { get; set; } = ObjectContext.Any;

        /// <summary>The qualified item IDs for this object.</summary>
        public HashSet<string> QualifiedId { get; set; } = new();

        /****
        ** Overrides
        ****/
        /// <summary>The translation key which should override the item name (if any).</summary>
        public string? NameKey { get; set; }

        /// <summary>The translation key which should override the item description (if any).</summary>
        public string? DescriptionKey { get; set; }

        /// <summary>The translation key which should override the item type name (if any).</summary>
        public string? TypeKey { get; set; }

        /// <summary>Whether the player can pick up this item.</summary>
        public bool? ShowInventoryFields { get; set; }
    }
}
