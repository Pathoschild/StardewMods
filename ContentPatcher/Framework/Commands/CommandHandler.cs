using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>Handles the 'patch' console command.</summary>
    internal class CommandHandler
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages loaded patches.</summary>
        private readonly PatchManager PatchManager;

        /// <summary>Handles constructing, permuting, and updating conditions.</summary>
        private readonly ConditionFactory ConditionFactory;


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the root command.</summary>
        public string CommandName { get; } = "patch";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="patchManager">Manages loaded patches.</param>
        /// <param name="conditionFactory">Handles constructing, permuting, and updating conditions.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public CommandHandler(PatchManager patchManager, ConditionFactory conditionFactory, IMonitor monitor)
        {
            this.PatchManager = patchManager;
            this.ConditionFactory = conditionFactory;
            this.Monitor = monitor;
        }

        /// <summary>Handle a console command.</summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        public bool Handle(string[] args)
        {
            string subcommand = args.FirstOrDefault();
            string[] subcommandArgs = args.Skip(1).ToArray();

            switch (subcommand?.ToLower())
            {
                case null:
                case "help":
                    return this.HandleHelp(subcommandArgs);

                case "summary":
                    return this.HandleSummary(subcommandArgs);

                default:
                    this.Monitor.Log($"The '{this.CommandName} {args[0]}' command isn't valid. Type '{this.CommandName} help' for a list of valid commands.");
                    return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Commands
        ****/
        /// <summary>Handle the 'patch help' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleHelp(string[] args)
        {
            // generate command info
            var helpEntries = new InvariantDictionary<string>
            {
                ["help"] = $"{this.CommandName} help\n   Usage: {this.CommandName} help\n   Lists all available {this.CommandName} commands.\n\n   Usage: {this.CommandName} help <cmd>\n   Provides information for a specific {this.CommandName} command.\n   - cmd: The {this.CommandName} command name.",
                ["summary"] = $"{this.CommandName} summary\n   Usage: {this.CommandName} summary\n   Shows a summary of the current conditions and loaded patches."
            };

            // build output
            StringBuilder help = new StringBuilder();
            if (!args.Any())
            {
                help.AppendLine(
                    $"The '{this.CommandName}' command is the entry point for Content Patcher commands. These are "
                    + "intended for troubleshooting and aren't intended for players. You use it by specifying a more "
                    + $"specific command (like 'help' in '{this.CommandName} help'). Here are the available commands:\n\n"
                );
                foreach (var entry in helpEntries.OrderBy(p => p.Key))
                {
                    help.AppendLine(entry.Value);
                    help.AppendLine();
                }
            }
            else if (helpEntries.TryGetValue(args[0], out string entry))
                help.AppendLine(entry);
            else
                help.AppendLine($"Unknown command '{this.CommandName} {args[0]}'. Type '{this.CommandName} help' for available commands.");

            // write output
            this.Monitor.Log(help.ToString());

            return true;
        }

        /// <summary>Handle the 'patch summary' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleSummary(string[] args)
        {
            ConditionContext context = this.PatchManager.ConditionContext;
            StringBuilder output = new StringBuilder();

            // add condition summary
            output.AppendLine();
            output.AppendLine("Current conditions:");
            foreach (ConditionKey key in this.ConditionFactory.GetValidConditions())
                output.AppendLine($"   {key}: {context.GetValue(key)}");
            output.AppendLine();

            // add patch summary
            var patches = this.GetAllPatches()
                .GroupBy(p => p.ContentPack.Manifest.Name)
                .OrderBy(p => p.Key);

            output.AppendLine("Patches by content pack ([X] means applied):");
            foreach (IGrouping<string, PatchInfo> patchGroup in patches)
            {
                output.AppendLine($"   {patchGroup.Key}:");
                foreach (PatchInfo patch in patchGroup.OrderBy(p => p.ShortName))
                {
                    // log checkbox and patch name
                    output.Append($"      [{(patch.IsApplied ? "X" : " ")}] {patch.ShortName}");

                    // log raw target (if not in name)
                    if (!patch.ShortName.Contains($"{patch.Type} {patch.RawAssetName}"))
                        output.Append($" | {patch.Type} {patch.RawAssetName}");

                    // log parsed target if tokenised
                    if (patch.IsApplied && patch.ParsedAssetName != null && patch.ParsedAssetName.ConditionTokens.Any())
                        output.Append($" | => {patch.ParsedAssetName.Value}");

                    // log reason not applied
                    if (!patch.IsLoaded)
                        output.Append($" | not loaded: {patch.ReasonDisabled}");
                    else if (!patch.IsApplied && patch.ParsedConditions != null)
                    {
                        string[] failedConditions = (
                            from condition in patch.ParsedConditions.Values
                            orderby condition.Key.ToString()
                            where !condition.IsMatch(context)
                            select $"{condition.Key} ({string.Join(", ", condition.Values)})"
                        ).ToArray();

                        output.Append(failedConditions.Any()
                            ? $" | failed conditions: {string.Join(", ", failedConditions)}"
                            : " | disabled (reason unknown)"
                        );
                    }
                    else if (!patch.IsApplied)
                        output.Append(" | disabled (reason unknown)");

                    // end line
                    output.AppendLine();
                }
                output.AppendLine(); // blank line between groups
            }

            this.Monitor.Log(output.ToString());
            return true;
        }

        /****
        ** Helpers
        ****/
        /// <summary>Get basic info about all patches, including those which couldn't be loaded.</summary>
        public IEnumerable<PatchInfo> GetAllPatches()
        {
            foreach (IPatch patch in this.PatchManager.GetPatches())
                yield return new PatchInfo(patch);
            foreach (DisabledPatch patch in this.PatchManager.GetPermanentlyDisabledPatches())
                yield return new PatchInfo(patch);
        }
    }
}
