namespace ContentPatcher.Framework.Conditions
{
    /// <summary>An in-game weather.</summary>
    internal enum Weather
    {
        /// <summary>The weather is sunny (includes hardcoded-sun days like festivals and weddings).</summary>
        Sun,

        /// <summary>Rain is falling, but without lightning.</summary>
        Rain,

        /// <summary>Rain is falling with lightning.</summary>
        Storm,

        /// <summary>Snow is falling.</summary>
        Snow
    }
}
