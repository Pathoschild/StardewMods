using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    public class AccessChestAnywhere : Mod
    {
        public override void Entry(params object[] objects)
        {
            ControlEvents.KeyPressed += this.ControlEvents_KeyPressed;
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed != Keys.B || Game1.activeClickableMenu != null)
                return;
            Dictionary<string, List<Vector2>> chestList = new Dictionary<string, List<Vector2>>();
            foreach (GameLocation location in Game1.locations)
            {
                List<Vector2> vector2List = new List<Vector2>();
                foreach (var pair in location.objects)
                {
                    Chest chest = pair.Value as Chest;
                    if (chest == null)
                        continue;

                    if (chest.Name == "Chest")
                        chest.Name = $"Chest({pair.Key.X},{pair.Key.Y})";
                    if (!chest.name.Contains("ignore"))
                        vector2List.Add(pair.Key);
                }
                if (vector2List.Count > 0)
                    chestList.Add(location.name, vector2List);
            }
            if (chestList.Count > 0)
                Game1.activeClickableMenu = new ACAMenu(chestList);
        }
    }
}
