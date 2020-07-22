namespace ContentPatcher.Framework
{
    /// <summary>A string value optionally containing tokens.</summary>
    internal interface ITokenString : IContextualInfo
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

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        bool IsSingleTokenOnly { get; }

        /// <summary>The path to the value from the root content file.</summary>
        string Path { get; }
    }
}
