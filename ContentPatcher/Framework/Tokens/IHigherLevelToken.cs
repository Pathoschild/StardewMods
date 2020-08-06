namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token wrapped by Content Patcher to handle global input arguments.</summary>
    internal interface IHigherLevelToken : IToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The wrapped token instance.</summary>
        IToken Token { get; }
    }
}
