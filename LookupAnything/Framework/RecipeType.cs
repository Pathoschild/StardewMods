namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>How a recipe is used to create an item.</summary>
    internal enum RecipeType
    {
        /// <summary>The recipe is crafted using a charcoal kiln.</summary>
        CharcoalKiln,

        /// <summary>The recipe is crafted using a cheese press.</summary>
        CheesePress,

        /// <summary>The recipe is used through the crafting menu.</summary>
        CraftingMenu,

        /// <summary>The recipe is cooked in the farmhouse kitchen.</summary>
        Cooking,

        /// <summary>The recipe is crafted using a furnace.</summary>
        Furnace,

        /// <summary>The recipe is crafted using a keg.</summary>
        Keg,

        /// <summary>The recipe is crafted using a loom.</summary>
        Loom,

        /// <summary>The recipe is crafted using a mayonnaise machine.</summary>
        MayonnaiseMachine,

        /// <summary>The recipe is crafted using a mill.</summary>
        Mill,

        /// <summary>The recipe is crafted using an oil maker.</summary>
        OilMaker,

        /// <summary>The recipe is crafted using an preserves jar.</summary>
        PreservesJar,

        /// <summary>The recipe is crafted using a recycling machine.</summary>
        RecyclingMachine,

        /// <summary>The recipe is crafted using a slime egg-press.</summary>
        SlimeEggPress
    }
}