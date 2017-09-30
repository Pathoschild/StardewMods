using System.Collections.Generic;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>Persistent player data.</summary>
    internal class SaveData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The machine/storage groups created by the player.</summary>
        public HashSet<GroupData> Groups { get; set; } = new HashSet<GroupData>();
    }
}
