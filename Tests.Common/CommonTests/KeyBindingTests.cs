using System;
using System.Collections.Generic;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Input;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Tests.Common.CommonTests
{
    /// <summary>Unit tests for <see cref="KeyBinding"/>.</summary>
    [TestFixture]
    internal class InputBindingTests
    {
        /*********
        ** Unit tests
        *********/
        /****
        ** Constructor
        ****/
        /// <summary>Assert the parsed fields when constructed from a simple single-key string.</summary>
        [TestCaseSource(nameof(InputBindingTests.GetAllButtons))]
        public void Construct_SimpleValue(SButton button)
        {
            // act
            KeyBinding binding = new KeyBinding($"{button}", _ => InputStatus.None, out string[] errors);

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
            KeyBinding binding = new KeyBinding(input, _ => InputStatus.None, out string[] errors);

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
            KeyBinding binding = new KeyBinding(input, _ => InputStatus.None, out string[] errors);

            // assert
            Assert.AreEqual(expectedOutput, binding.ToString(), message: "The key binding result is incorrect.");
            Assert.IsNotNull(errors, message: "The errors should never be null.");
            Assert.AreEqual(expectedError, string.Join("; ", errors), "The errors don't match the expected ones.");
        }


        /****
        ** GetStatus
        ****/
        /// <summary>Assert that <see cref="KeyBinding.GetStatus"/> returns the expected result for a given input state.</summary>
        // single value
        [TestCase("A", "A:Held", ExpectedResult = InputStatus.Held)]
        [TestCase("A", "A:Pressed", ExpectedResult = InputStatus.Pressed)]
        [TestCase("A", "A:Released", ExpectedResult = InputStatus.Released)]
        [TestCase("A", "A:None", ExpectedResult = InputStatus.None)]

        // multiple values
        [TestCase("A + B + C, D", "A:Released, B:None, C:None, D:Pressed", ExpectedResult = InputStatus.Pressed)] // right pressed => pressed
        [TestCase("A + B + C, D", "A:Pressed, B:Held, C:Pressed, D:None", ExpectedResult = InputStatus.Pressed)] // left pressed => pressed
        [TestCase("A + B + C, D", "A:Pressed, B:Pressed, C:Released, D:None", ExpectedResult = InputStatus.None)] // one key released but other keys weren't down last tick => none
        [TestCase("A + B + C, D", "A:Held, B:Held, C:Released, D:None", ExpectedResult = InputStatus.Released)] // all three keys were down last tick and now one is released => released

        // transitive
        [TestCase("A, B", "A: Released, B: Pressed", ExpectedResult = InputStatus.Held)]
        public InputStatus GetStatus(string input, string statusMap)
        {
            // act
            KeyBinding binding = new KeyBinding(input, key => this.GetStatusFromMap(key, statusMap), out string[] errors);

            // assert
            Assert.IsNotNull(errors, message: "The errors should never be null.");
            Assert.IsEmpty(errors, message: "The input bindings incorrectly reported errors.");
            return binding.GetStatus();
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

        /// <summary>Get the button status defined by a mapping string.</summary>
        /// <param name="button">The button to check.</param>
        /// <param name="statusMap">The status map.</param>
        private InputStatus GetStatusFromMap(SButton button, string statusMap)
        {
            foreach (string rawPair in statusMap.Split(','))
            {
                // parse values
                string[] parts = rawPair.Split(new[] { ':' }, 2);
                if (!Enum.TryParse(parts[0], ignoreCase: true, out SButton curButton))
                    Assert.Fail($"The status map is invalid: unknown button value '{parts[0].Trim()}'");
                if (!Enum.TryParse(parts[1], ignoreCase: true, out InputStatus status))
                    Assert.Fail($"The status map is invalid: unknown status value '{parts[1].Trim()}'");

                // get state
                if (curButton == button)
                    return status;
            }

            Assert.Fail($"The status map doesn't define button value '{button}'.");
            return InputStatus.None;
        }
    }
}
