using System.Linq;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework
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
                    this.Monitor.Log($"The '{args[0]}' subcommand isn't valid. Type '{this.CommandName} help' for a list of valid subcommands.");
                    return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Handle the 'patch help' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleHelp(string[] args)
        {
            // generate command info
            var helpEntries = new InvariantDictionary<string>
            {
                ["help"] = $"{this.CommandName} help\n   Usage: {this.CommandName} help\n   Lists all available {this.CommandName} subcommands.\n\n   Usage: {this.CommandName} help <cmd>\n   Provides information for a specific {this.CommandName} subcommand.\n   - cmd: The {this.CommandName} subcommand name.",
                ["summary"] = $"{this.CommandName} summary\n   Usage: {this.CommandName} summary\n   Shows a summary of the current conditions and loaded patches."
            };

            // build output
            StringBuilder help = new StringBuilder();
            if (!args.Any())
            {
                help.AppendLine(
                    $"The '{this.CommandName}' command is the entry point for Content Patcher subcommands. These are "
                    + "intended for troubleshooting and aren't intended for players. You use it by specifying a more "
                    + $"specific subcommand (like 'help' in '{this.CommandName} help'). Here are the available subcommands:\n\n"
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
                help.AppendLine($"Unknown subcommand '{args[0]}'. Type '{this.CommandName} help' for available subcommands.");

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
            var patches = this.PatchManager
                .GetAll()
                .GroupBy(p => p.ContentPack.Manifest.Name)
                .OrderBy(p => p.Key);

            output.AppendLine("Patches by content pack ([X] means applied):");
            foreach (IGrouping<string, IPatch> patchGroup in patches)
            {
                output.AppendLine($"   {patchGroup.Key}:");
                foreach (IPatch patch in patchGroup.OrderBy(p => p.LogName))
                {
                    // log 'applied' checkbox
                    output.Append($"      [{(patch.MatchesContext ? "X" : " ")}] ");

                    // log patch name
                    if (patch.LogName.StartsWith(patchGroup.Key + " > "))
                        output.Append($"{patch.LogName.Substring((patchGroup.Key + " > ").Length)}");
                    else
                        output.Append($"{patch.LogName}");

                    // log target (if not in name)
                    if (!patch.LogName.Contains($"{patch.Type} {patch.TokenableAssetName.Raw}"))
                        output.Append($" | {patch.Type} {patch.TokenableAssetName.Raw}");

                    // log target if tokenised
                    if (patch.MatchesContext && patch.TokenableAssetName.ConditionTokens.Any())
                        output.Append($" | => {patch.AssetName}");

                    // log conditions
                    if (!patch.MatchesContext)
                    {
                        string[] failedConditions = (
                            from condition in patch.Conditions.Values
                            orderby condition.Key.ToString()
                            where !condition.IsMatch(context)
                            select $"{condition.Key} ({string.Join(", ", condition.Values)})"
                        ).ToArray();

                        output.Append(failedConditions.Any()
                            ? $" | failed conditions: {string.Join(", ", failedConditions)}"
                            : " | disabled (reason unknown)."
                        );
                    }
                    output.AppendLine(); // end line
                }
                output.AppendLine(); // blank line between groups
            }

            this.Monitor.Log(output.ToString());
            return true;
        }
    }
}
