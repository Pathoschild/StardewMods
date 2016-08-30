using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Metadata
{
    /// <summary>Provides override metadata that's not available from the game data directly.</summary>
    public class OverrideData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Additional metadata about 'big craftable' objects (including furniture, crafting stations, scarecrows, etc).</summary>
        public IDictionary<int, ObjectOverride> BigCraftables { get; set; }

        /// <summary>Additional metadata about game items (including inventory items, terrain features, crops, trees, and other map objects).</summary>
        public IDictionary<int, ObjectOverride> Objects { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get overrides for an object.</summary>
        /// <param name="item">The item for which to get overrides.</param>
        public ObjectOverride GetOverrides(Item item)
        {
            // big craftable
            if ((item as Object)?.bigCraftable == true)
            {
                return this.Objects.ContainsKey(item.parentSheetIndex)
                    ? this.Objects[item.parentSheetIndex]
                    : null;
            }

            // object
            return this.Objects.ContainsKey(item.parentSheetIndex)
                ? this.Objects[item.parentSheetIndex]
                : null;
        }
    }
}
