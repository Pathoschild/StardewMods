using System;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework.Commands
{
    /// <summary>Handles the 'data-layers' console command.</summary>
    internal class CommandHandler : GenericCommandHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="currentLayer">Get the current data layer, if any.</param>
        public CommandHandler(IMonitor monitor, Func<ILayer> currentLayer)
            : base("data-layers", "Data Layers", CommandHandler.BuildCommands(monitor, currentLayer), monitor) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Build the available commands.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="currentLayer">Get the current data layer, if any.</param>
        private static ICommand[] BuildCommands(IMonitor monitor, Func<ILayer> currentLayer)
        {
            return new ICommand[]
            {
                new ExportCommand(monitor, currentLayer)
            };
        }
    }
}
