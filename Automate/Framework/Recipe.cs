using System;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Describes a generic recipe based on item input and output.</summary>
    internal class Recipe : IRecipe
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Matches items that can be used as input.</summary>
        public Func<Item, bool> Input { get; }

        /// <summary>The number of inputs needed.</summary>
        public int InputCount { get; }

        /// <summary>The output to generate (given an input).</summary>
        public Func<Item, Item> Output { get; }

        /// <summary>The time needed to prepare an output (given an input).</summary>
        public Func<Item, int> Minutes { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="input">The qualified input item ID or category ID.</param>
        /// <param name="inputCount">The number of inputs needed.</param>
        /// <param name="output">The output to generate (given an input).</param>
        /// <param name="minutes">The time needed to prepare an output.</param>
        public Recipe(string input, int inputCount, Func<Item, Item> output, int minutes)
            : this(input, inputCount, output, _ => minutes) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="input">The qualified input item ID or category ID.</param>
        /// <param name="inputCount">The number of inputs needed.</param>
        /// <param name="output">The output to generate (given an input).</param>
        /// <param name="minutes">The time needed to prepare an output (given an input).</param>
        public Recipe(string input, int inputCount, Func<Item, Item> output, Func<Item, int> minutes)
            : this(item => Recipe.MatchesInputId(item, input), inputCount, output, minutes) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="input">The input to accept.</param>
        /// <param name="inputCount">The number of inputs needed.</param>
        /// <param name="output">The output to generate (given an input).</param>
        /// <param name="minutes">The time needed to prepare an output (given an input).</param>
        public Recipe(Func<Item, bool> input, int inputCount, Func<Item, Item> output, Func<Item, int> minutes)
        {
            this.Input = input;
            this.InputCount = inputCount;
            this.Output = output;
            this.Minutes = minutes;
        }

        /// <summary>Get whether the recipe can accept a given item as input (regardless of stack size).</summary>
        /// <param name="stack">The item to check.</param>
        public bool AcceptsInput(ITrackedStack stack)
        {
            return this.Input(stack.Sample);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether an item matches the given ID.</summary>
        /// <param name="item">The item to check.</param>
        /// <param name="inputId">The input item or category ID.</param>
        private static bool MatchesInputId(Item item, string inputId)
        {
            return
                item.QualifiedItemId == inputId
                || (int.TryParse(inputId, out int category) && item.Category == category);
        }
    }
}
