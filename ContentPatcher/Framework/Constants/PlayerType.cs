namespace ContentPatcher.Framework.Constants
{
    /// <summary>A multiplayer player type.</summary>
    internal enum PlayerType
    {
        /// <summary>The player hosting the world in multiplayer.</summary>
        HostPlayer,

        /// <summary>The current player, regardless of whether they're the main or secondary player.</summary>
        CurrentPlayer,

        /// <summary>Any player instance. For token values, this is the combined result for all players. In other contexts, this is equivalent to <see cref="CurrentPlayer"/>.</summary>
        AnyPlayer
    }
}
