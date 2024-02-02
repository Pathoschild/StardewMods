using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows whether crops needs to be watered.</summary>
    internal class CropFertilizerLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for fertilizer.</summary>
        private readonly LegendEntry Fertilizer;

        /// <summary>The legend entry for retaining soil.</summary>
        private readonly LegendEntry RetainingSoil;

        /// <summary>The legend entry for speed-gro.</summary>
        private readonly LegendEntry SpeedGro;

        /// <summary>The legend for crops with multiple fertilizers applied, if MultiFertilizer is installed.</summary>
        private readonly LegendEntry? Multiple;

        /// <summary>Handles access to the supported mod integrations.</summary>
        private readonly ModIntegrations Mods;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        /// <param name="colors">The colors to render.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        public CropFertilizerLayer(LayerConfig config, ColorScheme colors, ModIntegrations mods)
            : base(I18n.CropFertilizer_Name(), config)
        {
            const string layerId = "FertilizedCrops";

            this.Legend =
                new[]
                {
                    this.Fertilizer = new LegendEntry(I18n.Keys.CropFertilizer_Fertilizer, colors.Get(layerId, "Fertilizer", Color.Green)),
                    this.RetainingSoil = new LegendEntry(I18n.Keys.CropFertilizer_RetainingSoil, colors.Get(layerId, "RetainingSoil", Color.Blue)),
                    this.SpeedGro = new LegendEntry(I18n.Keys.CropFertilizer_SpeedGro, colors.Get(layerId, "SpeedGro", Color.Magenta)),
                    this.Multiple = mods.MultiFertilizer.IsLoaded
                        ? new LegendEntry(I18n.Keys.CropFertilizer_Multiple, colors.Get(layerId, "Multiple", Color.Red))
                        : null
                }
                .WhereNotNull()
                .ToArray();

            this.Mods = mods;
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            FertilizedTile[] fertilizedTiles = this.GetFertilizedTiles(location, visibleTiles).ToArray();

            bool hasMultiFertilizer = this.Multiple != null;
            return
                new[]
                {
                    this.GetGroup(fertilizedTiles, this.Fertilizer, tile => tile.HasFertilizer && (!hasMultiFertilizer || !tile.HasMultiFertilizer)),
                    this.GetGroup(fertilizedTiles, this.SpeedGro, tile => tile.HasSpeedGro && (!hasMultiFertilizer || !tile.HasMultiFertilizer)),
                    this.GetGroup(fertilizedTiles, this.RetainingSoil, tile => tile.HasRetainingSoil && (!hasMultiFertilizer || !tile.HasMultiFertilizer)),

                    // if MultiFertilizer is installed, show crops with multiple fertilizer types in their own group
                    hasMultiFertilizer
                        ? this.GetGroup(fertilizedTiles, this.Multiple!, tile => tile.HasMultiFertilizer)
                        : null
                }
                .WhereNotNull()
                .ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a tile group.</summary>
        /// <param name="tiles">The tiles to check.</param>
        /// <param name="type">The legend entry for the group.</param>
        /// <param name="match">Matches the fertilized tiles to include in the group.</param>
        private TileGroup GetGroup(IEnumerable<FertilizedTile> tiles, LegendEntry type, Func<FertilizedTile, bool> match)
        {
            IEnumerable<TileData> matched = (
                from tile in tiles
                where match(tile)
                select new TileData(tile.Tile, type)
            );

            return new TileGroup(matched, outerBorderColor: type.Color);
        }

        /// <summary>Get fertilized tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<FertilizedTile> GetFertilizedTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            return (
                from tilePos in visibleTiles
                let tile = this.TryGetFertilizedSoil(location, tilePos)
                where tile != null
                select tile.Value
            );
        }

        /// <summary>Get the fertilizer info for a given dirt tile, if any.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile position.</param>
        /// <returns>Returns whether the tile has any fertilizer applied.</returns>
        private FertilizedTile? TryGetFertilizedSoil(GameLocation location, Vector2 tile)
        {
            // get dirt tile
            HoeDirt? dirt = this.GetDirt(location, tile);

            // get applied fertilizer item IDs
            HashSet<string>? applied = null;
            if (dirt is not null && !this.IsDeadCrop(dirt))
            {
                if (this.Mods.MultiFertilizer.IsLoaded)
                    applied = new HashSet<string>(this.Mods.MultiFertilizer.GetAppliedFertilizers(dirt));
                else if (CommonHelper.IsItemId(dirt.fertilizer.Value, allowZero: false))
                    applied = new HashSet<string> { dirt.fertilizer.Value };
            }

            // get fertilizer info
            if (applied == null)
                return null;
            return new FertilizedTile(
                tile: tile,
                hasFertilizer:
                    applied.Contains(HoeDirt.fertilizerLowQualityID)
                    || applied.Contains(HoeDirt.fertilizerLowQualityQID)
                    || applied.Contains(HoeDirt.fertilizerHighQualityID)
                    || applied.Contains(HoeDirt.fertilizerHighQualityQID)
                    || applied.Contains(HoeDirt.fertilizerDeluxeQualityID)
                    || applied.Contains(HoeDirt.fertilizerDeluxeQualityQID),
                hasRetainingSoil:
                    applied.Contains(HoeDirt.waterRetentionSoilDeluxeID)
                    || applied.Contains(HoeDirt.waterRetentionSoilDeluxeQID)
                    || applied.Contains(HoeDirt.waterRetentionSoilQualityID)
                    || applied.Contains(HoeDirt.waterRetentionSoilQualityQID)
                    || applied.Contains(HoeDirt.waterRetentionSoilDeluxeID)
                    || applied.Contains(HoeDirt.waterRetentionSoilDeluxeQID),
                hasSpeedGro:
                    applied.Contains(HoeDirt.speedGroID)
                    || applied.Contains(HoeDirt.speedGroQID)
                    || applied.Contains(HoeDirt.superSpeedGroID)
                    || applied.Contains(HoeDirt.superSpeedGroQID)
                    || applied.Contains(HoeDirt.hyperSpeedGroID)
                    || applied.Contains(HoeDirt.hyperSpeedGroQID)
            );
        }

        /// <summary>A fertilized dirt tile.</summary>
        private readonly struct FertilizedTile
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The tile position.</summary>
            public Vector2 Tile { get; }

            /// <summary>Whether the dirt has fertilizer applied.</summary>
            public bool HasFertilizer { get; }

            /// <summary>Whether the dirt has water retaining soil applied.</summary>
            public bool HasRetainingSoil { get; }

            /// <summary>Whether the dirt has Speed-Gro applied.</summary>
            public bool HasSpeedGro { get; }

            /// <summary>Whether the tile has multiple fertilizer types applied.</summary>
            public bool HasMultiFertilizer { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="tile">The tile position.</param>
            /// <param name="hasFertilizer">Whether the dirt has fertilizer applied.</param>
            /// <param name="hasRetainingSoil">Whether the dirt has water retaining soil applied.</param>
            /// <param name="hasSpeedGro">Whether the dirt has Speed-Gro applied.</param>
            public FertilizedTile(Vector2 tile, bool hasFertilizer, bool hasRetainingSoil, bool hasSpeedGro)
            {
                this.Tile = tile;
                this.HasFertilizer = hasFertilizer;
                this.HasRetainingSoil = hasRetainingSoil;
                this.HasSpeedGro = hasSpeedGro;

                this.HasMultiFertilizer = ((hasFertilizer ? 1 : 0) + (hasRetainingSoil ? 1 : 0) + (hasSpeedGro ? 1 : 0)) > 1;
            }
        }
    }
}
