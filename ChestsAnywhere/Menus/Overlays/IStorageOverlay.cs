using System;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays
{
    /// <summary>An overlay for a menu which lets the player navigate and edit a container.</summary>
    internal interface IStorageOverlay
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The menu instance for which the overlay was created.</summary>
        IClickableMenu ForMenuInstance { get; }

        /// <summary>An event raised when the player selects a chest.</summary>
        event Action<ManagedChest> OnChestSelected;

        /// <summary>An event raised when the Automate options for a chest change.</summary>
        event Action<ManagedChest> OnAutomateOptionsChanged;


        /*********
        ** Methods
        *********/
        /// <summary>Release all resources.</summary>
        void Dispose();
    }
}
