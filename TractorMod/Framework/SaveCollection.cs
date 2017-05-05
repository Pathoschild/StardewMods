using System.Collections.Generic;
using StardewValley;

namespace TractorMod.Framework
{
    internal class SaveCollection
    {
        /*********
        ** Accessors
        *********/
        public List<Save> saves = new List<Save>();


        /*********
        ** Public methods
        *********/
        public SaveCollection Add(Save input)
        {
            saves.Add(input);
            return this;
        }

        public Save FindSave(string nameInput, ulong input)
        {
            foreach (Save ASave in saves)
            {
                if (ASave.SaveSeed == input && ASave.FarmerName == nameInput)
                {
                    return ASave;
                }
            }
            return new Save();
        }

        public Save FindCurrentSave()
        {
            return FindSave(Game1.player.name, Game1.uniqueIDForThisGame);
        }
    }
}
