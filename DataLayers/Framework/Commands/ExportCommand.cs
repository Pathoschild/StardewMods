using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Framework.Commands
{
    /// <summary>A console command which prints a summary of automated machines.</summary>
    internal class ExportCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the current data layer, if any.</summary>
        private readonly Func<ILayer?> CurrentLayer;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="currentLayer">Get the current data layer, if any.</param>
        public ExportCommand(IMonitor monitor, Func<ILayer?> currentLayer)
            : base(monitor, "export")
        {
            this.CurrentLayer = currentLayer;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                data-layers export
                   Usage: data-layers export
                   Exports the current data layer for the entire location to the game folder.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            // validate context
            if (!Context.IsWorldReady || Game1.currentLocation == null)
            {
                this.Monitor.Log("The in-game world needs to be loaded before using this command.", LogLevel.Error);
                return;
            }

            // get current data layer
            ILayer? layer = this.CurrentLayer();
            if (layer == null)
            {
                this.Monitor.Log("There's no data layer being rendered; open the overlay in-game before using this command.", LogLevel.Error);
                return;
            }

            // get layer data
            IDictionary<string, ExportLegendGroup> export = layer.Legend.ToDictionary(p => p.Id, p => new ExportLegendGroup(p.Id, p.Name));
            foreach (TileGroup group in this.GetTileGroups(layer))
            {
                if (!group.ShouldExport)
                    continue;

                foreach (TileData tile in group.Tiles)
                {
                    if (!export.TryGetValue(tile.Type.Id, out ExportLegendGroup? exportGroup))
                        export[tile.Type.Id] = exportGroup = new ExportLegendGroup(tile.Type.Id, tile.Type.Name);

                    exportGroup.Tiles.Add(tile.TilePosition);
                }
            }

            // init export path
            string exportName = $"{DateTime.UtcNow:yyyy-MM-dd'T'HHmmss} {layer.Name} @ {Game1.currentLocation.Name}";
            string fullTargetPath = Path.Combine(Constants.GamePath, "layer-export", string.Join("_", exportName.Split(Path.GetInvalidFileNameChars())) + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);

            // export
            File.WriteAllText(fullTargetPath, JsonConvert.SerializeObject(export, Formatting.Indented));
            this.Monitor.Log($"Exported to '{fullTargetPath}'.", LogLevel.Info);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the current tile data for a layer.</summary>
        /// <param name="layer">The layer instance.</param>
        private IEnumerable<TileGroup> GetTileGroups(ILayer layer)
        {
            // validate
            if (Game1.currentLocation == null)
                throw new InvalidOperationException("The in-game world needs to be loaded before using this command.");
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            // get tile groups
            GameLocation location = Game1.currentLocation;
            int width = location.Map.Layers.Max(p => p.LayerWidth);
            int height = location.Map.Layers.Max(p => p.LayerHeight);

            var visibleArea = new Rectangle(0, 0, width, height);
            return layer.Update(Game1.currentLocation, visibleArea, visibleArea.GetTiles().ToArray(), new Vector2(0, 0));
        }

        /// <summary>A group of tiles associated with a given legend.</summary>
        /// <param name="Id">A unique identifier for the legend entry.</param>
        /// <param name="DisplayName">The translated legend entry name.</param>
        private record ExportLegendGroup(string Id, string DisplayName)
        {
            /// <summary>The tiles in the group.</summary>
            public List<Vector2> Tiles { get; } = new();
        }
    }
}
