using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A collection of machines and storage which work as one unit.</summary>
    internal interface IMachineGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The main location containing the group (as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>), unless this is an aggregate machine group.</summary>
        string? LocationKey { get; }

        /// <summary>The machines in the group.</summary>
        IMachine[] Machines { get; }

        /// <summary>The containers in the group.</summary>
        IContainer[] Containers { get; }

        /// <summary>Whether the machine group is linked to a Junimo chest.</summary>
        [MemberNotNullWhen(false, nameof(IMachineGroup.LocationKey))]
        bool IsJunimoGroup { get; }

        /// <summary>Whether the group has the minimum requirements to enable internal automation (i.e., at least one chest and one machine).</summary>
        bool HasInternalAutomation { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Automate the machines inside the group.</summary>
        void Automate();

        /// <summary>Get the tiles covered by this machine group.</summary>
        /// <param name="locationKey">The location key for which to get tiles.</param>
        IReadOnlySet<Vector2> GetTiles(string locationKey);
    }
}
