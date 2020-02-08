namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>How Automate should use a container.</summary>
    internal enum ContainerAutomatePreference
    {
        /// <summary>Allow input/output for this container.</summary>
        Allow,

        /// <summary>Prefer input/output for this container over non-preferred containers.</summary>
        Prefer,

        /// <summary>Disable input/output for this container.</summary>
        Disable
    }

    /// <summary>Provides extension methods for <see cref="ContainerAutomatePreference"/>.</summary>
    internal static class ContainerAutomatePreferenceExtensions
    {
        /// <summary>Get whether IO is enabled.</summary>
        /// <param name="preference">The IO preference.</param>
        public static bool IsAllowed(this ContainerAutomatePreference preference)
        {
            return preference != ContainerAutomatePreference.Disable;
        }

        /// <summary>Get whether IO is preferred.</summary>
        /// <param name="preference">The IO preference.</param>
        public static bool IsPreferred(this ContainerAutomatePreference preference)
        {
            return preference == ContainerAutomatePreference.Prefer;
        }
    }
}
