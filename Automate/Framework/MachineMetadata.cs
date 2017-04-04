using Microsoft.Xna.Framework;
using StardewValley;

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

        /// <summary>The machine's position in its location.</summary>
        public Vector2 Position { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="position">The machine's position in its location.</param>
        /// <param name="machine">The machine instance.</param>
        public MachineMetadata(GameLocation location, Vector2 position, IMachine machine)
        {
            this.Location = location;
            this.Machine = machine;
            this.Position = position;
        }
    }
}
