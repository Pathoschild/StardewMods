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
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
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
                    this.Monitor.Log($"Couldn't skip the menu: {ex}", LogLevel.Error);
                }
            };
        }
    }
}
