using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The API which lets other mods interact with Automate.</summary>
    public class AutomateAPI : IAutomateAPI
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages machine groups.</summary>
        private readonly MachineManager MachineManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="machineManager">Manages machine groups.</param>
        internal AutomateAPI(IMonitor monitor, MachineManager machineManager)
        {
            this.Monitor = monitor;
            this.MachineManager = machineManager;
        }

        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        public void AddFactory(IAutomationFactory factory)
        {
            this.Monitor.Log($"Adding automation factory: {factory.GetType().AssemblyQualifiedName}", LogLevel.Trace);
            this.MachineManager.Factory.Add(factory);
        }

        /// <summary>Get the status of machines in a tile area. This is a specialized API for Data Layers and similar mods.</summary>
        /// <param name="location">The location for which to display data.</param>
        /// <param name="tileArea">The tile area for which to display data.</param>
        public IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea)
        {
            IDictionary<Vector2, int> data = new Dictionary<Vector2, int>();
            foreach (IMachine machine in this.MachineManager.GetForApi(location).SelectMany(group => group.Machines))
            {
                if (machine.TileArea.Intersects(tileArea))
                {
                    int state = (int)machine.GetState();
                    foreach (Vector2 tile in machine.TileArea.GetTiles())
                    {
                        if (tileArea.Contains((int)tile.X, (int)tile.Y))
                            data[tile] = state;
                    }
                }
            }

            return data;
        }
    }
}
