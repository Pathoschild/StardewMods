namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A range at which chests should be accessible.</summary>
    internal enum ChestRange
    {
        /// <summary>All chests.</summary>
        Unlimited,

        /// <summary>Chests within the current world area.</summary>
        CurrentWorldArea,

        /// <summary>Chests within the current location.</summary>
        CurrentLocation,

        /// <summary>Don't allow remote access to any chest.</summary>
        None
    }
}
