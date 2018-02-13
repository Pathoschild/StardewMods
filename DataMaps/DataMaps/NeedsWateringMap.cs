using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows whether crops needs to be watered.</summary>
    internal class NeedsWateringMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for a dry crop.</summary>
        private readonly Color DryColor = Color.Red;

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        public NeedsWateringMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.needswatering.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.needswatering.dry-crops"), this.DryColor),
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            // yield dry crops
            TileData[] dryCrops = this.GetDryCrops(location, visibleTiles).Select(pos => new TileData(pos, this.DryColor)).ToArray();
            yield return new TileGroup(dryCrops, outerBorderColor: this.DryColor);
        }


        /*********
        ** Private methods
        *********/

        /// <summary>Get whether a map terrain feature is a dry-crop.</summary>
        /// <param name="terrain">The map terrain feature.</param>
        private bool IsDryCrop(TerrainFeature terrain)
        {
            return terrain is HoeDirt dirt && dirt.crop != null && dirt.state == 0;
        }

        /// <summary>Get tiles containing crops not covered by a sprinkler.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<Vector2> GetDryCrops(GameLocation location, Vector2[] visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && this.IsDryCrop(terrain))
                    yield return tile;
            }
        }
    }
}
