using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands
{
    /// <summary>Handles a root console command with any number of sub-commands.</summary>
    /// <remarks>For example, Content Patcher has a single 'patch' command with sub-commands like 'patch parse', 'patch export', etc.</remarks>
    internal class GenericCommandHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The console commands recognized by the mod through the root command, indexed by name.</summary>
        private readonly InvariantDictionary<ICommand> Commands;


        /*********
        ** Accessors
        *********/
        /// <summary>The human-readable name of the mod shown in the root command's help description.</summary>
        public string ModName { get; }

        /// <summary>The name of the root command.</summary>
        public string RootName { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rootName">The name of the root command.</param>
        /// <param name="modName">The human-readable name of the mod shown in the root command's help description.</param>
        /// <param name="commands">The console commands recognized by the mod through the root command.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public GenericCommandHandler(string rootName, string modName, IEnumerable<ICommand> commands, IMonitor monitor)
        {
            this.RootName = rootName;
            this.ModName = modName;
            this.Monitor = monitor;

            this.Commands = new InvariantDictionary<ICommand>(commands.ToDictionary(p => p.Name));
            this.Commands[GenericHelpCommand.CommandName] = new GenericHelpCommand(rootName, modName, monitor, () => this.Commands);
        }

        /// <summary>Handle a console command.</summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        public bool Handle(string[] args)
        {
            string commandName = args.FirstOrDefault() ?? GenericHelpCommand.CommandName;
            string[] commandArgs = args.Skip(1).ToArray();

            if (this.Commands.TryGetValue(commandName, out ICommand command))
            {
                command.Handle(commandArgs);
                return true;
            }
            else
            {
                this.Monitor.Log($"The '{this.RootName} {args[0]}' command isn't valid. Type '{this.RootName} {GenericHelpCommand.CommandName}' for a list of valid commands.", LogLevel.Error);
                return false;
            }
        }

        /// <summary>Register the root command with SMAPI.</summary>
        /// <param name="commandHelper">SMAPI's command API.</param>
        public void RegisterWith(ICommandHelper commandHelper)
        {
            commandHelper.Add(this.RootName, $"Starts a {this.ModName} command. Type '{this.RootName} {GenericHelpCommand.CommandName}' for details.", (_, args) => this.Handle(args));
        }
    }
}
