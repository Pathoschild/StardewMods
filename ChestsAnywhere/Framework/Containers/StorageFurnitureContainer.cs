using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;

/// <summary>A storage container for a <see cref="StorageFurniture"/> instance (e.g. a dresser).</summary>
internal class StorageFurnitureContainer : IContainer
{
    /*********
    ** Fields
    *********/
    /// <summary>The in-game storage furniture.</summary>
    internal readonly StorageFurniture Furniture;

    /// <summary>The categories accepted by a dresser.</summary>
    private static HashSet<int> DresserCategories = null!; // set when the class is first constructed


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public IList<Item?> Inventory => this.Furniture.heldItems;

    /// <inheritdoc />
    public ContainerData Data { get; }

    /// <inheritdoc />
    public bool CanConfigureAutomateStore => false; // Automate doesn't support storage containers

    /// <inheritdoc />
    public bool CanConfigureAutomateTake => false; // Automate doesn't support storage containers


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="furniture">The in-game storage furniture.</param>
    [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = $"{nameof(StorageFurnitureContainer.DresserCategories)} is only non-null after the first instance is constructed.")]
    public StorageFurnitureContainer(StorageFurniture furniture)
    {
        this.Furniture = furniture;
        this.Data = new ContainerData(furniture.modData);

        StorageFurnitureContainer.DresserCategories ??= [.. new ShopMenu("Dresser", new List<ISalable>()).categoriesToSellHere];
    }

    /// <inheritdoc />
    public bool CanAcceptItem(Item item)
    {
        return StorageFurnitureContainer.DresserCategories.Contains(item.Category);
    }

    /// <inheritdoc />
    public bool HasContextTag(string tag)
    {
        return this.Furniture.HasContextTag(tag);
    }

    /// <inheritdoc />
    public bool IsSameAs(IContainer? container)
    {
        return
            container is not null
            && this.IsSameAs(container.Inventory);
    }

    /// <inheritdoc />
    public bool IsSameAs(IList<Item?>? inventory)
    {
        return
            inventory is not null
            && object.ReferenceEquals(this.Inventory, inventory);
    }

    /// <inheritdoc />
    public IClickableMenu OpenMenu()
    {
        this.Furniture.ShowShopMenu();

        if (Game1.activeClickableMenu is ShopMenu shopMenu)
            shopMenu.source = this.Furniture;

        return Game1.activeClickableMenu;
    }

    /// <inheritdoc />
    public void SaveData()
    {
        this.Data.ToModData(this.Furniture.modData);
    }

    /// <inheritdoc />
    public bool TryGetIcon([NotNullWhen(true)] out Texture2D? texture, out Rectangle sourceRect, out float scale)
    {
        ParsedItemData parsedItemData = ItemRegistry.GetData(this.Furniture.QualifiedItemId);
        texture = parsedItemData.GetTexture();
        sourceRect = this.Furniture.sourceRect.Value;

        scale = Game1.pixelZoom * (32f / Math.Max(sourceRect.Height, sourceRect.Width));
        if (scale > Game1.pixelZoom)
            scale = Game1.pixelZoom;

        return true;
    }
}
