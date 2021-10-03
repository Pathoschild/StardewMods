using System;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Automate.Framework.Commands.Summary;
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
            GlobalStats stats = new GlobalStats(this.MachineManager.GetActiveMachineGroups());
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
                    ? $"   Found {stats.MachineGroupCount} machine groups in {stats.Locations.Length} locations, containing {stats.MachineCount} automated machines connected to {stats.ContainerCount} containers."
                    : "   No save loaded."
                );

                // custom machine factories
                IAutomationFactory[] customFactories = this.MachineManager.Factory.GetFactories().Where(p => p.GetType() != typeof(AutomationFactory)).ToArray();
                if (customFactories.Any())
                    report.AppendLine($"   Custom automation factories found: {string.Join(", ", customFactories.Select(p => p.GetType().FullName).OrderBy(p => p))}.");

                // warnings
                if (this.AnyMachineGroupsBlockedByFullChests(stats))
                {
                    report.AppendLine();
                    report.AppendLine("NOTE: some machine groups have output ready but all their chests are full.");
                    report.AppendLine("That may cause lag if many machines are blocked.");
                }
            }
            report.AppendLine();
            report.AppendLine();

            // machine groups
            if (Context.IsWorldReady)
            {
                report.AppendLine("Automated machine groups:\n------------------------------");
                if (stats.Locations.Any())
                {
                    foreach (LocationStats location in stats.Locations)
                    {
                        report.AppendLine($"   {location.Name}:");
                        foreach (GroupStats group in location.MachineGroups)
                        {
                            // show group name
                            report.AppendLine($"      {group.Name}:");

                            // list machines
                            foreach (GroupMachineStats machineTypeGroup in group.Machines)
                            {
                                report.Append($"          {machineTypeGroup.Count} x {machineTypeGroup.Name} (");
                                report.Append(machineTypeGroup.States.Count == 1
                                    ? $"{machineTypeGroup.States.First().Key.ToString().ToLower()}" // (done)
                                    : string.Join(" > ", machineTypeGroup.States.OrderBy(p => p.Key).Select(p => $"{p.Value} {p.Key.ToString().ToLower()}")) // (XX empty > XX processing > XX done)
                                );
                                report.AppendLine(")");
                            }

                            // list containers
                            foreach (var containerGroup in group.Containers)
                                report.AppendLine($"          {containerGroup.Count} x {containerGroup.Name} ({containerGroup.FilledSlots}/{containerGroup.TotalSlots} full)");

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

        /// <summary>Get whether any machine groups may be blocked by a full chest.</summary>
        /// <param name="stats">The global stats to check.</param>
        private bool AnyMachineGroupsBlockedByFullChests(GlobalStats stats)
        {
            foreach (GroupStats group in stats.Locations.SelectMany(p => p.MachineGroups))
            {
                bool chestsFull = group.Containers.Sum(p => p.FilledSlots) >= group.Containers.Sum(p => p.TotalSlots);
                if (!chestsFull)
                    continue;

                bool hasOutputReady = group.Machines.Any(p => p.States.ContainsKey(MachineState.Done));
                if (hasOutputReady)
                    return true;
            }

            return false;
        }
    }
}
