using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Inventories;

namespace Pathoschild.Stardew.Automate.Framework.Storage;

/// <summary>Temporarily hides a reserve amount of each item stack in an inventory, so code given access to the
/// inventory (like vanilla machine logic) can't reduce a stack below the reserve amount. Restores the hidden
/// amount (minus whatever was consumed in the meantime) when disposed.</summary>
internal class ReservedInventoryScope : IDisposable
{
    /*********
    ** Fields
    *********/
    /// <summary>A no-op scope used when there's no reserve to apply.</summary>
    private static readonly IDisposable NoOp = new ReservedInventoryScope(null!, []);

    /// <summary>The inventory being managed.</summary>
    private readonly IInventory Inventory;

    /// <summary>The items whose stacks were hidden, and the amount hidden for each.</summary>
    private readonly List<(Item Item, int HiddenAmount)> Hidden;


    /*********
    ** Public methods
    *********/
    /// <summary>Hide up to <paramref name="reserveAmount"/> of each item stack in the inventory until the returned scope is disposed.</summary>
    /// <param name="inventory">The inventory to manage.</param>
    /// <param name="reserveAmount">The number of each item to reserve, or zero to disable.</param>
    public static IDisposable Apply(IInventory inventory, int reserveAmount)
    {
        if (reserveAmount <= 0)
            return ReservedInventoryScope.NoOp;

        List<(Item, int)> hidden = new();
        foreach (Item? item in inventory.ToArray())
        {
            if (item is not { Stack: > 0 })
                continue;

            if (item.Stack <= reserveAmount)
            {
                // hide the whole stack
                inventory.Remove(item);
                hidden.Add((item, item.Stack));
            }
            else
            {
                // hide part of the stack
                item.Stack -= reserveAmount;
                hidden.Add((item, reserveAmount));
            }
        }

        return new ReservedInventoryScope(inventory, hidden);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach ((Item item, int hiddenAmount) in this.Hidden)
        {
            if (this.Inventory.Contains(item))
                item.Stack += hiddenAmount;
            else
            {
                item.Stack = hiddenAmount;
                this.Inventory.Add(item);
            }
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="inventory">The inventory being managed.</param>
    /// <param name="hidden">The items whose stacks were hidden, and the amount hidden for each.</param>
    private ReservedInventoryScope(IInventory inventory, List<(Item, int)> hidden)
    {
        this.Inventory = inventory;
        this.Hidden = hidden;
    }
}
