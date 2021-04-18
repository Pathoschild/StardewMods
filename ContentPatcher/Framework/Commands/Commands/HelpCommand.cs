using System;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which provides command documentation.</summary>
    internal class HelpCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the available console commands.</summary>
        private readonly Func<InvariantDictionary<ICommand>> GetCommands;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="getCommands">The available console commands.</param>
        public HelpCommand(IMonitor monitor, Func<InvariantDictionary<ICommand>> getCommands)
            : base(monitor, "help")
        {
            this.GetCommands = getCommands;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch help
                   Usage: patch help
                   Lists all available patch commands.

                   Usage: patch help <cmd>
                   Provides information for a specific patch command.
                   - cmd: The patch command name.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            InvariantDictionary<ICommand> commands = this.GetCommands();

            // build output
            StringBuilder help = new();
            if (!args.Any())
            {
                help.AppendLine(
                    "The 'patch' command is the entry point for Content Patcher commands. These are "
                    + "intended for troubleshooting and aren't intended for players. You use it by specifying a more "
                    + "specific command (like 'help' in 'patch help'). Here are the available commands:\n\n"
                );
                foreach (var entry in commands.OrderByIgnoreCase(p => p.Key))
                {
                    help.AppendLine(entry.Value.Description);
                    help.AppendLine();
                    help.AppendLine();
                }
            }
            else if (commands.TryGetValue(args[0], out ICommand command))
                help.AppendLine(command.Description);
            else
                help.AppendLine($"Unknown command 'patch {args[0]}'. Type 'patch help' for available commands.");

            // write output
            this.Monitor.Log(help.ToString().Trim(), LogLevel.Info);
        }
    }
}
