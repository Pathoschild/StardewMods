using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Tiles
{
    /// <summary>A trash can that accepts input and provides output.</summary>
    internal class TrashCanMachine : BaseMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The machine's position in its location.</summary>
        private readonly Vector2 Tile;

        /// <summary>The game's list of trash cans the player has already checked.</summary>
        private readonly IList<bool> TrashCansChecked;

        /// <summary>The trash can index (or -1 if not a valid trash can).</summary>
        private readonly int TrashCanIndex = -1;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="town">The town to search.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="trashCanIndex">The trash can index.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public TrashCanMachine(Town town, Vector2 tile, int trashCanIndex, IReflectionHelper reflection)
            : base(town, BaseMachine.GetTileAreaFor(tile))
        {
            this.Tile = tile;
            this.TrashCansChecked = reflection.GetField<IList<bool>>(town, "garbageChecked").GetValue();
            if (trashCanIndex >= 0 && trashCanIndex < this.TrashCansChecked.Count)
                this.TrashCanIndex = trashCanIndex;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.TrashCanIndex == -1)
                return MachineState.Disabled;
            if (this.TrashCansChecked[this.TrashCanIndex])
                return MachineState.Processing;
            return MachineState.Done;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            // get trash
            Item item = this.GetRandomTrash(this.TrashCanIndex);
            if (item != null)
                return new TrackedItem(item, onEmpty: this.MarkChecked);

            // if nothing is returned, mark trash can checked
            this.MarkChecked(null);
            return null;
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it starts processing the next item.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void MarkChecked(Item item)
        {
            this.TrashCansChecked[this.TrashCanIndex] = true;
            Game1.stats.incrementStat("trashCansChecked", 1);
        }

        /// <summary>Get a random trash item ID.</summary>
        /// <param name="index">The trash can index.</param>
        /// <remarks>Derived from <see cref="Town.checkAction"/>.</remarks>
        private Item GetRandomTrash(int index)
        {
            Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + index * 77);
            double dailyLuck = Game1.MasterPlayer.DailyLuck;

            // randomization noise
            {
                for (int index2 = 0; index2 < random.Next(0, 100); ++index2)
                    random.NextDouble();
                for (int index2 = 0; index2 < random.Next(0, 100); ++index2)
                    random.NextDouble();
            }

            // rare chance of garbage hat
            if (Game1.stats.getStat("trashCansChecked") > 20U && random.NextDouble() < 0.002)
                return new Hat(66);

            // normal loot
            if ((Game1.stats.getStat("trashCansChecked") > 20U && random.NextDouble() < 0.01) || random.NextDouble() < 0.2 + dailyLuck)
            {
                int itemID = 168;
                switch (random.Next(10))
                {
                    case 0:
                        itemID = 168;
                        break;
                    case 1:
                        itemID = 167;
                        break;
                    case 2:
                        itemID = 170;
                        break;
                    case 3:
                        itemID = 171;
                        break;
                    case 4:
                        itemID = 172;
                        break;
                    case 5:
                        itemID = 216;
                        break;
                    case 6:
                        itemID = Utility.getRandomItemFromSeason(Game1.currentSeason, ((int)this.Tile.X) * 653 + ((int)this.Tile.Y) * 777, false);
                        break;
                    case 7:
                        itemID = 403;
                        break;
                    case 8:
                        itemID = 309 + random.Next(3);
                        break;
                    case 9:
                        itemID = 153;
                        break;
                }
                if (index == 3 && random.NextDouble() < 0.2 + dailyLuck)
                {
                    itemID = 535;
                    if (random.NextDouble() < 0.05)
                        itemID = 749;
                }
                if (index == 4 && random.NextDouble() < 0.2 + dailyLuck)
                {
                    itemID = 378 + random.Next(3) * 2;
                    random.Next(1, 5);
                }
                if (index == 5 && random.NextDouble() < 0.2 + dailyLuck && Game1.dishOfTheDay != null)
                    itemID = Game1.dishOfTheDay.ParentSheetIndex != 217 ? Game1.dishOfTheDay.ParentSheetIndex : 216;
                if (index == 6 && random.NextDouble() < 0.2 + dailyLuck)
                    itemID = 223;
                if (index == 7 && random.NextDouble() < 0.2)
                {
                    if (!Utility.HasAnyPlayerSeenEvent(191393))
                        itemID = 167;
                    if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
                        itemID = random.NextDouble() >= 0.25 ? 270 : 809;
                }

                return new SObject(itemID, 1);
            }

            return null;
        }
    }
}
