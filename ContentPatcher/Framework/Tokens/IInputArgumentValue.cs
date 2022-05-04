namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A parsed input argument for a token.</summary>
    internal interface IInputArgumentValue
    {
        /// <summary>The raw input argument value.</summary>
        string Raw { get; }

        /// <summary>The input argument value split into its component values.</summary>
        string[] Parsed { get; }
    }
}
