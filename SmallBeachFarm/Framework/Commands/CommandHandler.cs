using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework.Commands
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
        public CommandHandler(IMonitor monitor, ModConfig config)
            : base("small_beach_farm", "Small Beach Farm", CommandHandler.BuildCommands(monitor, config), monitor) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Build the available commands.</summary>
        /// <param name="monitor">Writes messages to the console.</param>
        /// <param name="config">The mod configuration.</param>
        private static ICommand[] BuildCommands(IMonitor monitor, ModConfig config)
        {
            return new ICommand[]
            {
                new SetFarmTypeCommand(monitor, config)
            };
        }
    }
}
