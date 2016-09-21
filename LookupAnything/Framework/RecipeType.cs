namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>How a recipe is used to create an item.</summary>
    internal enum RecipeType
    {
        /// <summary>The recipe is crafted using a charcoal kiln.</summary>
        CharcoalKiln,

        /// <summary>The recipe is used through the crafting menu.</summary>
        CraftingMenu,

        /// <summary>The recipe is cooked in the farmhouse kitchen.</summary>
        Cooking,

        /// <summary>The recipe is crafted using a furnace.</summary>
        Furnace,

        /// <summary>The recipe is crafted using an oil maker.</summary>
        OilMaker,

        /// <summary>The recipe is crafted using a recycling machine.</summary>
        RecyclingMachine,

        /// <summary>The recipe is crafted using a slime egg-press.</summary>
        SlimeEggPress
    }
}