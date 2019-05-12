namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The condition types that can be checked.</summary>
    internal enum ConditionType
    {
        /****
        ** Date
        ****/
        /// <summary>The day of month.</summary>
        Day,

        /// <summary>The name of today's festival (if any), or 'wedding' if the current player is getting married.</summary>
        DayEvent,

        /// <summary>The <see cref="System.DayOfWeek"/> name.</summary>
        DayOfWeek,

        /// <summary>The total number of days played in the current save.</summary>
        DaysPlayed,

        /// <summary>The season name.</summary>
        Season,

        /// <summary>The current year number.</summary>
        Year,

        /// <summary>The current weather.</summary>
        Weather,

        /****
        ** Player
        ****/
        /// <summary>A letter ID or mail flag set for the player.</summary>
        HasFlag,

        /// <summary>A profession ID the player has.</summary>
        HasProfession,

        /// <summary>A letter ID read by the player. Equivalent to <see cref="HasFlag"/>, but only counts letters that have been displayed to the user.</summary>
        HasReadLetter,

        /// <summary>An event ID the player saw.</summary>
        HasSeenEvent,

        /// <summary>The special items in the player's wallet.</summary>
        HasWalletItem,

        /// <summary>Whether the player is the main player or farmhand.</summary>
        IsMainPlayer,

        /// <summary>Whether the player's current location is outdoors.</summary>
        IsOutdoors,

        /// <summary>The name of the player's current location.</summary>
        LocationName,

        /// <summary>The gender of the current player.</summary>
        PlayerGender,

        /// <summary>The name of the current player.</summary>
        PlayerName,

        /// <summary>The preferred pet selected by the player.</summary>
        PreferredPet,

        /// <summary>The current player's level for a skill.</summary>
        SkillLevel,

        /****
        ** Relationships
        ****/
        /// <summary>The current player's number of hearts with the character.</summary>
        Hearts,

        /// <summary>The current player's relationship status with the character (matching <see cref="StardewValley.FriendshipStatus"/>)</summary>
        Relationship,

        /// <summary>The current player's internal spouse name (if any).</summary>
        Spouse,

        /****
        ** World
        ****/
        /// <summary>The farm cave type.</summary>
        FarmCave,

        /// <summary>The upgrade level for the main farmhouse.</summary>
        FarmhouseUpgrade,

        /// <summary>The current farm name.</summary>
        FarmName,

        /// <summary>The current farm type.</summary>
        FarmType,

        /// <summary>Whether all bundles in the community center are completed.</summary>
        IsCommunityCenterComplete,

        /****
        ** Other
        ****/
        /// <summary>An installed mod ID.</summary>
        HasMod,

        /// <summary>Whether a file exists in the content pack's folder.</summary>
        HasFile,

        /// <summary>Whether a given value is non-blank.</summary>
        HasValue,

        /// <summary>The <see cref="StardewValley.LocalizedContentManager.LanguageCode"/> name.</summary>
        Language
    };
}
