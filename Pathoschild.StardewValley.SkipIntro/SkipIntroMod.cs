using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace Pathoschild.StardewValley.SkipIntro
{
    /// <summary>The mod entry point.</summary>
    public class SkipIntroMod : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            MenuEvents.MenuChanged += (sender, e) =>
            {
                // get menu
                TitleMenu menu = e.NewMenu as TitleMenu;
                if (menu == null)
                    return;

                // skip intro
                menu.skipToTitleButtons(); // skips everything except the Chucklefish logo
                menu.GetType().GetField("chuckleFishTimer", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(menu, 0);
            };
        }
    }
}
