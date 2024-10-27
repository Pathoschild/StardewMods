using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>An automatable entity, which can implement a more specific type like <see cref="IMachine"/> or <see cref="IContainer"/>. If it doesn't implement a more specific type, it's treated as a connector with no additional logic.</summary>
    public interface IAutomatable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying game object such as <see cref="StardewValley.Object"/> or <see cref="StardewValley.TerrainFeatures.TerrainFeature"/> that performs the automation function.</summary>
        /// <remarks>A <c>null</c> value may indicate that there is no single/primary participant, or that the automation is implemented in a mod based on an older version of the Automate API.</remarks>
        object? Instance => null;

        /// <summary>The location which contains the machine.</summary>
        GameLocation Location { get; }

        /// <summary>Role performed by this instance, if known.</summary>
        AutomationRole Role => AutomationRole.Unspecified;

        /// <summary>The tile area covered by the machine.</summary>
        Rectangle TileArea { get; }
    }
}
