namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Player animations
        ****/
        /// <summary>Whether to disable the confirmation dialogue before eating or drinking.</summary>
        public bool DisableEatAndDrinkConfirmation { get; set; } = false;

        /// <summary>The speed multiplier for eating and drinking.</summary>
        public int EatAndDrinkSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for fishing.</summary>
        public int FishingSpeed { get; set; } = 1;

        /// <summary>The speed multiplier for milking.</summary>
        public int MilkSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for shearing.</summary>
        public int ShearSpeed { get; set; } = 5;

        /****
        ** World animations
        ****/
        /// <summary>The speed multiplier for breaking geodes.</summary>
        public int BreakGeodeSpeed { get; set; } = 20;

        /// <summary>The speed multiplier for the casino slots minigame.</summary>
        public int CasinoSlotsSpeed { get; set; } = 8;

        /// <summary>The speed multiplier when Pam's bus is driving to/from the desert.</summary>
        public int PamBusSpeed { get; set; } = 6;

        /// <summary>The speed multiplier for falling trees.</summary>
        public int TreeFallSpeed { get; set; } = 1;

        /****
        ** UI animations
        ****/
        /// <summary>The speed multiplier for title menu transitions.</summary>
        public int TitleMenuTransitionSpeed { get; set; } = 10;

        /// <summary>The speed multiplier for the blinking-slot delay after clicking a load slot.</summary>
        public int LoadGameBlinkSpeed { get; set; } = 2;
    }
}
