using Pathoschild.Stardew.Common.Items.ItemData;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
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

        /// <summary>The item types to disambiguate IDs which can be duplicated between two sprite sheets.</summary>
        public ItemType Type { get; set; } = ItemType.Object;

        /// <summary>The sprite IDs for this object.</summary>
        public int[] SpriteID { get; set; }

        /****
        ** Overrides
        ****/
        /// <summary>The translation key which should override the item name (if any).</summary>
        public string NameKey { get; set; }

        /// <summary>The translation key which should override the item description (if any).</summary>
        public string DescriptionKey { get; set; }

        /// <summary>The translation key which should override the item type name (if any).</summary>
        public string TypeKey { get; set; }

        /// <summary>Whether the player can pick up this item.</summary>
        public bool? ShowInventoryFields { get; set; }
    }
}
