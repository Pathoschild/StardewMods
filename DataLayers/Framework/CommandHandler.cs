using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Handles the 'data-layers' console command.</summary>
    internal class CommandHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Get the current data layer, if any.</summary>
        private readonly Func<ILayer> CurrentLayer;


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the root command.</summary>
        public string CommandName { get; } = "data-layers";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="currentLayer">Get the current data layer, if any.</param>
        public CommandHandler(IMonitor monitor, Func<ILayer> currentLayer)
        {
            this.Monitor = monitor;
            this.CurrentLayer = currentLayer;
        }

        /// <summary>Handle a console command.</summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        public bool Handle(string[] args)
        {
            string subcommand = args.FirstOrDefault();
            string[] subcommandArgs = args.Skip(1).ToArray();

            switch (subcommand?.ToLower())
            {
                case null:
                case "help":
                    return this.HandleHelp(subcommandArgs);

                case "export":
                    return this.HandleExport(subcommandArgs);

                default:
                    this.Monitor.Log($"The '{this.CommandName} {args[0]}' command isn't valid. Type '{this.CommandName} help' for a list of valid commands.", LogLevel.Debug);
                    return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Handle the 'data-layers help' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleHelp(string[] args)
        {
            // generate command info
            var helpEntries = new InvariantDictionary<string>
            {
                ["help"] = $"{this.CommandName} help\n   Usage: {this.CommandName} help\n   Lists all available {this.CommandName} commands.\n\n   Usage: {this.CommandName} help <cmd>\n   Provides information for a specific {this.CommandName} command.\n   - cmd: The {this.CommandName} command name.",
                ["export"] = $"{this.CommandName} export\n   Usage: {this.CommandName} export \"<asset name>\"\n   Exports the current data layer for the entire location to the game folder."
            };

            // build output
            StringBuilder help = new StringBuilder();
            if (!args.Any())
            {
                help.AppendLine(
                    $"The '{this.CommandName}' command is the entry point for Data Layers commands. You use it by"
                    + $"specifying a more specific command (like 'help' in '{this.CommandName} help'). Here are the"
                    + $"available commands:\n\n"
                );
                foreach (var entry in helpEntries)
                {
                    help.AppendLine(entry.Value);
                    help.AppendLine();
                }
            }
            else if (helpEntries.TryGetValue(args[0], out string entry))
                help.AppendLine(entry);
            else
                help.AppendLine($"Unknown command '{this.CommandName} {args[0]}'. Type '{this.CommandName} help' for available commands.");

            // write output
            this.Monitor.Log(help.ToString(), LogLevel.Debug);

            return true;
        }

        /// <summary>Handle the 'data-layers export' command.</summary>
        /// <param name="args">The subcommand arguments.</param>
        /// <returns>Returns whether the command was handled.</returns>
        private bool HandleExport(string[] args)
        {
            // validate context
            if (!Context.IsWorldReady || Game1.currentLocation == null)
            {
                this.Monitor.Log("The in-game world needs to be loaded before using this command.", LogLevel.Error);
                return true;
            }

            // get current data layer
            ILayer layer = this.CurrentLayer();
            if (layer == null)
            {
                this.Monitor.Log("There's no data layer being rendered; open the overlay in-game before using this command.", LogLevel.Error);
                return true;
            }

            // get layer data
            IDictionary<string, ExportLegendGroup> export = layer.Legend.ToDictionary(p => p.Id, p => new ExportLegendGroup(p.Id, p.Name));
            foreach (TileGroup group in this.GetTileGroups(layer))
            {
                if (!group.ShouldExport)
                    continue;

                foreach (TileData tile in group.Tiles)
                {
                    if (!export.TryGetValue(tile.Type.Id, out ExportLegendGroup exportGroup))
                        export[tile.Type.Id] = exportGroup = new ExportLegendGroup(tile.Type.Id, tile.Type.Name);

                    exportGroup.Tiles.Add(tile.TilePosition);
                }
            }

            // init export path
            string exportName = $"{DateTime.UtcNow:yyyy-MM-dd'T'HHmmss} {layer.Name} @ {Game1.currentLocation.Name}";
            string fullTargetPath = Path.Combine(Constants.ExecutionPath, "layer-export", string.Join("_", exportName.Split(Path.GetInvalidFileNameChars())) + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath));

            // export
            File.WriteAllText(fullTargetPath, JsonConvert.SerializeObject(export, Formatting.Indented));
            this.Monitor.Log($"Exported to '{fullTargetPath}'.", LogLevel.Info);

            return true;
        }

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
    }

    /// <summary>A group of tiles associated with a given legend.</summary>
    internal class ExportLegendGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique identifier for the legend entry.</summary>
        public string Id { get; }

        /// <summary>The translated legend entry name.</summary>
        public string DisplayName { get; }

        /// <summary>The tiles in the group.</summary>
        public List<Vector2> Tiles { get; } = new List<Vector2>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">A unique identifier for the legend entry.</param>
        /// <param name="displayName">The translated legend entry name.</param>
        public ExportLegendGroup(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }
    }
}
