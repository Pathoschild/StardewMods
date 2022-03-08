namespace Pathoschild.Stardew.SkipIntro.Framework
{
    /// <summary>A step in the mod logic.</summary>
    internal enum Stage
    {
        /// <summary>No action needed.</summary>
        None,

        /// <summary>Skip the initial intro.</summary>
        SkipIntro,

        /// <summary>Transition from the title screen to the co-op section.</summary>
        TransitionToCoop,

        /// <summary>Transition from the co-op section to the host screen.</summary>
        TransitionToCoopHost
    }
}
