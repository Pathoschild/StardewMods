using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using StardewValley;

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

            // Birthday: npc.birthday_Season npc.birthday_Day
            // Romance option: npc.datable
            // Love interest: npc.loveInterest
            // Favourite item: npc.getFavoriteItem().Name
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
    }
}