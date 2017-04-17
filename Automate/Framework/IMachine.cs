namespace Pathoschild.Stardew.Automate.Framework
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

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        bool Pull(IPipe[] pipes);
    }
}
