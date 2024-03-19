using System.Diagnostics.CodeAnalysis;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The condition types that can be checked.</summary>
    internal enum ConditionType
    {
        /****
        ** Date and weather
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
        /// <summary>An active quest in the player's quest log.</summary>
        HasActiveQuest,

        /// <summary>The fish IDs caught by the player.</summary>
        HasCaughtFish,

        /// <summary>The cooking recipes known by the player.</summary>
        HasCookingRecipe,

        /// <summary>The crafting recipes known by the player.</summary>
        HasCraftingRecipe,

        /// <summary>A conversation topic ID set for the player.</summary>
        HasConversationTopic,

        /// <summary>A letter ID or mail flag set for the player.</summary>
        HasFlag,

        /// <summary>A profession ID the player has.</summary>
        HasProfession,

        /// <summary>A letter ID read by the player. Equivalent to <see cref="HasFlag"/>, but only counts letters that have been displayed to the user.</summary>
        HasReadLetter,

        /// <summary>An event ID the player saw.</summary>
        HasSeenEvent,

        /// <summary>The player's daily luck.</summary>
        DailyLuck,

        /// <summary>The response IDs for the player's answers to question dialogues.</summary>
        HasDialogueAnswer,

        /// <summary>The special items in the player's wallet.</summary>
        HasWalletItem,

        /// <summary>Whether the player is the main player or farmhand.</summary>
        IsMainPlayer,

        /// <summary>Whether the player's current location is outdoors.</summary>
        IsOutdoors,

        /// <summary>The general world area.</summary>
        LocationContext,

        /// <summary>The name of the player's current location.</summary>
        LocationName,

        /// <summary>The unique ID of the player who owns the location, if applicable.</summary>
        LocationOwnerId,

        /// <summary>The unique name of the player's current location. This differs from <see cref="LocationName"/> for constructed building interiors.</summary>
        LocationUniqueName,

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
        /// <summary>The names of the player's children.</summary>
        ChildNames,

        /// <summary>The genders of the player's children.</summary>
        ChildGenders,

        /// <summary>The current player's number of hearts with the character.</summary>
        Hearts,

        /// <summary>The current player's relationship status with the character (matching <see cref="StardewValley.FriendshipStatus"/>)</summary>
        Relationship,

        /// <summary>The current player's internal roommate name (if any).</summary>
        Roommate,

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

        /// <summary>Whether all bundles in the JojaMart are completed.</summary>
        IsJojaMartComplete,

        /// <summary>NPCs and players whose relationships have an active adoption or pregnancy.</summary>
        HavingChild,

        /// <summary>NPCs and players who are currently pregnant.</summary>
        Pregnant,

        /// <summary>The in-game time of day.</summary>
        Time,

        /****
        ** Number manipulation
        ****/
        /// <summary>The number of values contained in the input.</summary>
        Count,

        /// <summary>A dynamic query expression.</summary>
        Query,

        /// <summary>A list of numeric values based on the specified min/max values.</summary>
        Range,

        /// <summary>A rounded approximation of the input.</summary>
        Round,

        /****
        ** String manipulation
        ****/
        /// <summary>A token which transforms its input text to lowercase.</summary>
        Lowercase,

        /// <summary>Combine any number of input tokens and values into one.</summary>
        Merge,

        /// <summary>Get part of a file/asset path.</summary>
        PathPart,

        /// <summary>A random value selected from the given input.</summary>
        Random,

        /// <summary>A token which returns a string representation of its input text.</summary>
        Render,

        /// <summary>A token which transforms its input text to uppercase.</summary>
        Uppercase,

        /****
        ** Metadata
        ****/
        /// <summary>A token which returns the first input argument which matches an existing file in the content pack.</summary>
        FirstValidFile,

        /// <summary>An installed mod ID.</summary>
        HasMod,

        /// <summary>Whether a file exists in the content pack's folder.</summary>
        HasFile,

        /// <summary>Whether a given value is non-blank.</summary>
        HasValue,

        /// <summary>A translation from the mod's <c>i18n</c> folder.</summary>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named to match the Stardew Valley modding convention and folder name.")]
        I18n,

        /// <summary>The <see cref="StardewValley.LocalizedContentManager.LanguageCode"/> name.</summary>
        Language,

        /// <summary>The current content pack's unique manifest ID.</summary>
        ModId,

        /****
        ** Specialized
        ****/
        /// <summary>A token which returns the absolute path for a file in the content pack's folder.</summary>
        AbsoluteFilePath,

        /// <summary>A token which normalizes an asset name into the form expected by the game.</summary>
        FormatAssetName,

        /// <summary>A token which gets the internal asset key for a content pack file, to allow loading it directly through a content manager.</summary>
        InternalAssetKey,

        /****
        ** Patch-specific
        ****/
        /// <summary>The current patch's FromFile value.</summary>
        FromFile,

        /// <summary>The current patch's full target value.</summary>
        Target,

        /// <summary>The filename portion of the current patch's target value.</summary>
        TargetWithoutPath,

        /// <summary>The path portion of the current patch's target value, without the filename.</summary>
        TargetPathOnly
    }
}
