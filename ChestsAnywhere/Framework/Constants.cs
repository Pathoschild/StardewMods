using StardewValley;

namespace ChestsAnywhere.Framework
{
    /// <summary>Constant mod values.</summary>
    internal static class Constant
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The minimum supported version of Stardew Valley.</summary>
        public const string MinimumGameVersion = "1.1";

        /// <summary>The minimum supported version of SMAPI.</summary>
        public const string MinimumApiVersion = "0.40 1.1";

        /// <summary>The number of rows in an inventory grid.</summary>
        public const int SlotRows = 3;

        /// <summary>The number of columns in an inventory grid.</summary>
        public const int SlotColumns = 12;

        /// <summary>The maximum number of elements in an inventory.</summary>
        public const int SlotCount = Constant.SlotRows * Constant.SlotColumns;

        /// <summary>The inventory slot size for the current zoom level.</summary>
        public static int SlotSize => Game1.tileSize;
    }
}
