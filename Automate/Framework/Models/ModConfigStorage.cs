namespace Pathoschild.Stardew.Automate.Framework.Models;

/// <summary>The configuration for a specific storage type.</summary>
internal class ModConfigStorage
{
    internal enum SwitchTypes
    {
        Default,
        Enabled,
        Disabled
    }

    /*********
    ** Accessors
    *********/
    /// <summary>Triple state switch: follow default / enabled / disabled</summary>
    public SwitchTypes Switch { get; set; } = SwitchTypes.Default;
}
