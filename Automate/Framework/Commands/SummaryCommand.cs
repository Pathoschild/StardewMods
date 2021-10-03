using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework.Commands
{
    /// <summary>A console command which prints a summary of automated machines.</summary>
    internal class SummaryCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>Manages machine groups.</summary>
        private readonly MachineManager MachineManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="machineManager">Manages machine groups.</param>
        public SummaryCommand(IMonitor monitor, ModConfig config, MachineManager machineManager)
            : base(monitor, "summary")
        {
            this.Config = config;
            this.MachineManager = machineManager;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                automate summary
                   Usage: automate summary
                   Prints a summary of automated machines.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            StringBuilder report = new StringBuilder();
            IMachineGroup[] machineGroups = this.MachineManager.GetActiveMachineGroups().ToArray();

            report.AppendLine("\n##########\n## Automate summary\n##########");

            // settings
            report.AppendLine("Settings:\n------------------------------");
            report.AppendLine($"   Automation interval: {this.Config.AutomationInterval}");
            if (this.Config.ConnectorNames.Any())
                report.AppendLine($"   Connectors: {string.Join(", ", from name in this.Config.ConnectorNames orderby name select $"'{name}'")}");

            // per-machine settings
            {
                StringBuilder perMachineReport = new StringBuilder();

                foreach (var config in this.MachineManager.GetMachineOverrides().OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
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
                report.AppendLine(Context.IsWorldReady
                    ? $"   Found {machineGroups.Length} machine groups in {machineGroups.Length} locations, containing {machineGroups.Sum(p => p.Machines.Length)} automated machines connected to {machineGroups.Sum(p => p.Containers.Length)} containers."
                    : "   No save loaded."
                );

                // custom machine factories
                IAutomationFactory[] customFactories = this.MachineManager.Factory.GetFactories().Where(p => p.GetType() != typeof(AutomationFactory)).ToArray();
                if (customFactories.Any())
                    report.AppendLine($"   Custom automation factories found: {string.Join(", ", customFactories.Select(p => p.GetType().FullName).OrderBy(p => p))}.");
            }
            report.AppendLine();
            report.AppendLine();

            // machine groups
            if (Context.IsWorldReady)
            {
                report.AppendLine("Automated machine groups:\n------------------------------");
                if (machineGroups.Any())
                {
                    IGrouping<string, IMachineGroup>[] groupsByLocation = machineGroups
                        .GroupBy(p => p.LocationKey)
                        .OrderByDescending(p => p.Key == null)
                        .ThenBy(p => p.Key)
                        .ToArray();

                    foreach (var locationGroups in groupsByLocation)
                    {
                        bool isJunimoGroup = locationGroups.Key == null;
                        string label = locationGroups.Key ?? "Machines connected to a Junimo chest";

                        report.AppendLine($"   {label}:");
                        foreach (IMachineGroup group in locationGroups)
                        {
                            var tile = group.Tiles[0];

                            report.AppendLine(isJunimoGroup
                                ? "      Distributed group:"
                                : $"      Group at ({tile.X}, {tile.Y}):"
                            );

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
