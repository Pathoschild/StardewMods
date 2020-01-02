using CommonType = Pathoschild.Stardew.Common.ItemType;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The general item type, to disambiguate IDs which can be duplicated between two sprite sheets.</summary>
    public enum ItemType
    {
        /// <summary>The item isn't covered by one of the known types.</summary>
        Unknown = CommonType.Unknown,

        /// <summary>A generic inventory or placeable object from <c>Data\ObjectInformation</c>.</summary>
        Object = CommonType.Object,

        /// <summary>A scarecrow, tapper, crafting station, or similar placeable object from <c>Data\BigCraftablesInformation</c>.</summary>
        BigCraftable = CommonType.BigCraftable,

        /// <summary>Boot equipment from <c>Data\Boots</c>.</summary>
        Boots = CommonType.Boots,

        /// <summary>Clothing equipment from <c>Data\Clothing</c>.</summary>
        Clothing = CommonType.Clothing,

        /// <summary>Hat equipment from <c>Data\Hats</c>.</summary>
        Hat = CommonType.Hat,

        /// <summary>A furniture item from <c>Data\Furniture</c>.</summary>
        Furniture = CommonType.Furniture,

        /// <summary>A tool or weapon from <c>Data\Weapons</c>.</summary>
        Tool = CommonType.Tool,

        /// <summary>A wallpaper or flooring item. These have no data, but are drawn using <c>Maps\walls_and_floors</c>.</summary>
        Wallpaper = CommonType.Wallpaper
    }
}
