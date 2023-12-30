namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the built-in tractor attachments.</summary>
    internal class StandardAttachmentsConfig
    {
        /// <summary>Configuration for the axe attachment.</summary>
        public AxeConfig Axe { get; set; } = new();

        /// <summary>Configuration for the fertilizer attachment.</summary>
        public ExtendedDistanceConfig Fertilizer { get; set; } = new() { IncreaseDistance = false };

        /// <summary>Configuration for the grass starter attachment.</summary>
        public GenericAttachmentConfig GrassStarter { get; set; } = new();

        /// <summary>Configuration for the hoe attachment.</summary>
        public HoeConfig Hoe { get; set; } = new();

        /// <summary>Configuration for the milk pail attachment.</summary>
        public GenericAttachmentConfig MilkPail { get; set; } = new();

        /// <summary>Configuration for the melee blunt weapons attachment.</summary>
        public MeleeBluntConfig MeleeBlunt { get; set; } = new();

        /// <summary>Configuration for the melee dagger attachment.</summary>
        public MeleeDaggerConfig MeleeDagger { get; set; } = new();

        /// <summary>Configuration for the melee sword attachment.</summary>
        public MeleeSwordConfig MeleeSword { get; set; } = new();

        /// <summary>Configuration for the pickaxe attachment.</summary>
        public PickAxeConfig PickAxe { get; set; } = new();

        /// <summary>Configuration for the scythe attachment.</summary>
        public ScytheConfig Scythe { get; set; } = new();

        /// <summary>Configuration for the seeds attachment.</summary>
        public ExtendedDistanceConfig Seeds { get; set; } = new() { IncreaseDistance = false };

        /// <summary>Configuration for the shears attachment.</summary>
        public GenericAttachmentConfig Shears { get; set; } = new();

        /// <summary>Configuration for the slingshot attachment.</summary>
        public GenericAttachmentConfig Slingshot { get; set; } = new() { Enable = false };

        /// <summary>Configuration for the watering can attachment.</summary>
        public ExtendedDistanceConfig WateringCan { get; set; } = new();

        /// <summary>Configuration for the Seed Bag mod attachment.</summary>
        public ExtendedDistanceConfig SeedBagMod { get; set; } = new();
    }
}
