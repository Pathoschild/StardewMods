using System;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Constants
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

        /// <summary>Get the next better quality.</summary>
        /// <param name="current">The current quality.</param>
        public static ItemQuality GetNext(this ItemQuality current)
        {
            switch (current)
            {
                case ItemQuality.Normal:
                    return ItemQuality.Silver;
                case ItemQuality.Silver:
                    return ItemQuality.Gold;
                case ItemQuality.Gold:
                case ItemQuality.Iridium:
                    return ItemQuality.Iridium;
                default:
                    throw new NotSupportedException($"Unknown quality '{current}'.");
            }
        }
    }
}
