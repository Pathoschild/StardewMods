using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a Stardew Valley object.</summary>
    public class ObjectSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying object.</summary>
        private readonly Object Obj;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The Stardew Valley object.</param>
        public ObjectSubject(Object obj)
            : base(obj.Name, obj.getDescription(), ObjectSubject.GetTypeValue(obj))
        {
            this.Obj = obj;
            this.AddCustomFields(
                new SaleValueField("Sells for", this.GetSaleValue(obj)),
                new GiftTastesForItemField("Gift tastes", this.GetGiftTastes(obj), GiftTaste.Love, GiftTaste.Like)
            );
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size)
        {
            this.Obj.drawInMenu(sprites, position, 1);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the object type.</summary>
        /// <param name="obj">The object instance.</param>
        private static string GetTypeValue(Object obj)
        {
            string type = obj.getCategoryName();
            if (string.IsNullOrWhiteSpace(type))
                type = obj.type;
            return type;
        }

        /// <summary>Get the possible sale values for an item.</summary>
        /// <param name="item">The item.</param>
        private IDictionary<ItemQuality, int> GetSaleValue(Object item)
        {
            return new Dictionary<ItemQuality, int>
            {
                [ItemQuality.Low] = new Object(item.parentSheetIndex, 1).sellToStorePrice(),
                [ItemQuality.Medium] = new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.Medium).sellToStorePrice(),
                [ItemQuality.High] = new Object(item.parentSheetIndex, 1, quality: (int)ItemQuality.High).sellToStorePrice()
            };
        }

        /// <summary>Get how much each NPC likes receiving an item as a gift.</summary>
        /// <param name="item">The potential gift item.</param>
        private IDictionary<GiftTaste, NPC[]> GetGiftTastes(Object item)
        {
            IDictionary<GiftTaste, List<NPC>> tastes = new Dictionary<GiftTaste, List<NPC>>();
            foreach (NPC npc in Game1.locations.SelectMany(l => l.characters))
            {
                if (!npc.canReceiveThisItemAsGift(item))
                    continue;
                try
                {
                    GiftTaste taste = (GiftTaste)npc.getGiftTasteForThisItem(item);
                    if (!tastes.ContainsKey(taste))
                        tastes[taste] = new List<NPC>();
                    tastes[taste].Add(npc);
                }
                catch (Exception)
                {
                    // some NPCs (e.g. dog) claim to allow gifts, but crash if you check their preference
                }
            }
            return tastes.ToDictionary(p => p.Key, p => p.Value.ToArray());
        }
    }
}