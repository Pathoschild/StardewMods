using System;
using ContentPatcher.Framework.Commands.Commands;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>Handles the 'patch' console command.</summary>
    internal class CommandHandler : GenericCommandHandler
    {
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
        public CommandHandler(PerScreen<ScreenManager> screenManager, IMonitor monitor, IGameContentHelper contentHelper, LoadedContentPack[] contentPacks, Func<string?, IContext> getContext, Action updateContext)
            : base("patch", "Content Patcher", CommandHandler.BuildCommands(screenManager, monitor, contentHelper, contentPacks, getContext, updateContext), monitor) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Build the available commands.</summary>
        /// <param name="screenManager">Manages state for each screen.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="contentHelper">Provides an API for managing content assets.</param>
        /// <param name="contentPacks">The loaded content packs.</param>
        /// <param name="getContext">Get the current token context.</param>
        /// <param name="updateContext">A callback which immediately updates the current condition context.</param>
        private static ICommand[] BuildCommands(PerScreen<ScreenManager> screenManager, IMonitor monitor, IGameContentHelper contentHelper, LoadedContentPack[] contentPacks, Func<string?, IContext> getContext, Action updateContext)
        {
            return [
                new DumpCommand(
                    monitor: monitor,
                    getPatchManager: () => screenManager.Value.PatchManager
                ),
                new ExportCommand(
                    monitor: monitor,
                    contentHelper: contentHelper
                ),
                new InvalidateCommand(
                    monitor: monitor,
                    contentHelper: contentHelper
                ),
                new ParseCommand(
                    monitor: monitor,
                    getContext: getContext
                ),
                new ReloadCommand(
                    monitor: monitor,
                    getPatchLoader: () => screenManager.Value.PatchLoader,
                    getPatchManager: () => screenManager.Value.PatchManager,
                    contentPacks: contentPacks,
                    updateContext: updateContext
                ),
                new SummaryCommand(
                    monitor: monitor,
                    contentHelper: contentHelper,
                    getPatchManager: () => screenManager.Value.PatchManager,
                    getTokenManager: () => screenManager.Value.TokenManager,
                    getCustomLocationLoader: () => screenManager.Value.CustomLocationManager
                ),
                new UpdateCommand(
                    monitor: monitor,
                    updateContext: updateContext
                )
            ];
        }
    }
}
