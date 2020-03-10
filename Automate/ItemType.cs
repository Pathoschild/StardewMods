using CommonType = Pathoschild.Stardew.Common.Items.ItemData.ItemType;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The general item type, to disambiguate IDs which can be duplicated between two sprite sheets.</summary>
    public enum ItemType
    {
        /// <summary>The item isn't covered by one of the known types.</summary>
        Unknown = CommonType.Unknown,

        /// <summary>A big craftable object in <see cref="StardewValley.Game1.bigCraftablesInformation"/></summary>
        BigCraftable = CommonType.BigCraftable,

        /// <summary>A <see cref="StardewValley.Objects.Boots"/> item.</summary>
        Boots = CommonType.Boots,

        /// <summary>A <see cref="StardewValley.Objects.Clothing"/> item.</summary>
        Clothing = CommonType.Clothing,

        /// <summary>A <see cref="StardewValley.Objects.Wallpaper"/> flooring item.</summary>
        Flooring = CommonType.Flooring,

        /// <summary>A <see cref="StardewValley.Objects.Furniture"/> item.</summary>
        Furniture = CommonType.Furniture,

        /// <summary>A <see cref="StardewValley.Objects.Hat"/> item.</summary>
        Hat = CommonType.Hat,

        /// <summary>Any object in <see cref="StardewValley.Game1.objectInformation"/> (except rings).</summary>
        Object = CommonType.Object,

        /// <summary>A <see cref="StardewValley.Objects.Ring"/> item.</summary>
        Ring = CommonType.Ring,

        /// <summary>A <see cref="StardewValley.Tool"/> tool.</summary>
        Tool = CommonType.Tool,

        /// <summary>A <see cref="StardewValley.Objects.Wallpaper"/> wall item.</summary>
        Wallpaper = CommonType.Wallpaper,

        /// <summary>A <see cref="StardewValley.Tools.MeleeWeapon"/> or <see cref="StardewValley.Tools.Slingshot"/> item.</summary>
        Weapon = CommonType.Weapon
    }
}
