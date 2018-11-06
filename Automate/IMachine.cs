namespace Pathoschild.Stardew.Automate
{
    /// <summary>A machine that accepts input and provides output.</summary>
    internal interface IMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        MachineState GetState();

        /// <summary>Get the output item.</summary>
        ITrackedStack GetOutput();

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        bool SetInput(IStorage input);
    }
}
