using System.Collections.Generic;
using StardewValley;

namespace TractorMod.Framework
{
    internal class SaveCollection
    {
        /*********
        ** Accessors
        *********/
        public List<CustomSaveData> saves = new List<CustomSaveData>();


        /*********
        ** Public methods
        *********/
        public SaveCollection Add(CustomSaveData input)
        {
            saves.Add(input);
            return this;
        }

        public CustomSaveData FindSave(string nameInput, ulong input)
        {
            foreach (CustomSaveData ASave in saves)
            {
                if (ASave.SaveSeed == input && ASave.FarmerName == nameInput)
                {
                    return ASave;
                }
            }
            return new CustomSaveData();
        }

        public CustomSaveData FindCurrentSave()
        {
            return FindSave(Game1.player.name, Game1.uniqueIDForThisGame);
        }
    }
}
