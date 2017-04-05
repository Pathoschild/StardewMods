using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A machine instance with metadata.</summary>
    public class MachineMetadata
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The machine instance.</summary>
        public IMachine Machine { get; }

        /// <summary>The location containing the machine.</summary>
        public GameLocation Location { get; }

        /// <summary>The chests connected to the machine.</summary>
        public Chest[] Connected { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="connected">The chests connected to the machine.</param>
        /// <param name="machine">The machine instance.</param>
        public MachineMetadata(GameLocation location, IEnumerable<Chest> connected, IMachine machine)
        {
            this.Location = location;
            this.Machine = machine;
            this.Connected = connected.ToArray();
        }
    }
}
