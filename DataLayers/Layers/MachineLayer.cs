using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

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
        public MachineLayer(ITranslationHelper translations, LayerConfig config, ModIntegrations mods)
            : base(translations.Get("machines.name"), config)
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
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            // get tiles by color
            IDictionary<string, TileData[]> tiles = this
                .GetTiles(location, visibleArea)
                .GroupBy(p => p.Type.Id)
                .ToDictionary(p => p.Key, p => p.ToArray());

            // create tile groups
            foreach (LegendEntry type in new[] { this.Empty, this.Processing, this.Finished })
            {
                if (!tiles.TryGetValue(type.Id, out TileData[] groupTiles))
                    groupTiles = new TileData[0];
                yield return new TileGroup(groupTiles, outerBorderColor: type.Color);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, Rectangle visibleArea)
        {
            IDictionary<Vector2, int> machineStates = this.Mods.Automate.GetMachineStates(location, visibleArea);
            foreach (Vector2 tile in visibleArea.GetTiles())
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
