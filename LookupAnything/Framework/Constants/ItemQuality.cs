using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Constants
{
    /// <summary>Indicates an item quality. (Higher-quality items are sold at a higher price.)</summary>
    public enum ItemQuality
    {
        Normal = Object.lowQuality,
        Silver = Object.medQuality,
        Gold = Object.highQuality,
        Iridium = Object.bestQuality
    }
}