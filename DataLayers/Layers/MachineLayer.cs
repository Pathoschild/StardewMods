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
        ** Properties
        *********/
        /// <summary>The color for a machine with no input.</summary>
        private readonly Color EmptyColor = Color.Red;

        /// <summary>The color for a machine that's currently processing input.</summary>
        private readonly Color ProcessingColor = Color.Orange;

        /// <summary>The color for a machine whose output is ready to collect.</summary>
        private readonly Color FinishedColor = Color.Green;

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
                new LegendEntry(translations.Get("machines.empty"), this.EmptyColor),
                new LegendEntry(translations.Get("machines.processing"), this.ProcessingColor),
                new LegendEntry(translations.Get("machines.finished"), this.FinishedColor)
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
            IDictionary<Color, TileData[]> tiles = this
                .GetTiles(location, visibleArea)
                .GroupBy(p => p.Color)
                .ToDictionary(p => p.Key, p => p.ToArray());

            // create tile groups
            foreach (Color color in new[] { this.EmptyColor, this.ProcessingColor, this.FinishedColor })
            {
                if (!tiles.TryGetValue(color, out TileData[] groupTiles))
                    groupTiles = new TileData[0];
                yield return new TileGroup(groupTiles, outerBorderColor: color);
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
                Color? color = null;
                if (machineStates.TryGetValue(tile, out int state))
                {
                    switch (state)
                    {
                        case 1:
                            color = this.EmptyColor;
                            break;

                        case 2:
                            color = this.ProcessingColor;
                            break;

                        case 3:
                            color = this.FinishedColor;
                            break;
                    }
                }

                if (color.HasValue)
                    yield return new TileData(tile, color.Value);
            }
        }
    }
}
