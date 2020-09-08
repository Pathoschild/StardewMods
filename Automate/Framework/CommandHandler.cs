using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Automate.Framework.Models;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Handles console commands from players.</summary>
    internal class CommandHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Writes messages to the console.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>Constructs machine groups.</summary>
        private readonly MachineGroupFactory Factory;

        /// <summary>The machines to process.</summary>
        private readonly IDictionary<GameLocation, MachineGroup[]> ActiveMachineGroups;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Writes messages to the console.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="factory">Constructs machine groups.</param>
        /// <param name="activeMachineGroups">The machines to process.</param>
        public CommandHandler(IMonitor monitor, ModConfig config, MachineGroupFactory factory, IDictionary<GameLocation, MachineGroup[]> activeMachineGroups)
        {
            this.Monitor = monitor;
            this.Config = config;
            this.Factory = factory;
            this.ActiveMachineGroups = activeMachineGroups;
        }

        /// <summary>Handle a console command.</summary>
        /// <param name="command">The command name entered by the player.</param>
        /// <param name="args">The command arguments.</param>
        public void HandleCommand(string command, string[] args)
        {
            switch (args.FirstOrDefault())
            {
                case "summary":
                    this.HandleSummary();
                    break;

                default:
                    this.HandleHelp();
                    break;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Handle the 'automate help' command.</summary>
        private void HandleHelp()
        {
            this.Monitor.Log("Supported commands:\n- 'automate summary': get a summary of current automated machines.", LogLevel.Info);
        }

        /// <summary>Handle the 'automate summary' command.</summary>
        private void HandleSummary()
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("\n##########\n## Automate summary\n##########");

            // settings
            report.AppendLine("Settings:\n------------------------------");
            report.AppendLine($"   Automation interval: {this.Config.AutomationInterval}");
            if (this.Config.ConnectorNames.Any())
                report.AppendLine($"   Connectors: {string.Join(", ", from name in this.Config.ConnectorNames orderby name select $"'{name}'")}");

            // per-machine settings
            {
                StringBuilder perMachineReport = new StringBuilder();

                foreach (var config in this.Config.MachineOverrides.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                {
                    string[] customSettings = config.Value
                        .GetCustomSettings()
                        .OrderBy(p => p.Key)
                        .Select(p => $"{p.Key}={p.Value}")
                        .ToArray();

                    if (customSettings.Any())
                        perMachineReport.AppendLine($"      {config.Key}: {string.Join(", ", customSettings)}");
                }

                if (perMachineReport.Length > 0)
                {
                    report.AppendLine("   Per-machine settings:");
                    report.Append(perMachineReport);
                }
            }
            report.AppendLine();
            report.AppendLine();

            // machine summary
            report.AppendLine("Summary:\n------------------------------");
            {
                // machine groups
                if (Context.IsWorldReady)
                {
                    MachineGroup[] allGroups = this.ActiveMachineGroups.SelectMany(p => p.Value).ToArray();
                    report.AppendLine($"   Found {allGroups.Length} machine groups in {this.ActiveMachineGroups.Count} locations, containing {allGroups.Sum(p => p.Machines.Length)} automated machines connected to {allGroups.Sum(p => p.Containers.Length)} containers.");
                }
                else
                    report.AppendLine("   No save loaded.");

                // custom machine factories
                IAutomationFactory[] customFactories = this.Factory.GetFactories().Where(p => p.GetType() != typeof(AutomationFactory)).ToArray();
                if (customFactories.Any())
                    report.AppendLine($"   Custom automation factories found: {string.Join(", ", customFactories.Select(p => p.GetType().FullName).OrderBy(p => p))}.");
            }
            report.AppendLine();
            report.AppendLine();

            // machine groups
            if (Context.IsWorldReady)
            {
                report.AppendLine("Automated machine groups:\n------------------------------");
                if (this.ActiveMachineGroups.Any())
                {
                    foreach (GameLocation location in this.ActiveMachineGroups.Keys.OrderBy(p => $"{p.Name} ({p.NameOrUniqueName})", StringComparer.OrdinalIgnoreCase))
                    {
                        MachineGroup[] machineGroups = this.ActiveMachineGroups[location];

                        report.AppendLine($"   {location.Name}{(location.NameOrUniqueName != location.Name ? $" ({location.NameOrUniqueName})" : "")}:");

                        foreach (MachineGroup group in machineGroups)
                        {
                            var tile = group.Tiles[0];

                            report.AppendLine($"      Group at ({tile.X}, {tile.Y}):");

                            foreach (KeyValuePair<string, int> pair in group.Machines.GroupBy(p => p.MachineTypeID).ToDictionary(p => p.Key, p => p.Count()).OrderByDescending(p => p.Value))
                                report.AppendLine($"          {pair.Value} x {pair.Key}");

                            foreach (KeyValuePair<string, int> pair in group.Containers.GroupBy(p => p.Name).ToDictionary(p => p.Key, p => p.Count()).OrderByDescending(p => p.Value))
                                report.AppendLine($"          {pair.Value} x {pair.Key}");
                            report.AppendLine();
                        }

                        report.AppendLine();
                    }
                }
                else
                    report.AppendLine("   No machines are currently automated.");
            }

            this.Monitor.Log(report.ToString(), LogLevel.Info);
        }
    }
}
