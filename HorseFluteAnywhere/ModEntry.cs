using Harmony;
using Pathoschild.Stardew.HorseFluteAnywhere.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.HorseFluteAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // add patches
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            UtilityPatcher.Hook(harmony, this.Monitor);
        }
    }
}
