using System;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands
{
    /// <summary>A console sub-command which provides command documentation.</summary>
    internal class GenericHelpCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>The human-readable name of the mod shown in the root command's help description.</summary>
        private readonly string ModName;

        /// <summary>The name of the root command.</summary>
        private readonly string RootName;

        /// <summary>Get the available console commands.</summary>
        private readonly Func<InvariantDictionary<ICommand>> GetCommands;

        /// <summary>The name of the help command.</summary>
        internal const string CommandName = "help";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rootName">The name of the root command.</param>
        /// <param name="modName">The human-readable name of the mod shown in the root command's help description.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="getCommands">The available console commands.</param>
        public GenericHelpCommand(string rootName, string modName, IMonitor monitor, Func<InvariantDictionary<ICommand>> getCommands)
            : base(monitor, GenericHelpCommand.CommandName)
        {
            this.ModName = modName;
            this.RootName = rootName;
            this.GetCommands = getCommands;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return $@"
                {this.RootName} {GenericHelpCommand.CommandName}
                   Usage: {this.RootName} {GenericHelpCommand.CommandName}
                   Lists all available {this.RootName} commands.

                   Usage: {this.RootName} {GenericHelpCommand.CommandName} <cmd>
                   Provides information for a specific {this.RootName} command.
                   - cmd: The {this.RootName} command name.
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
                    $"The '{this.RootName}' command is the entry point for {this.ModName} commands. You use it by specifying a more "
                    + $"specific command (like '{GenericHelpCommand.CommandName}' in '{this.RootName} {GenericHelpCommand.CommandName}'). Here are the available commands:\n\n"
                );
                foreach (var entry in commands.OrderBy(p => p.Key, HumanSortComparer.DefaultIgnoreCase))
                {
                    help.AppendLine(entry.Value.Description);
                    help.AppendLine();
                    help.AppendLine();
                }
            }
            else if (commands.TryGetValue(args[0], out ICommand command))
                help.AppendLine(command.Description);
            else
                help.AppendLine($"Unknown command '{this.RootName} {args[0]}'. Type '{this.RootName} {GenericHelpCommand.CommandName}' for available commands.");

            // write output
            this.Monitor.Log(help.ToString().Trim(), LogLevel.Info);
        }
    }
}
