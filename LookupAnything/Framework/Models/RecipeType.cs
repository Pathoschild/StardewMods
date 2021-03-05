namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>Indicates an in-game recipe type.</summary>
    internal enum RecipeType
    {
        /// <summary>Food cooked in the kitchen.</summary>
        Cooking,

        /// <summary>An object crafted through the game menu.</summary>
        Crafting,

        /// <summary>The input/output for a crafting machine like a furnace.</summary>
        MachineInput,

        /// <summary>The input/output for a crafting building like a mill.</summary>
        BuildingInput,

        /// <summary>The materials needed to construct a building.</summary>
        BuildingBlueprint,

        /// <summary>Clothing created through the tailoring UI.</summary>
        TailorInput
    }
}
