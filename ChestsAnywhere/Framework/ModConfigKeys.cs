using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the chest UI.</summary>
        public KeybindList Toggle { get; set; } = new(SButton.B);

        /// <summary>The keys which navigate to the previous chest.</summary>
        public KeybindList PrevChest { get; set; } = KeybindList.Parse($"{SButton.Left}, {SButton.LeftShoulder}");

        /// <summary>The keys which navigate to the next chest.</summary>
        public KeybindList NextChest { get; set; } = KeybindList.Parse($"{SButton.Right}, {SButton.RightShoulder}");

        /// <summary>The keys which navigate to the previous category.</summary>
        public KeybindList PrevCategory { get; set; } = KeybindList.Parse($"{SButton.Up}, {SButton.LeftTrigger}");

        /// <summary>The keys which navigate to the next category.</summary>
        public KeybindList NextCategory { get; set; } = KeybindList.Parse($"{SButton.Down}, {SButton.RightTrigger}");

        /// <summary>The keys which edit the current chest.</summary>
        public KeybindList EditChest { get; set; } = new();

        /// <summary>The keys which sort items in the chest.</summary>
        public KeybindList SortItems { get; set; } = new();

        /// <summary>The keys which, when held, enable scrolling the chest dropdown with the mouse scroll wheel.</summary>
        public KeybindList HoldToMouseWheelScrollChests { get; set; } = new(SButton.LeftControl);

        /// <summary>The keys which, when held, enable scrolling the category dropdown with the mouse scroll wheel.</summary>
        public KeybindList HoldToMouseWheelScrollCategories { get; set; } = new(SButton.LeftAlt);


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
            this.Toggle ??= new KeybindList();
            this.PrevChest ??= new KeybindList();
            this.NextChest ??= new KeybindList();
            this.PrevCategory ??= new KeybindList();
            this.NextCategory ??= new KeybindList();
            this.EditChest ??= new KeybindList();
            this.SortItems ??= new KeybindList();
            this.HoldToMouseWheelScrollChests ??= new KeybindList();
            this.HoldToMouseWheelScrollCategories ??= new KeybindList();
        }
    }
}
