using System;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

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

        /// <summary>Manages loaded patches.</summary>
        private readonly Func<PatchManager> GetPatchManager;

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
        /// <param name="getPatchManager">Manages loaded patches.</param>
        /// <param name="contentPacks">The loaded content packs.</param>
        /// <param name="updateContext">A callback which immediately updates the current condition context.</param>
        public ReloadCommand(IMonitor monitor, Func<PatchLoader> getPatchLoader, Func<PatchManager> getPatchManager, LoadedContentPack[] contentPacks, Action updateContext)
            : base(monitor, "reload")
        {
            this.GetPatchLoader = getPatchLoader;
            this.GetPatchManager = getPatchManager;
            this.ContentPacks = contentPacks;
            this.UpdateContext = updateContext;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return
                $"""
                patch reload
                   Usage: patch reload "<content pack ID>"
                   Reloads every patch for the content pack with this mod ID. Non-patch content (like config schema and dynamic tokens) aren't reloaded.
                   
                   Usage: patch reload "<content pack ID>" "<include file>"
                   Reloads patches for the content pack with this mod ID, but only those loaded through an {PatchType.Include} patch with this relative path. Non-patch content (like config schema and dynamic tokens) aren't reloaded.
                """;
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            var patchLoader = this.GetPatchLoader();
            var patchManager = this.GetPatchManager();

            // get args
            string packId = ArgUtility.Get(args, 0, allowBlank: false);
            string? includePath = ArgUtility.Get(args, 1, allowBlank: false);
            if (packId is null)
            {
                this.Monitor.Log("The 'patch reload' command expects the first argument to be the target content pack's mod ID. See 'patch help' for more info.", LogLevel.Error);
                return;
            }

            // get pack
            RawContentPack? pack = this.ContentPacks.SingleOrDefault(p => p.Manifest.UniqueID == packId);
            if (pack == null)
            {
                this.Monitor.Log($"No Content Patcher content pack with the unique ID \"{packId}\".", LogLevel.Error);
                return;
            }

            // get patch
            IncludePatch? includePatch = null;
            if (includePath != null)
            {
                includePath = PathUtilities.NormalizePath(includePath);
                includePatch = patchManager.GetPatches().FirstOrDefault(p => p.FromAsset == includePath) as IncludePatch;

                if (includePatch is null)
                {
                    this.Monitor.Log($"There's no {PatchType.Include} patch which reads from the path \"{includePath}\".", LogLevel.Error);
                    return;
                }

                if (!includePatch.IsReady || !includePatch.Conditions.All(p => p.IsMatch))
                {
                    this.Monitor.Log("The specified patch isn't currently enabled.", LogLevel.Error);
                    return;
                }
            }

            // apply
            if (includePatch != null)
            {
                patchLoader.UnloadPatchesLoadedBy(includePatch);
                includePatch.AttemptLoad();
            }
            else
            {
                // unload patches
                patchLoader.UnloadPatchesLoadedBy(pack);

                // load pack patches
                if (!pack.TryReloadContent(out string? loadContentError))
                {
                    this.Monitor.Log($"Failed to reload content pack '{pack.Manifest.Name}' for configuration changes: {loadContentError}. The content pack may not be in a valid state.", LogLevel.Error); // should never happen
                    return;
                }

                // reload patches
                patchLoader.LoadPatches(
                    contentPack: pack,
                    rawPatches: pack.Content.Changes,
                    rootIndexPath: [pack.Index],
                    path: new LogPathBuilder(pack.Manifest.Name),
                    parentPatch: null
                );
            }

            // make the changes apply
            this.UpdateContext();

            this.Monitor.Log("Content pack reloaded.", LogLevel.Info);
        }
    }
}
