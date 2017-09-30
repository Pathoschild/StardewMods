using System.Collections.Generic;

namespace Pathoschild.Stardew.Automate.Models
{
    /// <summary>Persistent player data.</summary>
    internal class SaveData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The factory groups created by the player.</summary>
        public HashSet<FactoryGroupData> Factories { get; set; } = new HashSet<FactoryGroupData>();
    }
}
