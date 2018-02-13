namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to disable the confirmation dialogue before eating or drinking.</summary>
        public bool DisableEatAndDrinkConfirmation { get; set; } = false;

        /// <summary>The speed multiplier for breaking geodes.</summary>
        public int BreakGeodeSpeed { get; set; } = 20;

        /// <summary>The speed multiplier for the casino slots minigame.</summary>
        public int CasinoSlotsSpeed { get; set; } = 8;

        /// <summary>The speed multiplier for eating and drinking.</summary>
        public int EatAndDrinkSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for fishing.</summary>
        public int FishingSpeed { get; set; } = 1;

        /// <summary>The speed multiplier for milking.</summary>
        public int MilkSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for shearing.</summary>
        public int ShearSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for falling trees.</summary>
        public int TreeFallSpeed { get; set; } = 1;
    }
}
