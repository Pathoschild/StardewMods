using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Constants
{
    /// <summary>The direction a player is facing.</summary>
    internal enum FacingDirection
    {
        /// <summary>The player is facing the top of the screen.</summary>
        Up = Game1.up,

        /// <summary>The player is facing the right side of the screen.</summary>
        Right = Game1.right,

        /// <summary>The player is facing the bottom of the screen.</summary>
        Down = Game1.down,

        /// <summary>The player is facing the left side of the screen.</summary>
        Left = Game1.left
    }
}
