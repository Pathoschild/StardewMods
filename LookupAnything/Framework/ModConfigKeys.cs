using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the lookup UI for something under the cursor.</summary>
        public KeybindList ToggleLookup { get; set; } = new(SButton.F1);

        /// <summary>The keys which toggle the search UI.</summary>
        public KeybindList ToggleSearch { get; set; } = KeybindList.Parse($"{SButton.LeftShift} + {SButton.F1}");

        /// <summary>The keys which scroll up long content by a few lines.</summary>
        public KeybindList ScrollUp { get; set; } = new(SButton.Up);

        /// <summary>The keys which scroll down long content by a few lines.</summary>
        public KeybindList ScrollDown { get; set; } = new(SButton.Down);

        /// <summary>The keys which scroll up long content by a full page.</summary>
        public KeybindList PageUp { get; set; } = new(SButton.PageUp);

        /// <summary>The keys which scroll down long content by a full page.</summary>
        public KeybindList PageDown { get; set; } = new(SButton.PageDown);

        /// <summary>The keys which toggle the display of debug information.</summary>
        public KeybindList ToggleDebug { get; set; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
        public void OnDeserialized(StreamingContext context)
        {
            this.ToggleLookup ??= new KeybindList();
            this.ToggleSearch ??= new KeybindList();
            this.ScrollUp ??= new KeybindList();
            this.ScrollDown ??= new KeybindList();
            this.PageUp ??= new KeybindList();
            this.PageDown ??= new KeybindList();
            this.ToggleDebug ??= new KeybindList();
        }
    }
}
