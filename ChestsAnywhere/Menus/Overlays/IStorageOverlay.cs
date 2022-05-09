using System;
using Pathoschild.Stardew.ChestsAnywhere.Framework;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays
{
    /// <summary>An overlay for a menu which lets the player navigate and edit a container.</summary>
    internal interface IStorageOverlay
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The overlay element which is currently handling input.</summary>
        Element ActiveElement { get; }

        /// <summary>An event raised when the player selects a chest.</summary>
        event Action<ManagedChest>? OnChestSelected;

        /// <summary>An event raised when the Automate options for a chest change.</summary>
        event Action<ManagedChest>? OnAutomateOptionsChanged;


        /*********
        ** Methods
        *********/
        /// <summary>Release all resources.</summary>
        void Dispose();
    }
}
