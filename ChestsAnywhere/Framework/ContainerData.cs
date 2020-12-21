using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The model for custom container data.</summary>
    internal class ContainerData
    {
        /*********
        ** Fields
        *********/
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";

        /// <summary>The key prefix with which to store container options in a <see cref="ModDataDictionary"/>.</summary>
        private const string ModDataPrefix = "Pathoschild.ChestsAnywhere";


        /*********
        ** Accessors
        *********/
        /// <summary>The default name for the container type, if any.</summary>
        public string DefaultInternalName { get; set; }

        /// <summary>The display name.</summary>
        public string Name { get; set; }

        /// <summary>The category name (if any).</summary>
        public string Category { get; set; }

        /// <summary>Whether the container should be ignored.</summary>
        public bool IsIgnored { get; set; }

        /// <summary>Whether Automate should take items from this container.</summary>
        public AutomateContainerPreference AutomateTakeItems { get; set; } = AutomateContainerPreference.Allow;

        /// <summary>Whether Automate should put items in this container.</summary>
        public AutomateContainerPreference AutomateStoreItems { get; set; } = AutomateContainerPreference.Allow;

        /// <summary>The sort value (if any).</summary>
        public int? Order { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used in deserialization for " + nameof(ShippingBinContainer))]
        public ContainerData() { }

        /// <summary>Construct an empty instance.</summary>
        /// <param name="defaultInternalName">The game's default name for the container, if any.</param>
        public ContainerData(string defaultInternalName)
        {
            this.DefaultInternalName = defaultInternalName;
        }

        /// <summary>Load contain data for the given item.</summary>
        /// <param name="data">The mod data to read.</param>
        /// <param name="defaultInternalName">The game's default name for the container, if any.</param>
        /// <param name="discriminator">A key added to the mod data keys to distinguish different containers in the same mod data.</param>
        public static ContainerData FromModData(ModDataDictionary data, string defaultInternalName, string discriminator = null)
        {
            string prefix = ContainerData.GetKeyPrefix(discriminator);

            return new ContainerData(defaultInternalName)
            {
                IsIgnored = data.ReadField($"{prefix}/{nameof(ContainerData.IsIgnored)}", bool.Parse),
                Category = data.ReadField($"{prefix}/{nameof(ContainerData.Category)}"),
                Name = data.ReadField($"{prefix}/{nameof(ContainerData.Name)}"),
                Order = data.ReadField($"{prefix}/{nameof(ContainerData.Order)}", int.Parse),
                AutomateStoreItems = data.ReadField(AutomateContainerHelper.StoreItemsKey, p => (AutomateContainerPreference)Enum.Parse(typeof(AutomateContainerPreference), p), defaultValue: AutomateContainerPreference.Allow),
                AutomateTakeItems = data.ReadField(AutomateContainerHelper.TakeItemsKey, p => (AutomateContainerPreference)Enum.Parse(typeof(AutomateContainerPreference), p), defaultValue: AutomateContainerPreference.Allow)
            };
        }

        /// <summary>Save the container data to the given mod data.</summary>
        /// <param name="data">The mod data.</param>
        /// <param name="discriminator">A key added to the mod data keys to distinguish different containers in the same mod data.</param>
        public void ToModData(ModDataDictionary data, string discriminator = null)
        {
            string prefix = ContainerData.GetKeyPrefix(discriminator);

            data
                .WriteField($"{prefix}/{nameof(ContainerData.IsIgnored)}", this.IsIgnored ? "true" : null)
                .WriteField($"{prefix}/{nameof(ContainerData.Category)}", this.Category)
                .WriteField($"{prefix}/{nameof(ContainerData.Name)}", !this.HasDefaultDisplayName() ? this.Name : null)
                .WriteField($"{prefix}/{nameof(ContainerData.Order)}", this.Order != 0 ? this.Order?.ToString(CultureInfo.InvariantCulture) : null)
                .WriteField(AutomateContainerHelper.StoreItemsKey, this.AutomateStoreItems != AutomateContainerPreference.Allow ? this.AutomateStoreItems.ToString() : null)
                .WriteField(AutomateContainerHelper.TakeItemsKey, this.AutomateTakeItems != AutomateContainerPreference.Allow ? this.AutomateTakeItems.ToString() : null);
        }

        /// <summary>Whether the container has the default display name.</summary>
        public bool HasDefaultDisplayName()
        {
            return string.IsNullOrWhiteSpace(this.Name) || this.Name == this.DefaultInternalName;
        }

        /// <summary>Get whether the container has any non-default data.</summary>
        public bool HasData()
        {
            return
                !this.HasDefaultDisplayName()
                || (this.Order.HasValue && this.Order != 0)
                || this.IsIgnored
                || !string.IsNullOrWhiteSpace(this.Category)
                || this.AutomateTakeItems != AutomateContainerPreference.Allow
                || this.AutomateStoreItems != AutomateContainerPreference.Allow;
        }

        /// <summary>Reset all container data to the default.</summary>
        public void Reset()
        {
            this.Name = this.DefaultInternalName;
            this.Order = null;
            this.IsIgnored = false;
            this.Category = null;
            this.AutomateTakeItems = AutomateContainerPreference.Allow;
            this.AutomateStoreItems = AutomateContainerPreference.Allow;
        }

        /// <summary>Load contain data for the given item, migrating it if needed.</summary>
        /// <param name="item">The item whose container data to load.</param>
        /// <param name="defaultDisplayName">The default display name.</param>
        public static void MigrateLegacyData(Item item, string defaultDisplayName)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Name == defaultDisplayName)
                return;

            ContainerData data = ContainerData.ParseLegacyDataFromName(item.Name, defaultDisplayName);
            item.Name = defaultDisplayName;
            data.ToModData(item.modData);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the prefix for fields in a <see cref="ModDataDictionary"/> field.</summary>
        /// <param name="discriminator">A key added to the mod data keys to distinguish different containers in the same mod data.</param>
        private static string GetKeyPrefix(string discriminator)
        {
            string prefix = ContainerData.ModDataPrefix;

            if (discriminator != null)
                prefix = $"{prefix}/{discriminator}";

            return prefix;
        }

        /// <summary>Parse a serialized name string.</summary>
        /// <param name="name">The serialized name string.</param>
        /// <param name="defaultDisplayName">The default display name for the container.</param>
        private static ContainerData ParseLegacyDataFromName(string name, string defaultDisplayName)
        {
            // construct instance
            ContainerData data = new ContainerData(defaultDisplayName);
            if (string.IsNullOrWhiteSpace(name) || name == defaultDisplayName)
                return data;

            // read |tags|
            foreach (Match match in Regex.Matches(name, ContainerData.TagGroupPattern))
            {
                string tag = match.Groups[1].Value;

                // ignore
                if (tag.ToLower() == "ignore")
                    data.IsIgnored = true;

                // category
                else if (tag.ToLower().StartsWith("cat:"))
                    data.Category = tag.Substring(4).Trim();

                // order
                else if (int.TryParse(tag, out int order))
                    data.Order = order;

                // Automate options
                else if (tag.ToLower() == "automate:no-store")
                    data.AutomateStoreItems = AutomateContainerPreference.Disable;
                else if (tag.ToLower() == "automate:prefer-store")
                    data.AutomateStoreItems = AutomateContainerPreference.Prefer;
                else if (tag.ToLower() == "automate:no-take")
                    data.AutomateTakeItems = AutomateContainerPreference.Disable;
                else if (tag.ToLower() == "automate:prefer-take")
                    data.AutomateTakeItems = AutomateContainerPreference.Prefer;
            }

            // read display name
            name = Regex.Replace(name, ContainerData.TagGroupPattern, "").Trim();
            data.Name = !string.IsNullOrWhiteSpace(name) && name != defaultDisplayName
                ? name
                : defaultDisplayName;

            return data;
        }
    }
}
