using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A recycling machine that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Recycling Machine'). This differs slightly from the game implementation in that it uses a more random RNG, due to a C# limitation which prevents us from accessing machine info from the cached recipe output functions for use in the RNG seed.</remarks>
    internal class RecyclingMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The RNG to use for randomizing output.</summary>
        private static readonly Random Random = new Random();

        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // trash => coal/iron ore/stone
            new Recipe(
                input: "(O)168",
                inputCount: 1,
                output: _ => ItemRegistry.Create(RecyclingMachine.Random.Choose("(O)382", "380", "390"), RecyclingMachine.Random.Next(1, 4)),
                minutes: 60
            ),

            // driftwood => coal/wood
            new Recipe(
                input: "(O)169",
                inputCount: 1,
                output: _ => ItemRegistry.Create(RecyclingMachine.Random.NextDouble() < 0.25 ? "382" : "388", RecyclingMachine.Random.Next(1, 4)),
                minutes: 60
            ),

            // broken glasses or broken CD => refined quartz
            new Recipe(
                input: item => item.QualifiedItemId is "(O)170" or"(O)171",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)338"),
                minutes: _ => 60
            ),

            // soggy newspaper => cloth/torch
            new Recipe(
                input: "(O)172",
                inputCount: 1,
                output: _ => RecyclingMachine.Random.NextDouble() < 0.1 ? ItemRegistry.Create("(O)428") : new Torch(3),
                minutes: 60
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public RecyclingMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile, machineTypeId: "RecyclingMachine") { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            //Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay + (int)machine.tileLocation.X * 200 + (int)machine.tileLocation.Y);

            if (this.GenericPullRecipe(input, this.Recipes))
            {
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }
            return false;
        }
    }
}
