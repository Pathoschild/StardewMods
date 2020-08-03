using System;

namespace Pathoschild.Stardew.Automate.Framework.Storage
{
    /// <summary>Provides extensions for <see cref="IContainer"/> instances.</summary>
    internal static class ContainerExtensions
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether items can be stored in this container.</summary>
        /// <param name="container">The container instance.</param>
        public static bool StorageAllowed(this IContainer container)
        {
            return !container.HasTag("automate:no-store");
        }

        /// <summary>Get whether this container should be preferred when choosing where to store items.</summary>
        /// <param name="container">The container instance.</param>
        public static bool StoragePreferred(this IContainer container)
        {
            return container.StorageAllowed() && container.HasTag("automate:prefer-store");
        }

        /// <summary>Get whether items can be retrieved from this container.</summary>
        /// <param name="container">The container instance.</param>
        public static bool TakingItemsAllowed(this IContainer container)
        {
            return !container.HasTag("automate:no-take");
        }

        /// <summary>Get whether this container should be preferred when choosing where to retrieve items.</summary>
        /// <param name="container">The container instance.</param>
        public static bool TakingItemsPreferred(this IContainer container)
        {
            return container.TakingItemsAllowed() && container.HasTag("automate:prefer-take");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the container name contains a given tag.</summary>
        /// <param name="container">The container instance.</param>
        /// <param name="tag">The tag to check, excluding the '|' delimiters.</param>
        private static bool HasTag(this IContainer container, string tag)
        {
            return container.Name?.IndexOf($"|{tag}|", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
