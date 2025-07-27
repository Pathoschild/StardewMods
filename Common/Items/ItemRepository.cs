using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Common.Items;

/// <summary>Provides methods for searching and constructing items.</summary>
/// <remarks>This is copied from the SMAPI source code and should be kept in sync with it.</remarks>
internal class ItemRepository
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get all spawnable items.</summary>
    /// <param name="onlyType">Only include items for the given <see cref="IItemDataDefinition.Identifier"/>.</param>
    /// <param name="includeVariants">Whether to include flavored variants like "Sunflower Honey".</param>
    [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = $"{nameof(ItemRepository.TryCreate)} invokes the lambda immediately.")]
    public IEnumerable<SearchableItem> GetAll(string? onlyType = null, bool includeVariants = true)
    {
        //
        //
        // Be careful about closure variable capture here!
        //
        // SearchableItem stores the Func<Item> to create new instances later. Loop variables passed into the
        // function will be captured, so every func in the loop will use the value from the last iteration. Use the
        // TryCreate(type, id, entity => item) form to avoid the issue, or create a local variable to pass in.
        //
        //

        IEnumerable<SearchableItem?> GetAllRaw()
        {
            // get from item data definitions
            foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
            {
                if (onlyType != null && itemType.Identifier != onlyType)
                    continue;

                switch (itemType.Identifier)
                {
                    // objects
                    case "(O)":
                        {
                            ObjectDataDefinition objectDataDefinition = (ObjectDataDefinition)ItemRegistry.GetTypeDefinition(ItemRegistry.type_object);

                            foreach (string id in itemType.GetAllIds())
                            {
                                // base item
                                SearchableItem? result = this.TryCreate(itemType.Identifier, id, p => ItemRegistry.Create(itemType.Identifier + p.Id));

                                // ring
                                if (result?.Item is Ring)
                                    yield return result;

                                // journal scraps
                                else if (result?.QualifiedItemId == "(O)842")
                                {
                                    foreach (SearchableItem? journalScrap in this.GetSecretNotes(itemType, isJournalScrap: true))
                                        yield return journalScrap;
                                }

                                // secret notes
                                else if (result?.QualifiedItemId == "(O)79")
                                {
                                    foreach (SearchableItem? secretNote in this.GetSecretNotes(itemType, isJournalScrap: false))
                                        yield return secretNote;
                                }

                                // object
                                else
                                {
                                    switch (result?.QualifiedItemId)
                                    {
                                        // honey should be "Wild Honey" when there's no ingredient, instead of the base Honey item
                                        case "(O)340":
                                            yield return this.TryCreate(itemType.Identifier, result.Id, _ => objectDataDefinition.CreateFlavoredHoney(null));
                                            break;

                                        // don't return placeholder items
                                        case "(O)DriedFruit":
                                        case "(O)DriedMushrooms":
                                        case "(O)SmokedFish":
                                        case "(O)SpecificBait":
                                            break;

                                        default:
                                            if (result != null)
                                                yield return result;
                                            break;
                                    }

                                    if (includeVariants)
                                    {
                                        foreach (SearchableItem? variant in this.GetFlavoredObjectVariants(objectDataDefinition, result?.Item as SObject, itemType))
                                            yield return variant;
                                    }
                                }
                            }
                        }
                        break;

                    // no special handling needed
                    default:
                        foreach (string id in itemType.GetAllIds())
                            yield return this.TryCreate(itemType.Identifier, id, p => ItemRegistry.Create(itemType.Identifier + p.Id));
                        break;
                }
            }
        }

        return (
            from item in GetAllRaw()
            where item != null
            select item
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the individual secret note or journal scrap items.</summary>
    /// <param name="itemType">The object data definition.</param>
    /// <param name="isJournalScrap">Whether to get journal scraps.</param>
    /// <remarks>Derived from <see cref="GameLocation.tryToCreateUnseenSecretNote"/>.</remarks>
    private IEnumerable<SearchableItem?> GetSecretNotes(IItemDataDefinition itemType, bool isJournalScrap)
    {
        // get base item ID
        string baseId = isJournalScrap ? "842" : "79";

        // get secret note IDs
        var ids = this
            .TryLoad(() => DataLoader.SecretNotes(Game1.content))
            .Keys
            .Where(isJournalScrap
                ? id => (id >= GameLocation.JOURNAL_INDEX)
                : id => (id < GameLocation.JOURNAL_INDEX)
            )
            .Select<int, int>(isJournalScrap
                ? id => (id - GameLocation.JOURNAL_INDEX)
                : id => id
            );

        // build items
        foreach (int i in ids)
        {
            int id = i; // avoid closure capture

            yield return this.TryCreate(itemType.Identifier, $"{baseId}/{id}", _ =>
            {
                Item note = ItemRegistry.Create(itemType.Identifier + baseId);
                note.Name = $"{note.Name} #{id}";
                return note;
            });
        }
    }

    /// <summary>Get flavored variants of a base item (like Blueberry Wine for Blueberry), if any.</summary>
    /// <param name="objectDataDefinition">The item data definition for object items.</param>
    /// <param name="item">A sample of the base item.</param>
    /// <param name="itemType">The object data definition.</param>
    private IEnumerable<SearchableItem?> GetFlavoredObjectVariants(ObjectDataDefinition objectDataDefinition, SObject? item, IItemDataDefinition itemType)
    {
        if (item is null)
            yield break;

        string id = item.ItemId;

        // by category
        bool createdJuice = false;
        bool createdPickle = false;
        switch (item.Category)
        {
            // fish
            case SObject.FishCategory:
                yield return this.TryCreate(itemType.Identifier, $"SmokedFish/{id}", _ => objectDataDefinition.CreateFlavoredSmokedFish(item));
                yield return this.TryCreate(itemType.Identifier, $"SpecificBait/{id}", _ => objectDataDefinition.CreateFlavoredBait(item));
                break;

            // fruit products
            case SObject.FruitsCategory:
                yield return this.TryCreate(itemType.Identifier, $"348/{id}", _ => objectDataDefinition.CreateFlavoredWine(item));
                yield return this.TryCreate(itemType.Identifier, $"344/{id}", _ => objectDataDefinition.CreateFlavoredJelly(item));
                if (item.QualifiedItemId != "(O)398") // raisins are their own item
                    yield return this.TryCreate(itemType.Identifier, $"398/{id}", _ => objectDataDefinition.CreateFlavoredDriedFruit(item));
                break;

            // greens
            case SObject.GreensCategory:
                yield return this.TryCreate(itemType.Identifier, $"342/{id}", _ => objectDataDefinition.CreateFlavoredPickle(item));
                createdPickle = true;

                if (item.Edibility > 0 && !item.HasContextTag("edible_mushroom"))
                {
                    yield return this.TryCreate(itemType.Identifier, $"350/{id}", _ => objectDataDefinition.CreateFlavoredJuice(item));
                    createdJuice = true;
                }

                break;

            // vegetable products
            case SObject.VegetableCategory:
                yield return this.TryCreate(itemType.Identifier, $"350/{id}", _ => objectDataDefinition.CreateFlavoredJuice(item));
                createdJuice = true;

                yield return this.TryCreate(itemType.Identifier, $"342/{id}", _ => objectDataDefinition.CreateFlavoredPickle(item));
                createdPickle = true;
                break;

            // flower honey
            case SObject.flowersCategory:
                yield return this.TryCreate(itemType.Identifier, $"340/{id}", _ => objectDataDefinition.CreateFlavoredHoney(item));
                break;
        }

        // by context tag
        {
            // roe + aged roe
            if (item.HasContextTag("fish_has_roe"))
            {
                SearchableItem? roe = this.TryCreate(itemType.Identifier, $"812/{item.ItemId}", _ => objectDataDefinition.CreateFlavoredRoe(item));
                yield return roe;

                if (roe?.Item is SObject roeObj && item.QualifiedItemId != "(O)698") // skip aged sturgeon roe (which is a separate caviar item)
                    yield return this.TryCreate(itemType.Identifier, $"447/{item.ItemId}", _ => objectDataDefinition.CreateFlavoredAgedRoe(roeObj));
            }

            // juice
            if (!createdJuice && item.HasContextTag("keg_juice"))
                yield return this.TryCreate(itemType.Identifier, $"350/{id}", _ => objectDataDefinition.CreateFlavoredJuice(item));

            // pickles
            if (!createdPickle && item.HasContextTag("preserves_pickle") && item.Category is not (SObject.GreensCategory or SObject.VegetableCategory))
                yield return this.TryCreate(itemType.Identifier, $"342/{id}", _ => objectDataDefinition.CreateFlavoredPickle(item));

            // dried mushrooms
            if (item.HasContextTag("edible_mushroom"))
                yield return this.TryCreate(itemType.Identifier, $"DriedMushrooms/{id}", _ => objectDataDefinition.CreateFlavoredDriedMushroom(item));
        }
    }

    /// <summary>Try to load a data asset, and return empty data if it's invalid.</summary>
    /// <typeparam name="TAsset">The asset type.</typeparam>
    /// <param name="load">A callback which loads the asset.</param>
    private TAsset TryLoad<TAsset>(Func<TAsset> load)
        where TAsset : new()
    {
        try
        {
            return load();
        }
        catch (ContentLoadException)
        {
            // generally due to a player incorrectly replacing a data file with an XNB mod
            return new TAsset();
        }
    }

    /// <summary>Create a searchable item if valid.</summary>
    /// <param name="type">The item type.</param>
    /// <param name="key">The locally unique item key.</param>
    /// <param name="createItem">Create an item instance.</param>
    private SearchableItem? TryCreate(string type, string key, Func<SearchableItem, Item> createItem)
    {
        try
        {
            SearchableItem item = new(type, key, createItem);
            item.Item.getDescription(); // force-load item data, so it crashes here if it's invalid

            if (item.Item.Name is null or "Error Item")
                return null;

            return item;
        }
        catch
        {
            return null; // if some item data is invalid, just don't include it
        }
    }
}
