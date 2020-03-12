using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using xTile.Dimensions;
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
            // Note: this code must match the exact sequence of random calls to produce the same output as the game code.
            Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + index * 77);
            Farmer who = Game1.MasterPlayer;
            Location tileLocation = new Location((int)this.Tile.X, (int)this.Tile.Y);

            {
                int num2 = random.Next(0, 100);
                for (int index2 = 0; index2 < num2; ++index2)
                    random.NextDouble();
                int num3 = random.Next(0, 100);
                for (int index2 = 0; index2 < num3; ++index2)
                    random.NextDouble();
            }

            bool trashCheck = Game1.stats.getStat("trashCansChecked") > 20U && random.NextDouble() < 0.01; // flag1
            bool hatCheck = Game1.stats.getStat("trashCansChecked") > 20U && random.NextDouble() < 0.002; // flag2

            if (hatCheck)
                return new Hat(66); // garbage hat

            if (trashCheck || random.NextDouble() < 0.2 + who.DailyLuck)
            {
                int itemID = 168; // trash
                switch (random.Next(10))
                {
                    case 0:
                        itemID = 168; // trash
                        break;
                    case 1:
                        itemID = 167; // Joja Cola
                        break;
                    case 2:
                        itemID = 170; // broken glasses
                        break;
                    case 3:
                        itemID = 171; // broken CD
                        break;
                    case 4:
                        itemID = 172; // soggy newspaper
                        break;
                    case 5:
                        itemID = 216; // bread
                        break;
                    case 6:
                        itemID = Utility.getRandomItemFromSeason(Game1.currentSeason, tileLocation.X * 653 + tileLocation.Y * 777, false); // seasonal item
                        break;
                    case 7:
                        itemID = 403; // field snack
                        break;
                    case 8:
                        itemID = 309 + random.Next(3); // acorn, maple seed, or pine cone
                        break;
                    case 9:
                        itemID = 153; // green algae
                        break;
                }
                if (index == 3 && random.NextDouble() < 0.2 + who.DailyLuck)
                {
                    itemID = 535; // geode
                    if (random.NextDouble() < 0.05)
                        itemID = 749; // omni geode
                }
                if (index == 4 && random.NextDouble() < 0.2 + who.DailyLuck)
                {
                    itemID = 378 + random.Next(3) * 2; // copper ore, iron ore, coal
                    random.Next(1, 5);
                }
                if (index == 5 && random.NextDouble() < 0.2 + who.DailyLuck && Game1.dishOfTheDay != null)
                    itemID = Game1.dishOfTheDay.ParentSheetIndex != 217 ? Game1.dishOfTheDay.ParentSheetIndex : 216; // bread
                if (index == 6 && random.NextDouble() < 0.2 + who.DailyLuck)
                    itemID = 223; // cookie
                if (index == 7 && random.NextDouble() < 0.2)
                {
                    if (!Utility.HasAnyPlayerSeenEvent(191393))
                        itemID = 167; // Joja Cola
                    if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
                        itemID = random.NextDouble() >= 0.25 ? 270 : 809; // corn or movie ticket
                }

                return new SObject(itemID, 1);
            }

            return null;
        }
    }
}
