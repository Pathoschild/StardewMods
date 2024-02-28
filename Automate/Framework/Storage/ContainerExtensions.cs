using StardewValley.Mods;

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
            return container.GetStoragePreference().IsAllowed();
        }

        /// <summary>Get whether this container should be preferred when choosing where to store items.</summary>
        /// <param name="container">The container instance.</param>
        public static bool StoragePreferred(this IContainer container)
        {
            return container.GetStoragePreference().IsPreferred();
        }

        /// <summary>Get the preference for storing items in this container.</summary>
        /// <param name="container">The container instance.</param>
        public static AutomateContainerPreference GetStoragePreference(this IContainer container)
        {
            return container.ModData.ReadPreferenceField(AutomateContainerHelper.StoreItemsKey);
        }

        /// <summary>Get whether items can be retrieved from this container.</summary>
        /// <param name="container">The container instance.</param>
        public static bool TakingItemsAllowed(this IContainer container)
        {
            return container.GetTakingItemsPreference().IsAllowed();
        }

        /// <summary>Get whether this container should be preferred when choosing where to retrieve items.</summary>
        /// <param name="container">The container instance.</param>
        public static bool TakingItemsPreferred(this IContainer container)
        {
            return container.GetTakingItemsPreference().IsPreferred();
        }

        /// <summary>Get the preference for taking items from this container.</summary>
        /// <param name="container">The container instance.</param>
        public static AutomateContainerPreference GetTakingItemsPreference(this IContainer container)
        {
            return container.ModData.ReadPreferenceField(AutomateContainerHelper.TakeItemsKey);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Read a container preference from a mod data dictionary.</summary>
        /// <param name="data">The mod data dictionary to read.</param>
        /// <param name="key">The dictionary key to read.</param>
        private static AutomateContainerPreference ReadPreferenceField(this ModDataDictionary data, string key)
        {
            data.TryGetValue(key, out string rawValue);
            return rawValue switch
            {
                nameof(AutomateContainerPreference.Allow) => AutomateContainerPreference.Allow,
                nameof(AutomateContainerPreference.Prefer) => AutomateContainerPreference.Prefer,
                nameof(AutomateContainerPreference.Disable) => AutomateContainerPreference.Disable,
                _ => AutomateContainerPreference.Allow
            };
        }
    }
}
