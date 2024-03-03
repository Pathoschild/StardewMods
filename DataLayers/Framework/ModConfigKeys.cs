using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the data layer overlay.</summary>
        public KeybindList ToggleLayer { get; set; } = new(SButton.F2);

        /// <summary>The keys which cycle backwards through data layers.</summary>
        public KeybindList PrevLayer { get; set; } = KeybindList.Parse($"{SButton.LeftControl}, {SButton.LeftShoulder}");

        /// <summary>The keys which cycle forward through data layers.</summary>
        public KeybindList NextLayer { get; set; } = KeybindList.Parse($"{SButton.RightControl}, {SButton.RightShoulder}");


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
            this.ToggleLayer ??= new KeybindList();
            this.PrevLayer ??= new KeybindList();
            this.NextLayer ??= new KeybindList();
        }
    }
}
