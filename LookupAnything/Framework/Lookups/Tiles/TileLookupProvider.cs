using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles
{
    /// <summary>Provides lookup data for in-game map tiles.</summary>
    internal class TileLookupProvider : BaseLookupProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether to show raw tile info like tilesheets and tile indexes.</summary>
        private readonly Func<bool> ShowRawTileInfo;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
        public TileLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<bool> showRawTileInfo)
            : base(reflection, gameHelper)
        {
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
            if (this.ShowRawTileInfo())
                return new TileSubject(this.GameHelper, location, tile, this.ShowRawTileInfo());

            return null;
        }
    }
}
