using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>Constructs machines, containers, or connectors which can be added to a machine group.</summary>
    internal interface IAutomationFactory
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Get an entity</summary>
        /// <param name="obj">The object for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        IAutomatable GetFor(Object obj, GameLocation location, in Vector2 tile);

        /// <summary>Get a machine for the given terrain feature, if applicable.</summary>
        /// <param name="feature">The terrain feature for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        IAutomatable GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile);

        /// <summary>Get a machine for the given building, if applicable.</summary>
        /// <param name="building">The building for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        IAutomatable GetFor(Building building, BuildableGameLocation location);

        /// <summary>Get a machine for the given tile, if applicable.</summary>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <remarks>Shipping bin logic from <see cref="Farm.leftClick"/>, garbage can logic from <see cref="Town.checkAction"/>.</remarks>
        IAutomatable GetForTile(GameLocation location, in Vector2 tile);
    }
}
