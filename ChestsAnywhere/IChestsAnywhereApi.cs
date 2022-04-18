#nullable disable

namespace Pathoschild.Stardew.ChestsAnywhere
{
    /// <summary>The Chests Anywhere API which other mods can access.</summary>
    public interface IChestsAnywhereApi
    {
        /// <summary>Get whether the chest overlay is currently visible on top of the current menu. In split-screen mode, this is for the current screen being updated/rendered.</summary>
        bool IsOverlayActive();

        /// <summary>Get whether the chest overlay is currently blocking input to the underlying menu (e.g. a dropdown or the options form is open). In split-screen mode, this is for the current screen being updated/rendered.</summary>
        bool IsOverlayModal();
    }
}
