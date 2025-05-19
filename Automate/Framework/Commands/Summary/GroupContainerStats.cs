using System;
using System.Collections.Generic;

namespace Pathoschild.Stardew.Automate.Framework.Commands.Summary;

/// <summary>Metadata about containers of the same type within a machine group.</summary>
internal class GroupContainerStats
{
    /*********
    ** Accessors
    *********/
    /// <summary>The container name.</summary>
    public string Name { get; }

    /// <summary>The preference for storing items in these containers.</summary>
    public AutomateContainerPreference StoragePreference { get; }

    /// <summary>The preference for taking items from these containers.</summary>
    public AutomateContainerPreference TakeItemsPreference { get; }

    /// <summary>The number of containers in the group.</summary>
    public int Count { get; }

    /// <summary>The number of slots filled with an item slot.</summary>
    public int FilledSlots { get; }

    /// <summary>The number of empty slots.</summary>
    public int TotalSlots { get; }

    /// <summary>Whether the container is a Junimo chest.</summary>
    public HashSet<string> GlobalInventoryChests { get; } = new(StringComparer.OrdinalIgnoreCase);


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="name">The container name.</param>
    /// <param name="storagePreference">The preference for storing items in these containers.</param>
    /// <param name="takeItemsPreference">The preference for taking items from these containers.</param>
    /// <param name="containers">The containers in the group.</param>
    public GroupContainerStats(string name, AutomateContainerPreference storagePreference, AutomateContainerPreference takeItemsPreference, IEnumerable<IContainer> containers)
    {
        this.Name = name;
        this.StoragePreference = storagePreference;
        this.TakeItemsPreference = takeItemsPreference;

        foreach (IContainer container in containers)
        {
            // only track same global inventory chest once
            if (container.IsGlobalChest)
            {
                if (this.GlobalInventoryChests.Add(container.GlobalInventoryId))
                    continue;
            }

            // track stats
            int filled = container.GetFilled();
            this.Count++;
            this.FilledSlots += filled;
            this.TotalSlots += Math.Max(filled, container.GetCapacity());
        }
    }
}
