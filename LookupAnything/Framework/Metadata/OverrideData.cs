using System.Collections.Generic;

namespace Pathoschild.LookupAnything.Framework.Metadata
{
    /// <summary>Provides override metadata that's not available from the game data directly.</summary>
    public class OverrideData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Additional metadata about game items (including inventory items, terrain features, crops, trees, and other map objects).</summary>
        public IDictionary<int, ObjectOverride> Objects { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get overrides for an object.</summary>
        /// <param name="spriteIndex">The object's sprite index.</param>
        public ObjectOverride GetObject(int spriteIndex)
        {
            return this.Objects.ContainsKey(spriteIndex)
                ? this.Objects[spriteIndex]
                : null;
        }
    }
}
