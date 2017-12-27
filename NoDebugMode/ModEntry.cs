using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.NoDebugMode
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameEvents.QuarterSecondTick += (sender, e) => this.SuppressGameDebug();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Immediately suppress the game's debug mode if it's enabled.</summary>
        private void SuppressGameDebug()
        {
            if (Game1.debugMode)
            {
                Game1.debugMode = false;
                this.Monitor.Log("No Debug Mode suppressed SMAPI F2 debug mode.");
            }
        }
    }
}
