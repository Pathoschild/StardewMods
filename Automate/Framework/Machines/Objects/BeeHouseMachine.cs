using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A bee house that accepts input and provides output.</summary>
    /// <remarks>See the game's machine logic in <see cref="SObject.performDropDownAction"/>, <see cref="SObject.checkForAction"/>, and <see cref="SObject.minutesElapsed"/>.</remarks>
    internal class BeeHouseMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The location containing the machine.</summary>
        private readonly GameLocation Location;

        /// <summary>The machine's position in its location.</summary>
        private readonly Vector2 Tile;

        /// <summary>The honey types produced by this beehouse indexed by input ID.</summary>
        private readonly IDictionary<int, SObject.HoneyType> HoneyTypes = new Dictionary<int, SObject.HoneyType>
        {
            [376] = SObject.HoneyType.Poppy,
            [591] = SObject.HoneyType.Tulip,
            [593] = SObject.HoneyType.SummerSpangle,
            [595] = SObject.HoneyType.FairyRose,
            [597] = SObject.HoneyType.BlueJazz
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        public BeeHouseMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine)
        {
            this.Location = location;
            this.Tile = tile;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return Game1.currentSeason == "winter"
                ? MachineState.Disabled
                : base.GetState();
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            // get raw output
            SObject output = this.Machine.heldObject;
            if (output == null)
                return null;

            // get flower data
            SObject.HoneyType type = SObject.HoneyType.Wild;
            string prefix = type.ToString();
            int addedPrice = 0;
            if (this.Location is Farm)
            {
                Crop flower = this.FindCloseFlower(this.Location, this.Tile);
                if (flower != null)
                {
                    string[] flowerData = Game1.objectInformation[flower.indexOfHarvest].Split('/');
                    prefix = flowerData[0];
                    addedPrice = Convert.ToInt32(flowerData[1]) * 2;
                    if (!this.HoneyTypes.TryGetValue(flower.indexOfHarvest, out type))
                        type = SObject.HoneyType.Wild;
                }
            }

            // build object
            SObject result = new SObject(output.parentSheetIndex, output.stack)
            {
                name = $"{prefix} Honey",
                price = output.price + addedPrice,
                honeyType = type
            };

            // yield
            return new TrackedItem(result, onEmpty: this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input needed
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            SObject machine = this.Machine;

            machine.heldObject = new SObject(Vector2.Zero, 340, null, false, true, false, false);
            machine.minutesUntilReady = 2400 - Game1.timeOfDay + 4320;
            machine.readyForHarvest = false;
            machine.showNextIndex = false;
        }

        /// <summary>Get the closest flower within range of the given beehive.</summary>
        /// <param name="location">The bee hive's location.</param>
        /// <param name="tile">The bee hive's tile coordinate.</param>
        /// <remarks>This logic is duplicated from <see cref="Utility.findCloseFlower"/>, but allows for any location instead of being hardcoded to the farm.</remarks>
        private Crop FindCloseFlower(GameLocation location, Vector2 tile)
        {
            // use default game logic if possible
            if (location is Farm)
                return Utility.findCloseFlower(tile);

            // handle flowers in custom locations (e.g. Farm Expansion)
            {
                Queue<Vector2> queue = new Queue<Vector2>();
                HashSet<Vector2> visited = new HashSet<Vector2>();
                queue.Enqueue(tile);
                for (int i = 0; i <= 150 && queue.Count > 0; i++)
                {
                    // check for fully-grown tile on the current tile
                    Vector2 curTile = queue.Dequeue();
                    if (location.terrainFeatures.ContainsKey(curTile) && location.terrainFeatures[curTile] is HoeDirt dirt && dirt.crop != null && dirt.crop.programColored && dirt.crop.currentPhase >= dirt.crop.phaseDays.Count - 1 && !dirt.crop.dead)
                        return dirt.crop;

                    // try surrounding tiles
                    foreach (Vector2 nextTile in Utility.getAdjacentTileLocations(curTile))
                    {
                        if (!visited.Contains(nextTile))
                            queue.Enqueue(nextTile);
                    }
                    visited.Add(curTile);
                }
                return null;
            }
        }
    }
}
