namespace Pathoschild.Stardew.RotateToolbar.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to deselect the current slot after rotating the toolbar.</summary>
        public bool DeselectItemOnRotate { get; set; } = false;

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();
    }
}
