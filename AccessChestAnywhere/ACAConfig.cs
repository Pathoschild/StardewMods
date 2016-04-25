using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace AccessChestAnywhere
{
    public class ACAConfig : Config
    {
        public Dictionary<string, string> hotkeys { get; set; }
        public bool whitelist { get; set; }
        public List<string> blacklists { get; set; }
        public List<string> whitelists { get; set; }

        public override T GenerateDefaultConfig<T>()
        {
            hotkeys = new Dictionary<string, string>
            {
                { "keyboard","B"},
                {"gamepad","LeftShoulder" }
            };
            whitelist = false;
            blacklists = new List<string>
            {
                "Chest",
                "ignore"
            };
            whitelists = new List<string>
            {
                "ACA",
                "include"
            };
            return this as T;
        }
    }
}
