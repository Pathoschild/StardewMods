namespace ContentPatcher.Framework
{
    /// <summary>A string value optionally containing tokens.</summary>
    internal interface ITokenString : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        string Raw { get; }

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        bool HasAnyTokens { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        string Value { get; }
    }
}
