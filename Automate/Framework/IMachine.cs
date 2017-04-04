using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A machine that accepts input and provides output.</summary>
    public interface IMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        MachineState GetState();

        /// <summary>Get the output item.</summary>
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        Item GetOutput();

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        void Reset(bool outputTaken);

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        bool Pull(Chest[] chests);
    }
}
