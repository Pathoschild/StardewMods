namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The conditions that can be checked.</summary>
    public enum ConditionKey
    {
        /// <summary>The day of month.</summary>
        Day,

        /// <summary>The <see cref="System.DayOfWeek"/> name.</summary>
        DayOfWeek,

        /// <summary>The <see cref="StardewValley.LocalizedContentManager.LanguageCode"/> name.</summary>
        Language,

        /// <summary>The season name.</summary>
        Season,

        /// <summary>The current weather.</summary>
        Weather
    };
}
