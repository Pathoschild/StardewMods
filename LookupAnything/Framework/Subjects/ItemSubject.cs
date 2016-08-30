using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Fields;
using Pathoschild.LookupAnything.Framework.Metadata;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a Stardew Valley item.</summary>
    public class ItemSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying item.</summary>
        private readonly Item Item;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="item">The underlying item.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="overrides">Provides metadata that's not available from the game data directly.</param>
        public ItemSubject(Item item, bool knownQuality, OverrideData overrides)
            : base(item.Name, ItemSubject.GetDescription(item), ItemSubject.GetTypeValue(item))
        {
            this.Item = item;
            Object obj = item as Object;

            // basic info
            this.Name = item.Name;
            bool showInventoryFields = true;
            {
                ObjectOverride @override = overrides.GetOverrides(item);
                if (@override != null)
                {
                    this.Name = @override.Name ?? this.Name;
                    this.Description = @override.Description ?? this.Description;
                    this.Type = @override.Type ?? this.Type;
                    showInventoryFields = @override.ShowInventoryFields ?? showInventoryFields;
                }
            }

            // crafting
            if (obj?.heldObject != null)
                this.AddCustomFields(new GenericField("Crafting", $"{obj.heldObject.Name} " + (obj.minutesUntilReady > 0 ? "in " + GenericField.GetString(TimeSpan.FromMinutes(obj.minutesUntilReady)) : "ready") + "."));

            // item
            if (showInventoryFields)
            {
                this.AddCustomFields(
                    new SaleValueField("Sells for", this.GetSaleValue(item, knownQuality), item.Stack),
                    new GiftTastesForItemField("Gift tastes", this.GetGiftTastes(item), GiftTaste.Love, GiftTaste.Like)
                );
            }

            // recipe
            if (CraftingRecipe.cookingRecipes.ContainsKey(item.Name) || CraftingRecipe.craftingRecipes.ContainsKey(item.Name))
            {
                CraftingRecipe recipe = new CraftingRecipe(item.Name, CraftingRecipe.cookingRecipes.ContainsKey(item.Name));
                this.AddCustomFields(new RecipeIngredientsField("Recipe", recipe));
            }
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size)
        {
            this.Item.drawInMenu(sprites, position, 1);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the item description.</summary>
        /// <param name="item">The item.</param>
        private static string GetDescription(Item item)
        {
            try
            {
                return item.getDescription();
            }
            catch (KeyNotFoundException)
            {
                return null; // e.g. incubator
            }
        }

        /// <summary>Get the item type.</summary>
        /// <param name="item">The item.</param>
        private static string GetTypeValue(Item item)
        {
            string type = item.getCategoryName();
            if (string.IsNullOrWhiteSpace(type) && item is Object)
                type = ((Object)item).type;
            return type;
        }

        /// <summary>Get the possible sale values for an item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="qualityIsKnown">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        private IDictionary<ItemQuality, int> GetSaleValue(Item item, bool qualityIsKnown)
        {
            Func<Item, int> getPrice = i =>
            {
                int price = (i as Object)?.sellToStorePrice() ?? i.salePrice();
                return price > 0 ? price : 0;
            };

            // single quality
            if (!GameHelper.CanHaveQuality(item) || qualityIsKnown)
            {
                ItemQuality quality = qualityIsKnown && item is Object
                    ? (ItemQuality)((Object)item).quality
                    : ItemQuality.Low;

                return new Dictionary<ItemQuality, int> { [quality] = getPrice(item) };
            }

            // multiple qualities
            return new Dictionary<ItemQuality, int>
            {
                [ItemQuality.Low] = getPrice(new Object(item.parentSheetIndex, 1)),
                [ItemQuality.Medium] = getPrice(new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.Medium)),
                [ItemQuality.High] = getPrice(new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.High))
            };
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The potential gift item.</param>
        private IDictionary<GiftTaste, string[]> GetGiftTastes(Item item)
        {
            return GameHelper.GetGiftTastes(item)
                .GroupBy(p => p.Value, p => p.Key.getName())
                .ToDictionary(p => p.Key, p => p.ToArray());
        }
    }
}
