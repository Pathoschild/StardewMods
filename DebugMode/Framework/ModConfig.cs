namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Allow debug commands which are destructive. A command is considered destructive if it immediately ends the current day, randomises the player or farmhouse decorations, or crashes the game.</summary>
        public bool AllowDangerousCommands { get; set; }

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();
    }
}
