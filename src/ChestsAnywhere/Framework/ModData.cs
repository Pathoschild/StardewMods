using System.Collections.Generic;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The model for the <c>data.json</c> file.</summary>
    internal class ModData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The predefined world areas for <see cref="ChestRange.CurrentWorldArea"/>.</summary>
        public IDictionary<string, HashSet<string>> WorldAreas { get; set; } = new Dictionary<string, HashSet<string>>();
    }
}
