namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The condition types that can be checked.</summary>
    internal enum ConditionType
    {
        /****
        ** Tokenisable basic conditions
        ****/
        /// <summary>The day of month.</summary>
        Day,

        /// <summary>The <see cref="System.DayOfWeek"/> name.</summary>
        DayOfWeek,

        /// <summary>The <see cref="StardewValley.LocalizedContentManager.LanguageCode"/> name.</summary>
        Language,

        /// <summary>The season name.</summary>
        Season,

        /// <summary>The current weather.</summary>
        Weather,

        /****
        ** Other basic conditions
        ****/
        /// <summary>The name of today's festival (if any), or 'wedding' if the current player is getting married.</summary>
        DayEvent,

        /// <summary>A letter ID or mail flag set for the player.</summary>
        HasFlag,

        /// <summary>An installed mod ID.</summary>
        HasMod,

        /// <summary>An event ID the player saw.</summary>
        HasSeenEvent,

        /// <summary>The current player's internal spouse name (if any).</summary>
        Spouse,

        /****
        ** Multi-part conditions
        ****/
        /// <summary>The current player's number of hearts with the character.</summary>
        Hearts,

        /// <summary>The current player's relationship status with the character (matching <see cref="StardewValley.FriendshipStatus"/>)</summary>
        Relationship
    };
}
