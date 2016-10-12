using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Constants
{
    /// <summary>Indicates an item quality. (Higher-quality items are sold at a higher price.)</summary>
    internal enum ItemQuality
    {
        Normal = Object.lowQuality,
        Silver = Object.medQuality,
        Gold = Object.highQuality,
        Iridium = Object.bestQuality
    }

    /// <summary>Extension methods for <see cref="ItemQuality"/>.</summary>
    internal static class ItemQualityExtensions
    {
        /// <summary>Get the quality name.</summary>
        /// <param name="current">The quality.</param>
        public static string GetName(this ItemQuality current)
        {
            return current.ToString().ToLower();
        }
    }
}