using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the built-in tractor attachments.</summary>
    internal class StandardAttachmentsConfig
    {
        /// <summary>Configuration for the axe attachment.</summary>
        public AxeConfig Axe { get; set; } = new();

        /// <summary>Configuration for the fertilizer attachment.</summary>
        public GenericAttachmentConfig Fertilizer { get; set; } = new();

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
        public GenericAttachmentConfig Seeds { get; set; } = new();

        /// <summary>Configuration for the shears attachment.</summary>
        public GenericAttachmentConfig Shears { get; set; } = new();

        /// <summary>Configuration for the slingshot attachment.</summary>
        public GenericAttachmentConfig Slingshot { get; set; } = new() { Enable = false };

        /// <summary>Configuration for the watering can attachment.</summary>
        public GenericAttachmentConfig WateringCan { get; set; } = new();

        /// <summary>Configuration for the Seed Bag mod attachment.</summary>
        public GenericAttachmentConfig SeedBagMod { get; set; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
        public void OnDeserialized(StreamingContext context)
        {
            this.Axe ??= new AxeConfig();
            this.Fertilizer ??= new GenericAttachmentConfig();
            this.GrassStarter ??= new GenericAttachmentConfig();
            this.Hoe ??= new HoeConfig();
            this.MilkPail ??= new GenericAttachmentConfig();
            this.MeleeBlunt ??= new MeleeBluntConfig();
            this.MeleeDagger ??= new MeleeDaggerConfig();
            this.MeleeSword ??= new MeleeSwordConfig();
            this.PickAxe ??= new PickAxeConfig();
            this.Scythe ??= new ScytheConfig();
            this.Seeds ??= new GenericAttachmentConfig();
            this.Shears ??= new GenericAttachmentConfig();
            this.Slingshot ??= new GenericAttachmentConfig { Enable = false };
            this.WateringCan ??= new GenericAttachmentConfig();
            this.SeedBagMod ??= new GenericAttachmentConfig();
        }
    }
}
