using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles
{
    /// <summary>Provides lookup data for in-game map tiles.</summary>
    internal class TileLookupProvider : BaseLookupProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>Whether to show raw tile info like tilesheets and tile indexes.</summary>
        private readonly Func<bool> ShowRawTileInfo;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
        public TileLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, ModConfig config, Func<bool> showRawTileInfo)
            : base(reflection, gameHelper)
        {
            this.Config = config;
            this.ShowRawTileInfo = showRawTileInfo;
        }

        /// <inheritdoc />
        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            ISubject subject = this.BuildSubject(location, lookupTile);
            if (subject != null)
                yield return new TileTarget(this.GameHelper, lookupTile, () => subject);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build a subject.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile being looked up.</param>
        private ISubject BuildSubject(GameLocation location, Vector2 tile)
        {
            bool showRaw = this.ShowRawTileInfo();

            if (this.IsCrystalCavePuzzle(location, tile, out int? crystalId))
                return new CrystalCavePuzzleSubject(this.GameHelper, location, tile, showRaw, this.Config.ProgressionMode, crystalId);
            if (showRaw)
                return new TileSubject(this.GameHelper, location, tile, true);
            return null;
        }

        /// <summary>Get whether the tile is part of the <see cref="IslandWestCave1"/> crystal cave puzzle.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile position.</param>
        /// <param name="crystalId">The ID of the crystal being looked up, if any.</param>
        /// <remarks>See game logic in <see cref="IslandWestCave1"/>.</remarks>
        private bool IsCrystalCavePuzzle(GameLocation location, Vector2 tile, out int? crystalId)
        {
            crystalId = null;

            if (location is IslandWestCave1)
            {
                // match crystal puzzle action
                if (this.HasTileProperty(location, tile, "Action", "Buildings", out string[] actionArgs) && actionArgs.Any())
                {
                    switch (actionArgs[0])
                    {
                        case "CrystalCaveActivate":
                            return true;

                        case "Crystal":
                            if (actionArgs.Length > 1 && int.TryParse(actionArgs[1], out int id))
                                crystalId = id;
                            return true;
                    }
                }

                // match top of statue
                else if (location.getTileIndexAt((int)tile.X, (int)tile.Y, "Buildings") == 31)
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a tile property is defined.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile position.</param>
        /// <param name="name">The property name.</param>
        /// <param name="layer">The map layer name to check.</param>
        /// <param name="arguments">The space-separated property values, if any.</param>
        private bool HasTileProperty(GameLocation location, Vector2 tile, string name, string layer, out string[] arguments)
        {
            string property = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, name, layer);
            arguments = property?.Split(' ').ToArray() ?? new string[0];
            return property != null;
        }
    }
}
