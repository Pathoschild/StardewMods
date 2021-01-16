using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A collection of machines and storage which work as one unit.</summary>
    internal interface IMachineGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location containing the group.</summary>
        GameLocation Location { get; }

        /// <summary>The machines in the group.</summary>
        IMachine[] Machines { get; }

        /// <summary>The containers in the group.</summary>
        IContainer[] Containers { get; }

        /// <summary>The tiles comprising the group.</summary>
        Vector2[] Tiles { get; }

        /// <summary>Whether the group has the minimum requirements to enable internal automation (i.e., at least one chest and one machine).</summary>
        bool HasInternalAutomation { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Automate the machines inside the group.</summary>
        void Automate();
    }
}
