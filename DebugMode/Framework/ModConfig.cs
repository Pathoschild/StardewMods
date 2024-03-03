using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the key enables the game debug mode.</summary>
        public bool AllowGameDebug { get; set; }

        /// <summary>Allow debug commands which are destructive. A command is considered destructive if it immediately ends the current day, randomizes the player or farmhouse decorations, or crashes the game.</summary>
        public bool AllowDangerousCommands { get; set; }

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();


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
            this.Controls ??= new ModConfigKeys();
        }
    }
}
