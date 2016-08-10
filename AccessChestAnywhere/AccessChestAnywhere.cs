using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    /// <summary>The mod entry point.</summary>
    public class AccessChestAnywhere : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            ControlEvents.KeyPressed += this.ControlEvents_KeyPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed != Keys.B || Game1.activeClickableMenu != null)
                return;

            IDictionary<string, Vector2[]> chestsByLocation = new Dictionary<string, Vector2[]>();
            foreach (GameLocation location in Game1.locations)
            {
                List<Vector2> chestPositions = new List<Vector2>();
                foreach (var pair in location.objects)
                {
                    Chest chest = pair.Value as Chest;
                    if (chest == null)
                        continue;

                    if (chest.Name == "Chest")
                        chest.Name = $"Chest({pair.Key.X},{pair.Key.Y})";
                    if (!chest.Name.Contains("ignore"))
                        chestPositions.Add(pair.Key);
                }
                if (chestPositions.Any())
                    chestsByLocation.Add(location.Name, chestPositions.ToArray());
            }
            if (chestsByLocation.Any())
                Game1.activeClickableMenu = new ACAMenu(chestsByLocation);
        }
    }
}
