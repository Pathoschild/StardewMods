using System;
using System.Collections.Generic;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Input;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Tests.Common.CommonTests
{
    /// <summary>Unit tests for <see cref="KeyBinding"/>.</summary>
    [TestFixture]
    internal class KeyBindingTests
    {
        /*********
        ** Unit tests
        *********/
        /****
        ** Constructor
        ****/
        /// <summary>Assert the parsed fields when constructed from a simple single-key string.</summary>
        [TestCaseSource(nameof(KeyBindingTests.GetAllButtons))]
        public void Construct_SimpleValue(SButton button)
        {
            // act
            KeyBinding binding = new KeyBinding($"{button}", _ => SButtonState.None, out string[] errors);

            // assert
            Assert.AreEqual(binding.ToString(), $"{button}");
            Assert.IsNotNull(errors, message: "The errors should never be null.");
            Assert.IsEmpty(errors, message: "The input bindings incorrectly reported errors.");
        }

        /// <summary>Assert the parsed fields when constructed from multi-key values.</summary>
        [TestCase("", ExpectedResult = "None")]
        [TestCase("    ", ExpectedResult = "None")]
        [TestCase(null, ExpectedResult = "None")]
        [TestCase("A + B", ExpectedResult = "A + B")]
        [TestCase("A+B", ExpectedResult = "A + B")]
        [TestCase("      A+     B    ", ExpectedResult = "A + B")]
        [TestCase("a +b", ExpectedResult = "A + B")]
        [TestCase("a +b, LEFTcontrol + leftALT + LeftSHifT + delete", ExpectedResult = "A + B, LeftControl + LeftAlt + LeftShift + Delete")]
        public string Construct_MultiValues(string input)
        {
            // act
            KeyBinding binding = new KeyBinding(input, _ => SButtonState.None, out string[] errors);

            // assert
            Assert.IsNotNull(errors, message: "The errors should never be null.");
            Assert.IsEmpty(errors, message: "The input bindings incorrectly reported errors.");
            return binding.ToString();
        }

        /// <summary>Assert invalid values are rejected.</summary>
        [TestCase("+", "None", "Invalid empty button value")]
        [TestCase("A+", "None", "Invalid empty button value")]
        [TestCase("+C", "None", "Invalid empty button value")]
        [TestCase(",", "None", "Invalid empty button set")]
        [TestCase("A,", "A", "Invalid empty button set")]
        [TestCase(",A", "A", "Invalid empty button set")]
        [TestCase("A + B +, C", "C", "Invalid empty button value")]
        [TestCase("A, TotallyInvalid", "A", "Invalid button value 'TotallyInvalid'")]
        [TestCase("A + TotallyInvalid", "None", "Invalid button value 'TotallyInvalid'")]
        public void Construct_InvalidValues(string input, string expectedOutput, string expectedError)
        {
            // act
            KeyBinding binding = new KeyBinding(input, _ => SButtonState.None, out string[] errors);

            // assert
            Assert.AreEqual(expectedOutput, binding.ToString(), message: "The key binding result is incorrect.");
            Assert.IsNotNull(errors, message: "The errors should never be null.");
            Assert.AreEqual(expectedError, string.Join("; ", errors), "The errors don't match the expected ones.");
        }


        /****
        ** GetState
        ****/
        /// <summary>Assert that <see cref="KeyBinding.GetState"/> returns the expected result for a given input state.</summary>
        // single value
        [TestCase("A", "A:Held", ExpectedResult = SButtonState.Held)]
        [TestCase("A", "A:Pressed", ExpectedResult = SButtonState.Pressed)]
        [TestCase("A", "A:Released", ExpectedResult = SButtonState.Released)]
        [TestCase("A", "A:None", ExpectedResult = SButtonState.None)]

        // multiple values
        [TestCase("A + B + C, D", "A:Released, B:None, C:None, D:Pressed", ExpectedResult = SButtonState.Pressed)] // right pressed => pressed
        [TestCase("A + B + C, D", "A:Pressed, B:Held, C:Pressed, D:None", ExpectedResult = SButtonState.Pressed)] // left pressed => pressed
        [TestCase("A + B + C, D", "A:Pressed, B:Pressed, C:Released, D:None", ExpectedResult = SButtonState.None)] // one key released but other keys weren't down last tick => none
        [TestCase("A + B + C, D", "A:Held, B:Held, C:Released, D:None", ExpectedResult = SButtonState.Released)] // all three keys were down last tick and now one is released => released

        // transitive
        [TestCase("A, B", "A: Released, B: Pressed", ExpectedResult = SButtonState.Held)]
        public SButtonState GetState(string input, string stateMap)
        {
            // act
            KeyBinding binding = new KeyBinding(input, key => this.GetStateFromMap(key, stateMap), out string[] errors);

            // assert
            Assert.IsNotNull(errors, message: "The errors should never be null.");
            Assert.IsEmpty(errors, message: "The input bindings incorrectly reported errors.");
            return binding.GetState();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get all defined buttons.</summary>
        private static IEnumerable<SButton> GetAllButtons()
        {
            foreach (SButton button in Enum.GetValues(typeof(SButton)))
                yield return button;
        }

        /// <summary>Get the button state defined by a mapping string.</summary>
        /// <param name="button">The button to check.</param>
        /// <param name="stateMap">The state map.</param>
        private SButtonState GetStateFromMap(SButton button, string stateMap)
        {
            foreach (string rawPair in stateMap.Split(','))
            {
                // parse values
                string[] parts = rawPair.Split(new[] { ':' }, 2);
                if (!Enum.TryParse(parts[0], ignoreCase: true, out SButton curButton))
                    Assert.Fail($"The state map is invalid: unknown button value '{parts[0].Trim()}'");
                if (!Enum.TryParse(parts[1], ignoreCase: true, out SButtonState state))
                    Assert.Fail($"The state map is invalid: unknown state value '{parts[1].Trim()}'");

                // get state
                if (curButton == button)
                    return state;
            }

            Assert.Fail($"The state map doesn't define button value '{button}'.");
            return SButtonState.None;
        }
    }
}
