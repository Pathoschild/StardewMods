using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which shows which machines are currently processing.</summary>
    internal class MachineLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for machines with no input.</summary>
        private readonly LegendEntry Empty;

        /// <summary>The legend entry for machines that are currently processing input.</summary>
        private readonly LegendEntry Processing;

        /// <summary>The legend entry for machines whose output is ready to collect.</summary>
        private readonly LegendEntry Finished;

        /// <summary>Handles access to the supported mod integrations.</summary>
        private readonly ModIntegrations Mods;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public MachineLayer(ITranslationHelper translations, LayerConfig config, ModIntegrations mods, IInputHelper input, IMonitor monitor)
            : base(translations.Get("machines.name"), config, input, monitor)
        {
            this.Legend = new[]
            {
                this.Empty = new LegendEntry(translations, "machines.empty", Color.Red),
                this.Processing = new LegendEntry(translations, "machines.processing", Color.Orange),
                this.Finished = new LegendEntry(translations, "machines.finished", Color.Green)
            };
            this.Mods = mods;
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            // get tiles by color
            IDictionary<string, TileData[]> tiles = this
                .GetTiles(location, visibleArea, visibleTiles)
                .GroupBy(p => p.Type.Id)
                .ToDictionary(p => p.Key, p => p.ToArray());

            // create tile groups
            return new[] { this.Empty, this.Processing, this.Finished }
                .Select(type =>
                {
                    if (!tiles.TryGetValue(type.Id, out TileData[] groupTiles))
                        groupTiles = new TileData[0];

                    return new TileGroup(groupTiles, outerBorderColor: type.Color);
                })
                .ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, Rectangle visibleArea, Vector2[] visibleTiles)
        {
            IDictionary<Vector2, int> machineStates = this.Mods.Automate.GetMachineStates(location, visibleArea);
            foreach (Vector2 tile in visibleTiles)
            {
                LegendEntry type = null;
                if (machineStates.TryGetValue(tile, out int state))
                {
                    switch (state)
                    {
                        case 1:
                            type = this.Empty;
                            break;

                        case 2:
                            type = this.Processing;
                            break;

                        case 3:
                            type = this.Finished;
                            break;
                    }
                }

                if (type != null)
                    yield return new TileData(tile, type);
            }
        }
    }
}
