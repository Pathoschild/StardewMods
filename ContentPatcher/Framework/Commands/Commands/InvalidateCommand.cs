using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which invalidates an asset from the game/SMAPI content cache.</summary>
    internal class InvalidateCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>The content helper with which to invalidate assets.</summary>
        private readonly IContentHelper ContentHelper;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="contentHelper">The content helper with which to invalidate assets.</param>
        public InvalidateCommand(IMonitor monitor, IContentHelper contentHelper)
            : base(monitor, "invalidate")
        {
            this.ContentHelper = contentHelper;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch invalidate
                   Usage: patch invalidate ""<asset name>""
                   Invalidates an asset from the game/SMAPI content cache. If it's an asset handled by SMAPI, the asset will be reloaded immediately and Content Patcher will reapply its changes to it. Otherwise the next code which loads the same asset will get a new instance.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            // get asset name
            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch invalidate' command expects one argument containing the target asset name. See 'patch help' for more info.", LogLevel.Error);
                return;
            }
            string assetName = args[0];

            // invalidate asset
            if (this.ContentHelper.InvalidateCache(assetName))
                this.Monitor.Log($"Invalidated asset '{assetName}'.", LogLevel.Info);
            else
                this.Monitor.Log($"The asset '{assetName}' wasn't found in the cache.", LogLevel.Warn);
        }
    }
}
