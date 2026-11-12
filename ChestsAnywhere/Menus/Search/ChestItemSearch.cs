using System;
using Pathoschild.Stardew.Common.Integrations.StardewAccess;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Search;

/// <summary>Find and focus an item in a chest inventory.</summary>
internal static class ChestItemSearch
{
    private static readonly PerScreen<(ItemGrabMenu Menu, string Text)?> PendingJump = new();

    public static void QueueJump(ItemGrabMenu menu, string text)
    {
        PendingJump.Value = (menu, text);
    }

    public static void CompletePendingJump(ItemGrabMenu menu, StardewAccessIntegration access)
    {
        if (PendingJump.Value is not { } pending)
            return;

        PendingJump.Value = null;
        if (ReferenceEquals(menu, pending.Menu) && ReferenceEquals(Game1.activeClickableMenu, menu))
            Jump(menu, pending.Text, -1, access);
    }

    public static bool Jump(ItemGrabMenu menu, string text, int afterIndex, StardewAccessIntegration access)
    {
        InventoryMenu inventory = menu.ItemsToGrabMenu;
        int count = inventory.inventory.Count;
        if (!string.IsNullOrWhiteSpace(text))
        {
            for (int offset = 1; offset <= count; offset++)
            {
                int index = (afterIndex + offset) % count;
                if (index >= inventory.actualInventory.Count || inventory.actualInventory[index] is not Item item || !item.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                    continue;

                menu.currentlySnappedComponent = inventory.inventory[index];
                menu.snapCursorToCurrentSnappedComponent();
                string details = access.GetDetailsOfItem(item, giveExtraDetails: true);
                access.Say(I18n.Access_ItemSearchSlot(item: details, position: index + 1, total: count), true);
                access.PrevMenuQueryText = $"{details}:{index}";
                return true;
            }
        }

        access.Say(I18n.Access_ItemSearchNoMatch(), true);
        return false;
    }
}
