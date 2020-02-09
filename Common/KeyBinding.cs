using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Input
{
    /// <summary>One or more key bindings, each of which may have multiple key bindings.</summary>
    internal class KeyBinding
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying buttons.</summary>
        private readonly SButton[][] ButtonSets;

        /// <summary>Get the current state for a button.</summary>
        private readonly Func<SButton, InputStatus> GetButtonState;

        /// <summary>The last game tick when <see cref="JustPressedUnique"/> was called.</summary>
        private int LastUniqueTick;

        /// <summary>Whether any keys are bound.</summary>
        private bool HasAnyImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="input">The key binding string containing button codes, with <c>+</c> between buttons for multi-key bindings and commas between alternative bindings.</param>
        /// <param name="getButtonState">Get the current state for a button.</param>
        /// <param name="errors">The errors that occurred while parsing the input, if any.</param>
        public KeyBinding(string input, Func<SButton, InputStatus> getButtonState, out string[] errors)
        {
            this.GetButtonState = getButtonState;

            if (string.IsNullOrWhiteSpace(input))
            {
                this.ButtonSets = new SButton[0][];
                errors = new string[0];
                return;
            }

            List<string> rawErrors = new List<string>();
            List<SButton[]> buttonSets = new List<SButton[]>();
            foreach (string rawSet in input.Split(','))
            {
                if (string.IsNullOrWhiteSpace(rawSet))
                {
                    rawErrors.Add("Invalid empty button set");
                    continue;
                }

                string[] rawButtons = rawSet.Split('+');
                SButton[] buttons = new SButton[rawButtons.Length];
                bool isValid = true;
                for (int i = 0; i < buttons.Length; i++)
                {
                    string rawButton = rawButtons[i].Trim();
                    if (string.IsNullOrWhiteSpace(rawButton))
                    {
                        rawErrors.Add("Invalid empty button value");
                        isValid = false;
                    }
                    else if (!Enum.TryParse(rawButton, ignoreCase: true, out SButton button))
                    {
                        rawErrors.Add($"Invalid button value '{rawButton}'");
                        isValid = false;
                    }
                    else
                        buttons[i] = button;
                }

                if (isValid)
                    buttonSets.Add(buttons);
            }

            this.ButtonSets = buttonSets.ToArray();
            this.HasAnyImpl = this.ButtonSets.Any(set => set.All(p => p != SButton.None));
            errors = rawErrors.Distinct().ToArray();
        }

        /// <summary>Get the overall state of the input bindings.</summary>
        /// <remarks>States are transitive across sets. For example, if set A is 'released' and set B is 'pressed', the combined state is 'held'.</remarks>
        public InputStatus GetStatus()
        {
            bool wasPressed = false;
            bool isPressed = false;

            foreach (SButton[] set in this.ButtonSets)
            {
                switch (this.GetStateFor(set))
                {
                    case InputStatus.Pressed:
                        isPressed = true;
                        break;

                    case InputStatus.Held:
                        wasPressed = true;
                        isPressed = true;
                        break;

                    case InputStatus.Released:
                        wasPressed = true;
                        break;
                }
            }

            if (wasPressed == isPressed)
            {
                return wasPressed
                    ? InputStatus.Held
                    : InputStatus.None;
            }

            return wasPressed
                ? InputStatus.Released
                : InputStatus.Pressed;
        }

        /// <summary>Get whether any of the button sets are pressed.</summary>
        public bool IsDown()
        {
            InputStatus status = this.GetStatus();
            return status == InputStatus.Pressed || status == InputStatus.Held;
        }

        /// <summary>Get whether the input binding was just pressed this tick, *and* this method hasn't been called during the same tick yet. This method is only useful if you check input every tick, since otherwise you may miss the tick where it's pressed; otherwise you should cache the result of <see cref="IsDown"/> and compare.</summary>
        public bool JustPressedUnique()
        {
            if (this.LastUniqueTick == Game1.ticks)
                return false;
            this.LastUniqueTick = Game1.ticks;

            return this.GetStatus() == InputStatus.Pressed;
        }

        /// <summary>Get whether any keys are bound.</summary>
        public bool HasAny()
        {
            return this.HasAnyImpl;
        }

        /// <summary>Get a string representation of the input binding.</summary>
        public override string ToString()
        {
            if (!this.ButtonSets.Any())
                return SButton.None.ToString();

            return string.Join(", ", this.ButtonSets.Select(set => string.Join(" + ", set)));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the state for a button set.</summary>
        /// <param name="buttons">The buttons in the set.</param>
        private InputStatus GetStateFor(SButton[] buttons)
        {
            InputStatus[] states = buttons.Select(this.GetButtonState).Distinct().ToArray();

            // single state
            if (states.Length == 1)
                return states[0];

            // if any key has no state, the whole set wasn't enabled last tick
            if (states.Contains(InputStatus.None))
                return InputStatus.None;

            // mix of held + pressed => pressed
            if (states.All(p => p == InputStatus.Pressed || p == InputStatus.Held))
                return InputStatus.Pressed;

            // mix of held + released => released
            if (states.All(p => p == InputStatus.Held || p == InputStatus.Released))
                return InputStatus.Released;

            // not down last tick or now
            return InputStatus.None;
        }
    }
}
