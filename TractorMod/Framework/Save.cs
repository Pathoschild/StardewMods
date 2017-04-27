using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TractorMod.Framework
{
    public class Save
    {
        /*********
        ** Accessors
        *********/
        public string FarmerName { get; set; } = "";
        public ulong SaveSeed { get; set; }
        public List<Vector2> TractorHouse = new List<Vector2>();
        public Save() { SaveSeed = ulong.MaxValue; }


        /*********
        ** Public methods
        *********/
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
}
