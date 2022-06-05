using System;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework.Commands
{
    /// <summary>Handles console commands from players.</summary>
    internal class CommandHandler : GenericCommandHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Writes messages to the console.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="machineManager">Manages machine groups.</param>
        public CommandHandler(IMonitor monitor, Func<ModConfig> config, MachineManager machineManager)
            : base("automate", "Automate", CommandHandler.BuildCommands(monitor, config, machineManager), monitor) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Build the available commands.</summary>
        /// <param name="monitor">Writes messages to the console.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="machineManager">Manages machine groups.</param>
        private static ICommand[] BuildCommands(IMonitor monitor, Func<ModConfig> config, MachineManager machineManager)
        {
            return new ICommand[]
            {
                new ResetCommand(monitor, machineManager),
                new SummaryCommand(monitor, config, machineManager)
            };
        }
    }
}
