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
        {
            this.Character = character;
            this.Initialise(character.getName(), null, "NPC");

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
            Sprites.Draw(sprites, this.Character.Portrait, new Rectangle(0, 0, NPC.portrait_width, NPC.portrait_height), (int)position.X, (int)position.Y, (int)size.X, (int)size.Y, Color.White, size.X / NPC.portrait_width);
            return true;
        }
    }
}