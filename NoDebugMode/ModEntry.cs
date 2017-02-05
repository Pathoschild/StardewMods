using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.NoStardewDebugMode
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
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
                Log.Debug("No Debug Mode suppressed SMAPI F2 debug mode.");
            }
        }
    }
}
