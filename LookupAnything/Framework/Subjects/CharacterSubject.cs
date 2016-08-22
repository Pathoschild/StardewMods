using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes an NPC.</summary>
    public class CharacterSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying character.</summary>
        private readonly NPC Character;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="character">The underlying character.</param>
        public CharacterSubject(NPC character)
            : base(character.getName(), null, "NPC")
        {
            this.Character = character;
            if (character.isVillager())
            {
                this.AddCustomFields(
                    new GenericField("Birthday", $"{character.birthday_Season} {character.birthday_Day}"),
                    new GenericField("Can romance", character.datable.ToString().ToLower()),
                    new GenericField("Love interest", character.loveInterest != "null" ? character.loveInterest : "none"),
                    new GenericField("Favourite item", character.getFavoriteItem()?.Name),
                    new GiftTastesForCharacterField("Gift tastes", this.GetGiftTastes(character))
                );
            }

        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size)
        {
            // use character portrait (most NPCs)
            if (this.Character.Portrait != null)
            {
                sprites.DrawBlock(this.Character.Portrait, new Rectangle(0, 0, NPC.portrait_width, NPC.portrait_height), position.X, position.Y, Color.White, size.X / NPC.portrait_width);
                return true;
            }

            // else draw sprite (e.g. for pets)
            this.Character.Sprite.draw(sprites, position, 1, 0, 0, Color.White, scale: size.X / this.Character.Sprite.getWidth());
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get how much an NPC likes receiving each item as a gift.</summary>
        /// <param name="npc">The NPC.</param>
        private IDictionary<GiftTaste, Item[]> GetGiftTastes(NPC npc)
        {
            IDictionary<GiftTaste, List<Item>> tastes = new Dictionary<GiftTaste, List<Item>>();
            foreach (var objectInfo in Game1.objectInformation)
            {
                Object item = new Object(objectInfo.Key, 1);
                if (!npc.canReceiveThisItemAsGift(item))
                    continue;
                try
                {
                    GiftTaste taste = (GiftTaste)npc.getGiftTasteForThisItem(item);
                    if (!tastes.ContainsKey(taste))
                        tastes[taste] = new List<Item>();
                    tastes[taste].Add(item);
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