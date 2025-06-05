using System.Diagnostics.CodeAnalysis;

namespace Pathoschild.Stardew.FastAnimations.Framework;

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

    /// <summary>The speed multiplier for holding up an item.</summary>
    public float HoldUpItemSpeed { get; set; } = 5;

    /// <summary>The speed multiplier for playing the horse flute.</summary>
    public float HorseFluteSpeed { get; set; } = 6;

    /// <summary>The speed multiplier for milking.</summary>
    public float MilkSpeed { get; set; } = 5;

    /// <summary>The speed multiplier for mounting or dismounting the horse.</summary>
    public float MountOrDismountSpeed { get; set; } = 2;

    /// <summary>The speed multiplier for reading a book.</summary>
    public float ReadBookSpeed { get; set; } = 2;

    /// <summary>The speed multiplier for shearing.</summary>
    public float ShearSpeed { get; set; } = 5;

    /// <summary>The speed multiplier for using slingshot.</summary>
    public float UseSlingshotSpeed { get; set; } = 1;

    /// <summary>The speed multiplier for using tools.</summary>
    public float ToolSwingSpeed { get; set; } = 1;

    /// <summary>The speed multiplier for using a totem.</summary>
    public float UseTotemSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for using weapons.</summary>
    public float WeaponSwingSpeed { get; set; } = 1;

    /****
    ** World animations
    ****/
    /// <summary>The speed multiplier for breaking geodes.</summary>
    public float BreakGeodeSpeed { get; set; } = 20;

    /// <summary>The speed multiplier for the casino slots minigame.</summary>
    public float CasinoSlotsSpeed { get; set; } = 8;

    /// <summary>The speed multiplier for the screen fade to black.</summary>
    public float FadeSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for fishing text like 'HIT!' and 'PERFECT'.</summary>
    public float FishingTextSpeed { get; set; } = 1;

    /// <summary>The speed multiplier for opening a fishing treasure chest.</summary>
    public float FishingTreasureSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for the volcano forge.</summary>
    public float ForgeSpeed { get; set; } = 8;

    /// <summary>The speed multiplier for opening a chest.</summary>
    public float OpenChestSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for opening a dialogue box.</summary>
    public float OpenDialogueBoxSpeed { get; set; } = 4;

    /// <summary>The speed multiplier when Pam's bus is driving to/from the desert.</summary>
    public float PamBusSpeed { get; set; } = 6;

    /// <summary>The speed multiplier for the parrot express.</summary>
    public float ParrotExpressSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for Lewis' ticket prize machine.</summary>
    public float PrizeTicketMachineSpeed { get; set; } = 20;

    /// <summary>The speed multiplier for tailoring.</summary>
    public float TailorSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for falling trees.</summary>
    public float TreeFallSpeed { get; set; } = 1;

    /// <summary>The speed multiplier for the Stardew Valley Fair wheel spin minigame.</summary>
    public float WheelSpinSpeed { get; set; } = 16;

    /****
    ** UI animations
    ****/
    /// <summary>The speed multiplier for the dialogue typing animation.</summary>
    public float DialogueTypeSpeed { get; set; } = 4;

    /// <summary>The speed multiplier for shipping menu transitions.</summary>
    public float ShippingMenuTransitionSpeed { get; set; } = 1;

    /// <summary>The speed multiplier for title menu transitions.</summary>
    public float TitleMenuTransitionSpeed { get; set; } = 10;

    /// <summary>The speed multiplier for the blinking-slot delay after clicking a load slot.</summary>
    public float LoadGameBlinkSpeed { get; set; } = 2;

    /****
    ** Experimental options
    ****/
    /// <summary>The speed multiplier for event cutscenes.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for clarity for players who edit the config directly.")]
    public float Experimental_EventSpeed { get; set; } = 1;
}
