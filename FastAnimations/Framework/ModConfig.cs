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
        public float EatAndDrinkSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for fishing.</summary>
        public float FishingSpeed { get; set; } = 1;

        /// <summary>The speed multiplier for harvesting crops or forage.</summary>
        public float HarvestSpeed { get; set; } = 3;

        /// <summary>The speed multiplier for milking.</summary>
        public float MilkSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for mounting or dismounting the horse.</summary>
        public float MountOrDismountSpeed { get; set; } = 2;

        /// <summary>The speed multiplier for shearing.</summary>
        public float ShearSpeed { get; set; } = 5;

        /// <summary>The speed multiplier for using tools.</summary>
        public float ToolSwingSpeed { get; set; } = 1;

        /// <summary>The speed multiplier for using weapons.</summary>
        public float WeaponSwingSpeed { get; set; } = 1;

        /****
        ** World animations
        ****/
        /// <summary>The speed multiplier for breaking geodes.</summary>
        public float BreakGeodeSpeed { get; set; } = 20;

        /// <summary>The speed multiplier for the casino slots minigame.</summary>
        public float CasinoSlotsSpeed { get; set; } = 8;

        /// <summary>The speed multiplier when Pam's bus is driving to/from the desert.</summary>
        public float PamBusSpeed { get; set; } = 6;

        /// <summary>The speed multiplier for falling trees.</summary>
        public float TreeFallSpeed { get; set; } = 1;

        /****
        ** UI animations
        ****/
        /// <summary>The speed multiplier for title menu transitions.</summary>
        public float TitleMenuTransitionSpeed { get; set; } = 10;

        /// <summary>The speed multiplier for the blinking-slot delay after clicking a load slot.</summary>
        public float LoadGameBlinkSpeed { get; set; } = 2;
    }
}
