namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>How a recipe is used to create an item.</summary>
    internal enum RecipeType
    {
        /// <summary>The recipe is used through the crafting menu.</summary>
        Crafting,

        /// <summary>The recipe is cooked in the farmhouse kitchen.</summary>
        Cooking,

        /// <summary>The recipe is crafted using a furnace.</summary>
        Furnace,

        /// <summary>The recipe is crafted using an oil maker.</summary>
        OilMaker
    }
}