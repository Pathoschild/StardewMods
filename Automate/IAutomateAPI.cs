namespace Pathoschild.Stardew.Automate
{
    /// <summary>The API which lets other mods interact with Automate.</summary>
    public interface IAutomateAPI
    {
        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        void AddFactory(IAutomationFactory factory);
    }
}
