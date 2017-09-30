using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Metadata about a factory group which can read and saved.</summary>
    internal class FactoryGroupData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tiles which are part of this group.</summary>
        public IList<Vector2> Tiles { get; set; }
    }
}
