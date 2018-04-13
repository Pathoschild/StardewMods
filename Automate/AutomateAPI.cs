using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    public class AutomateAPI : IAutomateAPI
    {
        /// <summary>A list of delegates to call to get a machine at a particular tile.</summary>
        internal static List<GetMachineHook> GetMachineHooks = new List<GetMachineHook>();

        /// <summary>A list of delegates to call to get a container at a particular tile.</summary>
        internal static List<GetContainerHook> GetContainerHooks = new List<GetContainerHook>();

        /// <summary>A lock that synchronizes access to our event handlers.</summary>
        private static object eventLock = new object();

        /// <summary>An event that is dispatched when the machines are reloaded for the given location.</summary>
        private static event EventHandler<EventArgsLocationMachinesChanged> _LocationMachinesChanged;

        /// <summary>Registers a delegate to be called to check if there is a machine at a given tile.</summary>
        /// <param name="hook">The delegate to call.</param>
        public void RegisterGetMachineHook(GetMachineHook hook)
        {
            GetMachineHooks.Add(hook);
        }

        /// <summary>Registers a delegate to be called to check if there is a container at a given tile.</summary>
        /// <param name="hook">The delegate to call.</param>
        public void RegisterGetContainerHook(GetContainerHook hook)
        {
            GetContainerHooks.Add(hook);
        }

        /// <summary>An event that is dispatched when the machines are reloaded for the given location.</summary>
        public event EventHandler<EventArgsLocationMachinesChanged> LocationMachinesChanged
        {
            add
            {
                lock (eventLock)
                {
                    _LocationMachinesChanged += value;
                }
            }
            remove
            {
                lock (eventLock)
                {
                    _LocationMachinesChanged -= value;
                }
            }
        }

        /// <summary>Called when the machines are reloaded for the given location.</summary>
        internal static void OnLocationMachinesChanged(object sender, GameLocation location, MachineGroup[] machineGroups)
        {
            if (_LocationMachinesChanged != null)
                _LocationMachinesChanged(sender, new EventArgsLocationMachinesChanged(location, machineGroups));
        }
    }
}
