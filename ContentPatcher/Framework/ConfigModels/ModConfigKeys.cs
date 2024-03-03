using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the display of debug information.</summary>
        public KeybindList ToggleDebug { get; set; } = new(SButton.F3);

        /// <summary>The keys which switch to the previous texture.</summary>
        public KeybindList DebugPrevTexture { get; set; } = new(SButton.LeftControl);

        /// <summary>The keys which switch to the next texture.</summary>
        public KeybindList DebugNextTexture { get; set; } = new(SButton.RightControl);


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
            this.ToggleDebug ??= new KeybindList();
            this.DebugPrevTexture ??= new KeybindList();
            this.DebugNextTexture ??= new KeybindList();
        }
    }
}
