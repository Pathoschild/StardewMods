using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Wraps a machine instance to simplify patching.</summary>
    internal class MachineWrapper : IMachine
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The wrapped machine instance.</summary>
        public IMachine Machine { get; }

        /// <summary>The location which contains the machine.</summary>
        public GameLocation Location => this.Machine.Location;

        /// <summary>The tile area covered by the machine.</summary>
        public Rectangle TileArea => this.Machine.TileArea;

        /// <summary>A unique ID for the machine type.</summary>
        /// <remarks>This value should be identical for two machines if they have the exact same behavior and input logic. For example, if one machine in a group can't process input due to missing items, Automate will skip any other empty machines of that type in the same group since it assumes they need the same inputs.</remarks>
        public string MachineTypeID => this.Machine.MachineTypeID;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The wrapped machine instance.</param>
        public MachineWrapper(IMachine machine)
        {
            this.Machine = machine ?? throw new ArgumentNullException(nameof(machine));
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            return this.Machine.GetState();
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            return this.Machine.GetOutput();
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            return this.Machine.SetInput(input);
        }
    }
}
