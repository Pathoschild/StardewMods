using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace AccessChestAnywhere
{
    public class AccessChestAnywhere : Mod
    {
        public static ACAConfig config { get; private set; }

        public Dictionary<string, List<Chest>> chests;

        public override void Entry(params object[] objects)
        {
#if DEBUG
            Log.AsyncR("DEBUGGING!!!!!!!");
#endif
            config = new ACAConfig().InitializeConfig(BaseConfigPath);
            chests = new Dictionary<string, List<Chest>>();

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            ControlEvents.ControllerButtonPressed += ControlEvents_ButtonPressed;
            ControlEvents.ControllerTriggerPressed += ControlEvents_TriggerPressed;
            GameEvents.OneSecondTick += GameEvents_SecondTick;
        }

        private void GameEvents_SecondTick(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu == null)
            {
                config = new ACAConfig().InitializeConfig(BaseConfigPath);
                if (chests.Count > 0) chests.Clear();
            }
        }

        private void ControlEvents_TriggerPressed(object sender, EventArgsControllerTriggerPressed e)
        {
            if (e.ButtonPressed.ToString().Equals(config.hotkeys["gamepad"]) && Game1.activeClickableMenu == null)
            {
                openMenu();
            }
        }

        private void ControlEvents_ButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (e.ButtonPressed.ToString().Equals(config.hotkeys["gamepad"]) && Game1.activeClickableMenu == null)
            {
                openMenu();
            }
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString().Equals(config.hotkeys["keyboard"]) && Game1.activeClickableMenu == null)
            {
                openMenu();
            }
        }

        private void openMenu()
        {
#if DEBUG
            Log.AsyncR("openMenu");
#endif
            populateChests();
            if (chests.Count > 0)
            {
                Game1.activeClickableMenu = new ACA(this);
            }
        }

        private bool showChest(string name)
        {
            if (config.whitelist)
            {
                foreach (string s in config.whitelists)
                {
                    if (name.Contains(s)) return true;
                }
                return false;
            }
            else
            {
                foreach (string s in config.blacklists)
                {
                    if (name.Contains(s)) return false;
                }
                return true;
            }
        }

        public void populateChests()
        {
            chests.Clear();
            foreach (GameLocation location in Game1.locations)
            {
                List<Chest> chestss = new List<Chest>();
                foreach (KeyValuePair<Vector2, Object> o in location.objects)
                {
                    if (o.Value is Chest)
                    {
                        if(showChest(o.Value.name))
                            chestss.Add((Chest) o.Value);
                    }
                }
                if (location is BuildableGameLocation)
                {
                    BuildableGameLocation bLocation = (BuildableGameLocation) location;
                    foreach (Building building in bLocation.buildings)
                    {
                        if (building.indoors != null)
                        {
                            foreach (KeyValuePair<Vector2, Object> o in building.indoors.objects)
                            {
                                if (o.Value is Chest)
                                {
                                    if (showChest(o.Value.name))
                                        chestss.Add((Chest)o.Value);
                                }
                            }
                        }
                    }
                }
                if (location is FarmHouse)
                {
                    FarmHouse farmHouse = (FarmHouse) location;
                    if (farmHouse.fridge != null && farmHouse.fridge.items.Count > 0)
                    {
                        if (farmHouse.fridge.name == "Chest") farmHouse.fridge.Name = "Fridge";
                        chestss.Add(farmHouse.fridge);
                    }
                }
                if (chestss.Count > 0)
                {
                    var orderedChests = from element in chestss
                        orderby element.name
                        select element;
                    chests.Add(location.name,new List<Chest>(orderedChests));
                }
            }
        }
    }
}
