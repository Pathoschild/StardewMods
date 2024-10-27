using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The API which lets other mods interact with Automate.</summary>
    public interface IAutomateAPI
    {
        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        void AddFactory(IAutomationFactory factory);

        /// <summary>Finds all discrete automation groups (connected groups of containers and machines) in a location.</summary>
        /// <remarks>
        /// Specifying a <paramref name="tileArea"/> limits the initial search area, but does not limit the items within a group.
        /// <see cref="IAutomationGroup.Machines"/> and other elements can be outside the requested area as long as at least one element is inside.
        /// </remarks>
        /// <param name="location">The location in which to perform the search.</param>
        /// <param name="tileArea">Optional tile area to restrict the search. If not specified, all tiles in the <paramref name="location"/> are included.</param>
        /// <param name="includeDisabled">Whether or not to include disabled groups in the result.</param>
        /// <returns>All groups present within the specified <paramref name="location"/> and within the given <paramref name="tileArea"/></returns>
        IEnumerable<IAutomationGroup> GetAutomationGroups(GameLocation location, Rectangle? tileArea = null, bool includeDisabled = false);

        /// <summary>Get the status of machines in a tile area. This is a specialized API for Data Layers and similar mods.</summary>
        /// <param name="location">The location for which to display data.</param>
        /// <param name="tileArea">The tile area for which to display data.</param>
        IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea);
    }
}
