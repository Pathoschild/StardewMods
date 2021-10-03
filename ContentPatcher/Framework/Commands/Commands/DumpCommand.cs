using System;
using System.Globalization;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which provides specialized low-level reports.</summary>
    internal class DumpCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages loaded patches.</summary>
        private readonly Func<PatchManager> GetPatchManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="getPatchManager">Manages loaded patches.</param>
        public DumpCommand(IMonitor monitor, Func<PatchManager> getPatchManager)
            : base(monitor, "dump")
        {
            this.GetPatchManager = getPatchManager;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch dump
                   Usage: patch dump applied
                   Lists active patches grouped by their target value.

                   Usage: patch dump order
                   Lists every loaded patch in their apply order. When two patches edit the same asset, they'll be applied in the apply order.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            var patchManager = this.GetPatchManager();

            if (args.Length != 1)
            {
                this.Monitor.Log("The 'patch dump' command requires an argument which indicates what to dump. See 'patch help dump' for more info.", LogLevel.Error);
                return;
            }

            switch (args[0]?.ToLower())
            {
                case "applied":
                    {
                        StringBuilder str = new();
                        str.AppendLine("Here are the active patches grouped by their current target value. Within each group, patches are listed in the expected apply order and the checkbox indicates whether each patch is currently applied. See `patch summary` for more info about each patch, including reasons it may not be applied.");

                        foreach (var group in patchManager.GetPatchesByTarget().OrderByHuman(p => p.Key))
                        {
                            str.AppendLine();
                            str.AppendLine(group.Key);
                            str.AppendLine("".PadRight(group.Key.Length, '-'));

                            var patches = group.Value
                                .OrderByDescending(p => p.Type == PatchType.Load)
                                .ThenBy(p => p, PatchIndexComparer.Instance);

                            foreach (var patch in patches)
                                str.AppendLine($"   [{(patch.IsApplied ? "X" : " ")}] {patch.Type} {patch.Path}");
                        }

                        this.Monitor.Log(str.ToString(), LogLevel.Info);
                    }
                    break;

                case "order":
                    {
                        var patches = patchManager.GetPatches()
                            .Select((patch, globalIndex) => new
                            {
                                globalPosition = (globalIndex + 1).ToString(CultureInfo.InvariantCulture),
                                indexPath = string.Join(" > ", patch.IndexPath),
                                name = patch.Path.ToString()
                            })
                            .ToArray();

                        int indexLen = Math.Max("order".Length, patches.Max(p => p.globalPosition.Length));
                        int pathLen = Math.Max("index path".Length, patches.Max(p => p.indexPath.Length));

                        StringBuilder str = new();
                        str.AppendLine("Here's the global patch definition order across all loaded content packs, which affects the order that patches are applied. The 'order' column is the patch's global position in the order; the 'index path' column is Content Patcher's internal hierarchical definition order.");
                        str.AppendLine();
                        str.AppendLine($"   {"order".PadRight(indexLen, ' ')}   {"index path".PadRight(pathLen, ' ')}   patch");
                        str.AppendLine($"   {"".PadRight(indexLen, '-')}   {"".PadRight(pathLen, '-')}   -----");

                        foreach (var patch in patches)
                            str.AppendLine($"   {patch.globalPosition.PadRight(indexLen, ' ')}   {patch.indexPath.PadRight(pathLen, ' ')}   {patch.name}");

                        this.Monitor.Log(str.ToString(), LogLevel.Info);
                    }
                    break;

                default:
                    this.Monitor.Log("Invalid 'patch dump' argument. See 'patch help dump' for more info.", LogLevel.Error);
                    break;
            }
        }
    }
}
