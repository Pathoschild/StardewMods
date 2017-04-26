using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace TractorMod.Framework
{
    public class SaveCollection
    {
        public class Save
        {
            public string FarmerName { get; set; } = "";
            public ulong SaveSeed { get; set; }
            public List<Vector2> TractorHouse = new List<Vector2>();
            public Save() { SaveSeed = ulong.MaxValue; }
            public Save(string nameInput, ulong input)
            {
                SaveSeed = input;
                FarmerName = nameInput;
            }

            public Save AddTractorHouse(int inputX, int inputY)
            {
                foreach (Vector2 THS in TractorHouse)
                {
                    if (THS.X == inputX && THS.Y == inputY)
                        return this;
                }
                TractorHouse.Add(new Vector2(inputX, inputY));
                return this;
            }
        }
        public List<Save> saves = new List<Save>();
        public SaveCollection() { }
        public SaveCollection Add(Save input)
        {
            saves.Add(input);
            return this;
        }
        public Save FindSave(string nameInput, ulong input)
        {
            foreach (SaveCollection.Save ASave in saves)
            {
                if (ASave.SaveSeed == input && ASave.FarmerName == nameInput)
                {
                    return ASave;
                }
            }
            return new SaveCollection.Save();
        }

        public Save FindCurrentSave()
        {
            return FindSave(Game1.player.name, Game1.uniqueIDForThisGame);
        }
    }
}
