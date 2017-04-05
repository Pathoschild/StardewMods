using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A crab pot that accepts input and provides output.</summary>
    internal class CrabPotMachine : GenericMachine<CrabPot>
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public CrabPotMachine(CrabPot machine, IReflectionHelper reflection)
            : base(machine)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.heldObject == null)
            {
                bool hasBait = this.Machine.bait != null || Game1.player.professions.Contains(11); // no bait needed if luremaster
                return hasBait
                    ? MachineState.Processing
                    : MachineState.Empty;
            }
            return this.Machine.readyForHarvest
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public override void Reset(bool outputTaken)
        {
            CrabPot pot = this.Machine;

            // apply logic from CrabPot.cs
            if (outputTaken)
            {
                // add fishing XP
                Game1.player.gainExperience(SFarmer.fishingSkill, 5);

                // mark fish caught for achievements and stats
                IDictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
                if (fishData.ContainsKey(pot.heldObject.parentSheetIndex))
                {
                    string[] fields = fishData[pot.heldObject.parentSheetIndex].Split('/');
                    int lowerSize = fields.Length > 5 ? Convert.ToInt32(fields[5]) : 1;
                    int upperSize = fields.Length > 5 ? Convert.ToInt32(fields[6]) : 10;
                    Game1.player.caughtFish(pot.heldObject.parentSheetIndex, Game1.random.Next(lowerSize, upperSize + 1));
                }
            }

            // reset pot
            pot.readyForHarvest = false;
            pot.heldObject = null;
            pot.tileIndexToShow = 710;
            pot.bait = null;
            this.Reflection.GetPrivateField<bool>(pot, "lidFlapping").SetValue(true);
            this.Reflection.GetPrivateField<float>(pot, "lidFlapTimer").SetValue(60f);
            this.Reflection.GetPrivateField<Vector2>(pot, "shake").SetValue(Vector2.Zero);
            this.Reflection.GetPrivateField<float>(pot, "shakeTimer").SetValue(0f);
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            // get bait
            if (chests.TryGetIngredient(item => item.category == SObject.baitCategory, 1, out Requirement bait))
            {
                bait.Consume();
                this.Machine.bait = (SObject)bait.GetOne();
                return true;
            }

            return false;
        }
    }
}
