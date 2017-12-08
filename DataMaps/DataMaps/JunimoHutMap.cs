using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows Junimo hut coverage.</summary>
    internal class JunimoHutMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for tiles harvested by a Junimo hut.</summary>
        private readonly Color Covered = Color.Green;

        /// <summary>The color for tiles not harvested by a Junimo hut.</summary>
        private readonly Color NotCovered = Color.Red;

        /// <summary>The maximum number of tiles from the center a Junimo hut can harvest.</summary>
        private readonly int MaxRadius = JunimoHut.cropHarvestRadius;


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        public JunimoHutMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.junimo-huts.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.junimo-huts.can-harvest"), this.Covered),
                new LegendEntry(translations.Get("maps.junimo-huts.cannot-harvest"), this.NotCovered)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea)
        {
            if (!(location is BuildableGameLocation buildableLocation))
                yield break;

            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            // get Junimo huts
            Rectangle searchArea = visibleArea.Expand(this.MaxRadius);
            JunimoHut[] huts =
                (
                    from JunimoHut hut in buildableLocation.buildings.OfType<JunimoHut>()
                    where searchArea.Contains(hut.tileX, hut.tileY)
                    select hut
                )
                .ToArray();

            // yield Junimo hut coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (JunimoHut hut in huts)
            {
                TileData[] tiles = this.GetCoverage(hut).Select(pos => new TileData(pos, this.Covered)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorders: true);
            }

            // yield unharvested crops
            TileData[] unharvested = this.GetUnharvestedCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.NotCovered)).ToArray();
            yield return new TileGroup(unharvested, outerBorders: true);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map terrain feature is a crop.</summary>
        /// <param name="terrain">The map terrain feature.</param>
        private bool IsCrop(TerrainFeature terrain)
        {
            return terrain is HoeDirt dirt && dirt.crop != null;
        }

        /// <summary>Get a Junimo hut tile radius.</summary>
        /// <param name="hut">The Junimo hut whose radius to get.</param>
        /// <remarks>Derived from <see cref="StardewValley.Characters.JunimoHarvester.pathFindToNewCrop_doWork"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(JunimoHut hut)
        {
            Vector2 origin = new Vector2(hut.tileX + 1, hut.tileY + 1); // centered on hut door
            for (int x = (int)origin.X - this.MaxRadius; x <= (int)origin.X + this.MaxRadius; x++)
            {
                for (int y = (int)origin.Y - this.MaxRadius; y <= (int)origin.Y + this.MaxRadius; y++)
                {
                    yield return new Vector2(x, y);
                }
            }
        }

        /// <summary>Get tiles containing crops not harvested by a Junimo hut.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="coveredTiles">The tiles harvested by Junimo huts.</param>
        private IEnumerable<Vector2> GetUnharvestedCrops(GameLocation location, Vector2[] visibleTiles, HashSet<Vector2> coveredTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                if (coveredTiles.Contains(tile))
                    continue;

                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && this.IsCrop(terrain))
                    yield return tile;
            }
        }
    }
}
