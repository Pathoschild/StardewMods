namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token wrapped by Content Patcher to handle global input arguments.</summary>
    /// <typeparam name="TToken">The wrapped token type.</typeparam>
    internal interface IHigherLevelToken<out TToken> : IToken
        where TToken : IToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The wrapped token instance.</summary>
        public TToken Token { get; }
    }
}
