namespace Pathoschild.Stardew.Common
{
    /// <summary>The general item type, to disambiguate IDs which can be duplicated between two sprite sheets.</summary>
    internal enum ItemType
    {
        /// <summary>The item isn't covered by one of the known types.</summary>
        Unknown,

        /// <summary>A generic inventory or placeable object from <c>Data\ObjectInformation</c>.</summary>
        Object,

        /// <summary>A scarecrow, tapper, crafting station, or similar placeable object from <c>Data\BigCraftablesInformation</c>.</summary>
        BigCraftable,

        /// <summary>Boot equipment from <c>Data\Boots</c>.</summary>
        Boots,

        /// <summary>Clothing equipment from <c>Data\Clothing</c>.</summary>
        Clothing,

        /// <summary>Hat equipment from <c>Data\Hats</c>.</summary>
        Hat,

        /// <summary>A furniture item from <c>Data\Furniture</c>.</summary>
        Furniture,

        /// <summary>A tool or weapon from <c>Data\Weapons</c>.</summary>
        Tool,

        /// <summary>A wallpaper or flooring item. These have no data, but are drawn using <c>Maps\walls_and_floors</c>.</summary>
        Wallpaper
    }
}
