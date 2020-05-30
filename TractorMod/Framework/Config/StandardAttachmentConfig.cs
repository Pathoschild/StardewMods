namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the built-in tractor attachments.</summary>
    internal class StandardAttachmentsConfig
    {
        /// <summary>Configuration for the axe attachment.</summary>
        public AxeConfig Axe { get; set; } = new AxeConfig();

        /// <summary>Configuration for the fertilizer attachment.</summary>
        public GenericAttachmentConfig Fertilizer { get; set; } = new GenericAttachmentConfig();

        /// <summary>Configuration for the grass starter attachment.</summary>
        public GenericAttachmentConfig GrassStarter { get; set; } = new GenericAttachmentConfig();

        /// <summary>Configuration for the hoe attachment.</summary>
        public HoeConfig Hoe { get; set; } = new HoeConfig();

        /// <summary>Configuration for the milk pail attachment.</summary>
        public GenericAttachmentConfig MilkPail { get; set; } = new GenericAttachmentConfig();

        /// <summary>Configuration for the melee weapon attachment.</summary>
        public MeleeWeaponConfig MeleeWeapon { get; set; } = new MeleeWeaponConfig();

        /// <summary>Configuration for the pickaxe attachment.</summary>
        public PickAxeConfig PickAxe { get; set; } = new PickAxeConfig();

        /// <summary>Configuration for the scythe attachment.</summary>
        public ScytheConfig Scythe { get; set; } = new ScytheConfig();

        /// <summary>Configuration for the seeds attachment.</summary>
        public GenericAttachmentConfig Seeds { get; set; } = new GenericAttachmentConfig();

        /// <summary>Configuration for the shears attachment.</summary>
        public GenericAttachmentConfig Shears { get; set; } = new GenericAttachmentConfig();

        /// <summary>Configuration for the slingshot attachment.</summary>
        public GenericAttachmentConfig Slingshot { get; set; } = new GenericAttachmentConfig { Enable = false };

        /// <summary>Configuration for the watering can attachment.</summary>
        public GenericAttachmentConfig WateringCan { get; set; } = new GenericAttachmentConfig();

        /// <summary>Configuration for the Seed Bag mod attachment.</summary>
        public GenericAttachmentConfig SeedBagMod { get; set; } = new GenericAttachmentConfig();
    }
}
