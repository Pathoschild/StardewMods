using System.Linq;
using System.Text;
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


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the root command.</summary>
        public string CommandName { get; } = "patch";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor"></param>
        public CommandHandler(IMonitor monitor)
        {
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
                ["help"] = $"{this.CommandName} help\n   Usage: {this.CommandName} help\n   Lists all available {this.CommandName} subcommands.\n\n   Usage: {this.CommandName} help <cmd>\n   Provides information for a specific {this.CommandName} subcommand.\n   - cmd: The {this.CommandName} subcommand name."
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


    }
}
