using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A crab pot that accepts input and provides output.</summary>
    /// <remarks>See the game's machine logic in <see cref="CrabPot.DayUpdate"/> and <see cref="CrabPot.performObjectDropInAction"/>.</remarks>
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

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject, onEmpty: this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            // get bait
            if (input.TryGetIngredient(SObject.baitCategory, 1, out IConsumable bait))
            {
                this.Machine.bait = (SObject)bait.Take();
                this.Reflection.GetField<bool>(this.Machine, "lidFlapping").SetValue(true);
                this.Reflection.GetField<float>(this.Machine, "lidFlapTimer").SetValue(60);
                return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        /// <remarks>XP and achievement logic based on <see cref="CrabPot.checkForAction"/>.</remarks>
        private void Reset(Item item)
        {
            CrabPot pot = this.Machine;

            // add fishing XP
            Game1.player.gainExperience(SFarmer.fishingSkill, 5);

            // mark fish caught for achievements and stats
            IDictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            if (fishData.ContainsKey(item.parentSheetIndex))
            {
                string[] fields = fishData[item.parentSheetIndex].Split('/');
                int lowerSize = fields.Length > 5 ? Convert.ToInt32(fields[5]) : 1;
                int upperSize = fields.Length > 5 ? Convert.ToInt32(fields[6]) : 10;
                Game1.player.caughtFish(item.parentSheetIndex, Game1.random.Next(lowerSize, upperSize + 1));
            }

            // reset pot
            pot.readyForHarvest = false;
            pot.heldObject = null;
            pot.tileIndexToShow = 710;
            pot.bait = null;
            this.Reflection.GetField<bool>(pot, "lidFlapping").SetValue(true);
            this.Reflection.GetField<float>(pot, "lidFlapTimer").SetValue(60f);
            this.Reflection.GetField<Vector2>(pot, "shake").SetValue(Vector2.Zero);
            this.Reflection.GetField<float>(pot, "shakeTimer").SetValue(0f);
        }
    }
}
