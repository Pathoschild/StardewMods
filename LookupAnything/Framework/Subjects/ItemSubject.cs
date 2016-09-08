using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Fields;
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
        /// <summary>The menu item to render, which may be different from the item that was looked up (e.g. for fences).</summary>
        private readonly Item DisplayItem;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="item">The underlying target.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public ItemSubject(Item item, ObjectContext context, bool knownQuality, Metadata metadata)
        {
            // initialise
            this.DisplayItem = this.GetMenuItem(item);
            this.Initialise(this.DisplayItem.Name, this.GetDescription(this.DisplayItem), this.GetTypeValue(this.DisplayItem));

            // add custom fields
            {
                Object obj = item as Object;

                // get overrides
                bool showInventoryFields = true;
                {
                    ObjectData objData = metadata.GetObject(item, context);
                    if (objData != null)
                    {
                        this.Name = objData.Name ?? this.Name;
                        this.Description = objData.Description ?? this.Description;
                        this.Type = objData.Type ?? this.Type;
                        showInventoryFields = objData.ShowInventoryFields ?? true;
                    }
                }

                // crafting
                if (obj?.heldObject != null)
                    this.AddCustomFields(new GenericField("Crafting", $"{obj.heldObject.Name} " + (obj.minutesUntilReady > 0 ? "in " + GenericField.GetString(TimeSpan.FromMinutes(obj.minutesUntilReady)) : "ready") + "."));

                // item
                if (showInventoryFields)
                {
                    var giftTastes = this.GetGiftTastes(item);
                    this.AddCustomFields(
                        new SaleValueField("Sells for", this.GetSaleValue(item, knownQuality), item.Stack),
                        new ItemGiftTastesField("Loves this", giftTastes, GiftTaste.Love),
                        new ItemGiftTastesField("Likes this", giftTastes, GiftTaste.Like)
                    );
                }

                // fence
                if (item is Fence)
                {
                    Fence fence = (Fence)item;
                    float maxHealth = fence.isGate ? fence.maxHealth * 2 : fence.maxHealth;
                    float health = fence.health / maxHealth;
                    float daysLeft = fence.health * Constant.FenceDecayRate / 60 / 24;
                    this.AddCustomFields(new PercentageBarField("Health", (int)fence.health, (int)maxHealth, Color.Green, Color.Red, $"{Math.Round(health * 100)}% (roughly {Math.Round(daysLeft)} days left)"));
                }

                // recipe
                if (CraftingRecipe.cookingRecipes.ContainsKey(item.Name) || CraftingRecipe.craftingRecipes.ContainsKey(item.Name))
                {
                    CraftingRecipe recipe = new CraftingRecipe(item.Name, CraftingRecipe.cookingRecipes.ContainsKey(item.Name));
                    this.AddCustomFields(new RecipeIngredientsField("Recipe", recipe));
                }
            }
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size)
        {
            Item item = this.DisplayItem;

            // draw stackable object
            if ((item as Object)?.stack > 1)
            {
                Object obj = (Object)item;
                obj = new Object(obj.parentSheetIndex, 1, obj.isRecipe, obj.price, obj.quality) { bigCraftable = obj.bigCraftable }; // remove stack number (doesn't play well with clipped content)
                obj.drawInMenu(sprites, position, 1);
                return true;
            }

            // draw generic item
            item.drawInMenu(sprites, position, 1);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the equivalent menu item for the specified target. (For example, the inventory item matching a fence object.)<</summary>
        /// <param name="item">The target item.</param>
        private Item GetMenuItem(Item item)
        {
            // fence
            if (item is Fence)
            {
                Fence fence = (Fence)item;

                // get equivalent object's sprite ID
                FenceType fenceType = (FenceType)fence.whichType;
                int? spriteID = null;
                if (fence.isGate)
                    spriteID = 325;
                else if (fenceType == FenceType.Wood)
                    spriteID = 322;
                else if (fenceType == FenceType.Stone)
                    spriteID = 323;
                else if (fenceType == FenceType.Iron)
                    spriteID = 324;
                else if (fenceType == FenceType.Hardwood)
                    spriteID = 298;

                // get object
                if (spriteID.HasValue)
                    return new Object(spriteID.Value, 1);
            }

            return item;
        }

        /// <summary>Get the item description.</summary>
        /// <param name="item">The item.</param>
        private string GetDescription(Item item)
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
        private string GetTypeValue(Item item)
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
