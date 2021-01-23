namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the key enables the game debug mode.</summary>
        public bool AllowGameDebug { get; set; }

        /// <summary>Allow debug commands which are destructive. A command is considered destructive if it immediately ends the current day, randomizes the player or farmhouse decorations, or crashes the game.</summary>
        public bool AllowDangerousCommands { get; set; }

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();
    }
}
