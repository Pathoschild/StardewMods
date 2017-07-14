using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>The input mapping configuration.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    internal class InputMapConfiguration<T>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The control which toggles the lookup UI for something under the cursor.</summary>
        public T ToggleLookup { get; set; }

        /// <summary>The control which toggles the lookup UI for something in front of the player.</summary>
        public T ToggleLookupInFrontOfPlayer { get; set; }

        /// <summary>The control which scrolls up long content.</summary>
        public T ScrollUp { get; set; }

        /// <summary>The control which scrolls down long content.</summary>
        public T ScrollDown { get; set; }

        /// <summary>Toggle the display of debug information.</summary>
        public T ToggleDebug { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the specified key is valid.</summary>
        /// <param name="key">The key to check.</param>
        public bool IsValidKey(T key)
        {
            return key != null && !key.Equals(default(T));
        }

        /// <summary>Get whether any keys are configured.</summary>
        public bool HasAny()
        {
            return new[] { this.ToggleLookup, this.ToggleLookupInFrontOfPlayer, this.ScrollUp, this.ScrollDown, this.ToggleDebug }.Any(this.IsValidKey);
        }
    }
}
