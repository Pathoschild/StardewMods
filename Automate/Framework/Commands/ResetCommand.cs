using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework.Commands
{
    /// <summary>A console command which discards all cached machine data and triggers a full rescan.</summary>
    internal class ResetCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages machine groups.</summary>
        private readonly MachineManager MachineManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="machineManager">Manages machine groups.</param>
        public ResetCommand(IMonitor monitor, MachineManager machineManager)
            : base(monitor, "reset")
        {
            this.MachineManager = machineManager;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                automate rescan
                   Usage: automate reset
                   Resets all cached data and rescans the world for machines.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("You must load a save to use this command.", LogLevel.Error);
                return;
            }

            if (!Context.IsMainPlayer)
            {
                this.Monitor.Log("In multiplayer, automation is handled by the host player's Automate. Since you're not the host, this command has no effect for you.", LogLevel.Error);
                return;
            }

            this.MachineManager.Reset();
            this.Monitor.Log("Reset all cached data and queued a rescan of the world.", LogLevel.Info);
        }
    }
}
