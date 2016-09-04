using System;
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
                try
                {
                    // get menu
                    TitleMenu menu = e.NewMenu as TitleMenu;
                    if (menu == null)
                        return;

                    // skip intro (except the Chucklefish logo)
                    menu.skipToTitleButtons();

                    // skip Chucklefish logo
                    FieldInfo logoTimer = menu.GetType().GetField("chuckleFishTimer", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (logoTimer == null)
                        throw new InvalidOperationException("The 'chuckleFishTimer' field doesn't exist.");
                    logoTimer.SetValue(menu, 0);
                }
                catch (Exception ex)
                {
                    Log.Error($"'Skip Intro' mod couldn't skip the menu: {ex}");
                }
            };
        }
    }
}
