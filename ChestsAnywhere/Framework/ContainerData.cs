using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        /// <summary>The key prefix with which to store container options in a <see cref="ModDataDictionary"/>.</summary>
        private const string ModDataPrefix = "Pathoschild.ChestsAnywhere";


        /*********
        ** Accessors
        *********/
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
        /// <summary>Construct an instance.</summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used in deserialization for " + nameof(ShippingBinContainer))]
        public ContainerData() { }

        /// <summary>Load contain data for the given item.</summary>
        /// <param name="data">The mod data to read.</param>
        /// <param name="discriminator">A key added to the mod data keys to distinguish different containers in the same mod data.</param>
        public ContainerData(ModDataDictionary data, string discriminator = null)
        {
            string prefix = ContainerData.GetKeyPrefix(discriminator);

            this.IsIgnored = data.ReadField($"{prefix}/{nameof(ContainerData.IsIgnored)}", bool.Parse);
            this.Category = data.ReadField($"{prefix}/{nameof(ContainerData.Category)}");
            this.Name = data.ReadField($"{prefix}/{nameof(ContainerData.Name)}");
            this.Order = data.ReadField($"{prefix}/{nameof(ContainerData.Order)}", int.Parse);
            this.AutomateStoreItems = data.ReadField(AutomateContainerHelper.StoreItemsKey, p => (AutomateContainerPreference)Enum.Parse(typeof(AutomateContainerPreference), p), defaultValue: AutomateContainerPreference.Allow);
            this.AutomateTakeItems = data.ReadField(AutomateContainerHelper.TakeItemsKey, p => (AutomateContainerPreference)Enum.Parse(typeof(AutomateContainerPreference), p), defaultValue: AutomateContainerPreference.Allow);
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
            return string.IsNullOrWhiteSpace(this.Name);
        }

        /// <summary>Reset all container data to the default.</summary>
        public void Reset()
        {
            this.Name = null;
            this.Order = null;
            this.IsIgnored = false;
            this.Category = null;
            this.AutomateTakeItems = AutomateContainerPreference.Allow;
            this.AutomateStoreItems = AutomateContainerPreference.Allow;
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
    }
}
