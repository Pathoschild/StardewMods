using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which summon the tractor.</summary>
        public KeybindList SummonTractor { get; set; } = new(SButton.Back);

        /// <summary>The keys which return the tractor to its home.</summary>
        public KeybindList DismissTractor { get; set; } = new(SButton.Back);

        /// <summary>The keys which activate the tractor when held, or none to activate automatically.</summary>
        public KeybindList HoldToActivate { get; set; } = new();


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
            this.SummonTractor ??= new KeybindList();
            this.DismissTractor ??= new KeybindList();
            this.HoldToActivate ??= new KeybindList();
        }
    }
}
