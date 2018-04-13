using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The public API provided to other mods.</summary>
    public interface IAutomateAPI
    {
        /// <summary>Registers a delegate to be called to check if there is a machine at a given tile.</summary>
        /// <param name="hook">The delegate to call.</param>
        void RegisterGetMachineHook(GetMachineHook hook);

        /// <summary>Registers a delegate to be called to check if there is a container at a given tile.</summary>
        /// <param name="hook">The delegate to call.</param>
        void RegisterGetContainerHook(GetContainerHook hook);

        /// <summary>An event that is dispatched when the machines are reloaded for the given location.</summary>
        event EventHandler<EventArgsLocationMachinesChanged> LocationMachinesChanged;
    }

    /// <summary>A delegate to be called to check if there is a machine at a given tile.</summary>
    public delegate IMachine GetMachineHook(GameLocation location, Vector2 tile, out Vector2 size);

    /// <summary>A delegate to be called to check if there is a container at a given tile.</summary>
    public delegate IContainer GetContainerHook(GameLocation location, Vector2 tile, out Vector2 size);

    /// <summary>The event args to LocationMachinesChanged.</summary>
    public class EventArgsLocationMachinesChanged : EventArgs
    {
        /// <summary>The location where machines were changed.</summary>
        public GameLocation Location { get; }

        /// <summary>The set of machine groups at this location.</summary>
        public MachineGroup[] MachineGroups { get; }

        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location where machines were changed.</param>
        /// <param name="machineGroups">The set of machine groups at this location.</param>
        public EventArgsLocationMachinesChanged(GameLocation location, MachineGroup[] machineGroups)
        {
            this.Location = location;
            this.MachineGroups = machineGroups;
        }
    }
}
