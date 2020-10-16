namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>How Automate should use a container.</summary>
    internal enum AutomateContainerPreference
    {
        /// <summary>Allow input/output for this container.</summary>
        Allow,

        /// <summary>Prefer input/output for this container over non-preferred containers.</summary>
        Prefer,

        /// <summary>Disable input/output for this container.</summary>
        Disable
    }

    /// <summary>Provides extension methods for <see cref="AutomateContainerPreference"/>.</summary>
    internal static class AutomateContainerPreferenceHelper
    {
        /// <summary>Get whether IO is enabled.</summary>
        /// <param name="preference">The IO preference.</param>
        public static bool IsAllowed(this AutomateContainerPreference preference)
        {
            return preference != AutomateContainerPreference.Disable;
        }

        /// <summary>Get whether IO is preferred.</summary>
        /// <param name="preference">The IO preference.</param>
        public static bool IsPreferred(this AutomateContainerPreference preference)
        {
            return preference == AutomateContainerPreference.Prefer;
        }
    }
}
