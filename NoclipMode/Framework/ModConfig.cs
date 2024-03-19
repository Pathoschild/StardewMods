using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.NoclipMode.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle noclip mode.</summary>
        public KeybindList ToggleKey { get; set; } = new(SButton.F11);

        /// <summary>Whether to show a confirmation message when noclip is enabled.</summary>
        public bool ShowEnabledMessage { get; set; } = true;

        /// <summary>Whether to show a confirmation message when noclip is disabled.</summary>
        public bool ShowDisabledMessage { get; set; } = false;


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
            this.ToggleKey ??= new KeybindList();
        }
    }
}
