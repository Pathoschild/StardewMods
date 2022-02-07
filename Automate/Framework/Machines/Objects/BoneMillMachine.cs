using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A bone mill that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Bone Mill').</remarks>
    internal class BoneMillMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
            new Dictionary<string, int>
            {
                ["(O)579"] = 1, // Prehistoric Scapula
                ["(O)580"] = 1, // Prehistoric Tibia
                ["(O)581"] = 1, // Prehistoric Skull
                ["(O)582"] = 1, // Skeletal Hand
                ["(O)583"] = 1, // Prehistoric Rib
                ["(O)584"] = 1, // Prehistoric Vertebra
                ["(O)585"] = 1, // Skeletal Tail
                ["(O)586"] = 1, // Nautilus Fossil
                ["(O)587"] = 1, // Amphibian Fossil
                ["(O)588"] = 1, // Palm Fossil
                ["(O)589"] = 1, // Trilobyte
                ["(O)820"] = 1, // Fossilized Skull
                ["(O)821"] = 1, // Fossilized Spine
                ["(O)822"] = 1, // Fossilized Tail
                ["(O)823"] = 1, // Fossilized Leg
                ["(O)824"] = 1, // Fossilized Ribs
                ["(O)825"] = 1, // Snake Skull
                ["(O)826"] = 1, // Snake Vertebrae
                ["(O)827"] = 1, // Mummified Bat
                ["(O)828"] = 1, // Mummified Frog

                ["(O)881"] = 5 // Bone Fragment
            }
            .Select(pair => (IRecipe)new Recipe(
                input: pair.Key,
                inputCount: pair.Value,
                output: BoneMillMachine.GetRecipeOutput,
                minutes: 240
            ))
            .ToArray();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public BoneMillMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return this.GenericPullRecipe(input, this.Recipes);
        }

        /// <summary>Get the output for a recipe.</summary>
        /// <param name="item">The input ingredient.</param>
        private static SObject GetRecipeOutput(Item item)
        {
            string? which = null;
            int howMany = -1;

            switch (Game1.random.Next(4))
            {
                case 0:
                    which = "466";
                    howMany = 3;
                    break;
                case 1:
                    which = "465";
                    howMany = 5;
                    break;
                case 2:
                    which = "369";
                    howMany = 10;
                    break;
                case 3:
                    which = "805";
                    howMany = 5;
                    break;
            }

            if (Game1.random.NextDouble() < 0.1)
                howMany *= 2;

            return ItemRegistry.Create<SObject>(which, howMany);
        }
    }
}
