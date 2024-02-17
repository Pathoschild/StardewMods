using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A crab pot that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="CrabPot.DayUpdate"/> and <see cref="CrabPot.performObjectDropInAction"/>.</remarks>
    internal class CrabPotMachine : GenericObjectMachine<CrabPot>
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The qualified fish IDs for which any crab pot has logged an 'invalid fish data' error.</summary>
        private static readonly ISet<string> LoggedInvalidDataErrors = new HashSet<string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public CrabPotMachine(CrabPot machine, GameLocation location, Vector2 tile, IMonitor monitor)
            : base(machine, location, tile)
        {
            this.Monitor = monitor;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            MachineState state = this.GetGenericState();

            if (state == MachineState.Empty && (this.Machine.bait.Value != null || !this.PlayerNeedsBait(this.GetOwner())))
                state = MachineState.Processing;

            return state;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack? GetOutput()
        {
            return this.GetTracked(this.Machine.heldObject.Value, onEmpty: this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            // get bait
            if (input.TryGetIngredient(p => p.Sample.TypeDefinitionId == ItemRegistry.type_object && p.Sample.Category == SObject.baitCategory, 1, out IConsumable? bait))
            {
                this.Machine.bait.Value = (SObject)bait.Take()!;
                this.Machine.lidFlapping = true;
                this.Machine.lidFlapTimer = 60;
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
            Farmer owner = this.GetOwner();

            // add fishing XP
            owner.gainExperience(Farmer.fishingSkill, 5);

            // mark fish caught for achievements and stats
            IDictionary<string, string> fishData = DataLoader.Fish(Game1.content);
            if (fishData.TryGetValue(item.ItemId, out string? fishRow))
            {
                int size = 0;
                try
                {
                    string[] fields = fishRow.Split('/');
                    int lowerSize = fields.Length > 5 ? Convert.ToInt32(fields[5]) : 1;
                    int upperSize = fields.Length > 5 ? Convert.ToInt32(fields[6]) : 10;
                    size = Game1.random.Next(lowerSize, upperSize + 1);
                }
                catch (Exception ex)
                {
                    // The fish length stats don't affect anything, so it's not worth notifying the
                    // user; just log one trace message per affected fish for troubleshooting.
                    if (CrabPotMachine.LoggedInvalidDataErrors.Add(item.QualifiedItemId))
                        this.Monitor.Log($"The game's fish data has an invalid entry ({item.ItemId}: {fishData[item.ItemId]}). Automated crab pots won't track fish length stats for that fish.\n{ex}");
                }

                owner.caughtFish(item.ItemId, size);
            }

            // reset pot
            this.GenericReset(item);
            pot.tileIndexToShow = 710;
            pot.bait.Value = null;
            pot.lidFlapping = true;
            pot.lidFlapTimer = 60f;
            pot.shake = Vector2.Zero;
            pot.shakeTimer = 0f;
        }

        /// <summary>Get whether the current player needs to bait crab pots.</summary>
        /// <param name="owner">The player who owns the machine.</param>
        private bool PlayerNeedsBait(Farmer owner)
        {
            return !owner.professions.Contains(11); // no bait needed if luremaster
        }
    }
}
