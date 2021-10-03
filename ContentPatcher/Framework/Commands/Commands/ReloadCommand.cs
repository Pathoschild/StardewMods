using System;
using System.Linq;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which reloads the patches from a given content pack.</summary>
    internal class ReloadCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages loading and unloading patches.</summary>
        private readonly Func<PatchLoader> GetPatchLoader;

        /// <summary>The loaded content packs.</summary>
        private readonly LoadedContentPack[] ContentPacks;

        /// <summary>A callback which immediately updates the current condition context.</summary>
        private readonly Action UpdateContext;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="getPatchLoader">Manages loading and unloading patches.</param>
        /// <param name="contentPacks">The loaded content packs.</param>
        /// <param name="updateContext">A callback which immediately updates the current condition context.</param>
        public ReloadCommand(IMonitor monitor, Func<PatchLoader> getPatchLoader, LoadedContentPack[] contentPacks, Action updateContext)
            : base(monitor, "reload")
        {
            this.GetPatchLoader = getPatchLoader;
            this.ContentPacks = contentPacks;
            this.UpdateContext = updateContext;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch reload
                   Usage: patch reload ""<content pack ID>""
                   Reloads the patches of the content.json of a content pack. Config schema changes and dynamic token changes are unsupported.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            var patchLoader = this.GetPatchLoader();

            // get pack ID
            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch reload' command expects a single arguments containing the target content pack ID. See 'patch help' for more info.", LogLevel.Error);
                return;
            }
            string packId = args[0];

            // get pack
            RawContentPack pack = this.ContentPacks.SingleOrDefault(p => p.Manifest.UniqueID == packId);
            if (pack == null)
            {
                this.Monitor.Log($"No Content Patcher content pack with the unique ID \"{packId}\".", LogLevel.Error);
                return;
            }

            // unload patches
            patchLoader.UnloadPatchesLoadedBy(pack, false);

            // load pack patches
            if (!pack.TryReloadContent(out string loadContentError))
            {
                this.Monitor.Log($"Failed to reload content pack '{pack.Manifest.Name}' for configuration changes: {loadContentError}. The content pack may not be in a valid state.", LogLevel.Error); // should never happen
                return;
            }

            // reload patches
            patchLoader.LoadPatches(
                contentPack: pack,
                rawPatches: pack.Content.Changes,
                rootIndexPath: new[] { pack.Index },
                path: new LogPathBuilder(pack.Manifest.Name),
                reindex: true,
                parentPatch: null
            );

            // make the changes apply
            this.UpdateContext();

            this.Monitor.Log("Content pack reloaded.", LogLevel.Info);
        }
    }
}
