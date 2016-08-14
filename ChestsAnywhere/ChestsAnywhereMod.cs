using System.Linq;
using ChestsAnywhere.Components;
using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace ChestsAnywhere
{
    /// <summary>The mod entry point.</summary>
    public class ChestsAnywhereMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The selected chest.</summary>
        private Chest SelectedChest;

        /// <summary>The keyboard input map.</summary>
        private InputMapConfiguration<Keys> Keyboard;

        /// <summary>The controller input map.</summary>
        private InputMapConfiguration<Buttons?> Controller;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            // read config
            var config = new Configuration().InitializeConfig(this.BaseConfigPath);
            this.Keyboard = config.GetKeyboard();
            this.Controller = config.GetController();

            // hook UI
            ControlEvents.KeyPressed += (sender, e) => this.TryOpenMenu(e.KeyPressed, this.Keyboard.Toggle);
            ControlEvents.ControllerButtonPressed += (sender, e) => this.TryOpenMenu(e.ButtonPressed, this.Controller.Toggle);
            ControlEvents.ControllerTriggerPressed += (sender, e) => this.TryOpenMenu(e.ButtonPressed, this.Controller.Toggle);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Open the menu if the input matches the configured toggle, and another menu isn't already open.</summary>
        /// <typeparam name="T">The input type.</typeparam>
        /// <param name="expected">The configured toggle input.</param>
        /// <param name="received">The received toggle input.</param>
        private void TryOpenMenu<T>(T expected, T received)
        {
            if (received.Equals(expected) && Game1.activeClickableMenu == null)
                this.OpenMenu();
        }

        /// <summary>Open the menu UI.</summary>
        private void OpenMenu()
        {
            // get chests
            ManagedChest[] chests = (
                from location in Game1.locations
                from obj in location.Objects.Where(p => p.Value is Chest)
                let chest = new ManagedChest((Chest)obj.Value, location, obj.Key)
                where !chest.IsIgnored
                orderby chest.Location.Name ascending, (chest.Order ?? int.MaxValue) ascending, chest.Name ascending
                select chest
            ).ToArray();
            ManagedChest selectedChest = chests.FirstOrDefault(p => p.Chest == this.SelectedChest) ?? chests.First();

            // render menu
            if (chests.Any())
            {
                AccessChestMenu menu = new AccessChestMenu(chests, selectedChest, this.Keyboard, this.Controller);
                menu.OnChestSelected += chest => this.SelectedChest = chest.Chest; // remember selected chest on next load
                Game1.activeClickableMenu = menu;
            }
        }
    }
}
