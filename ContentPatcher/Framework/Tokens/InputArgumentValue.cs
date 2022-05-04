namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A parsed input argument for a token.</summary>
    internal class InputArgumentValue : IInputArgumentValue
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw input argument value.</summary>
        public string Raw { get; }

        /// <summary>The input argument value split into its component values.</summary>
        public string[] Parsed { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw input argument value.</param>
        /// <param name="parsed">The input argument value split into its component values.</param>
        public InputArgumentValue(string raw, string[] parsed)
        {
            this.Raw = raw;
            this.Parsed = parsed;
        }
    }
}
