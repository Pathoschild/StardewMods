using System.Collections.Generic;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>
    /// Provides details about a group of mutually-connected automation objects.
    /// </summary>
    public interface IAutomationGroup
    {
        /// <summary>
        /// A <b>non-persistent</b> ID that identifies the group as a whole.
        /// </summary>
        /// <remarks>
        /// IDs can be used to detect identical groups within the context of a single game session, e.g. to dedupe the results of <see cref="IAutomateAPI.GetAutomationGroups"/> invoked on multiple tile areas in one location.
        /// However, this ID should never be stored in mod data or game data as it is not guaranteed to be stable across game loads.
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// The list of containers, which hold the items that can be consumed by <see cref="Machines"/> for processing.
        /// </summary>
        IReadOnlyList<IAutomatable> Containers { get; }

        /// <summary>
        /// The list of machines, which process the contents of <see cref="Containers"/>.
        /// </summary>
        IReadOnlyList<IAutomatable> Machines { get; }
    }
}
