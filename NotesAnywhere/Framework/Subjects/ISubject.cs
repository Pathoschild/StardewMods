using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.NotesAnywhere.Framework.Subjects
{
    /// <summary>Provides metadata about something in the game.</summary>
    public interface ISubject
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The display name.</summary>
        string Name { get; }

        /// <summary>The item description (if applicable).</summary>
        string Description { get; }

        /// <summary>The item type (if applicable).</summary>
        string Type { get; }

        /// <summary>The item price when sold or shipped (if applicable).</summary>
        int? SalePrice { get; }

        /// <summary>How much each NPC likes receiving this item as a gift (if applicable).</summary>
        IDictionary<GiftTaste, NPC[]> GiftTastes { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size);
    }
}