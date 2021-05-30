using System;
using System.Linq;
using ContentPatcher.Framework.Commands.Commands;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>Handles the 'patch' console command.</summary>
    internal class CommandHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The console commands recognized by Content Patcher, indexed by subcommand name.</summary>
        private readonly InvariantDictionary<ICommand> Commands;


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the root command.</summary>
        public string CommandName { get; } = "patch";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="screenManager">Manages state for each screen.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="contentHelper">Provides an API for managing content assets.</param>
        /// <param name="contentPacks">The loaded content packs.</param>
        /// <param name="getContext">Get the current token context.</param>
        /// <param name="updateContext">A callback which immediately updates the current condition context.</param>
        public CommandHandler(PerScreen<ScreenManager> screenManager, IMonitor monitor, IContentHelper contentHelper, LoadedContentPack[] contentPacks, Func<string, IContext> getContext, Action updateContext)
        {
            this.Monitor = monitor;

            this.Commands = new InvariantDictionary<ICommand>(
                new ICommand[]
                {
                    new DumpCommand(
                        monitor: this.Monitor,
                        getPatchManager: () => screenManager.Value.PatchManager
                    ),
                    new ExportCommand(
                        monitor: this.Monitor
                    ),
                    new HelpCommand(
                        monitor: this.Monitor,
                        getCommands: () => this.Commands
                    ),
                    new InvalidateCommand(
                        monitor: this.Monitor,
                        contentHelper: contentHelper
                    ),
                    new ParseCommand(
                        monitor: this.Monitor,
                        getContext: getContext
                    ),
                    new ReloadCommand(
                        monitor: this.Monitor,
                        getPatchLoader: () => screenManager.Value.PatchLoader,
                        contentPacks: contentPacks,
                        updateContext: updateContext
                    ),
                    new SummaryCommand(
                        monitor: this.Monitor,
                        getPatchManager: () => screenManager.Value.PatchManager,
                        getTokenManager: () => screenManager.Value.TokenManager,
                        getCustomLocationLoader: () => screenManager.Value.CustomLocationManager
                    ),
                    new UpdateCommand(
                        monitor: this.Monitor,
                        updateContext: updateContext
                    )
                }
                .ToDictionary(p => p.Name)
            );
        }

        /// <summary>Handle a console command.</summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        public bool Handle(string[] args)
        {
            string commandName = args.FirstOrDefault() ?? "help";
            string[] commandArgs = args.Skip(1).ToArray();

            if (this.Commands.TryGetValue(commandName, out ICommand command))
            {
                command.Handle(commandArgs);
                return true;
            }
            else
            {
                this.Monitor.Log($"The 'patch {args[0]}' command isn't valid. Type 'patch help' for a list of valid commands.", LogLevel.Error);
                return false;
            }
        }
    }
}
