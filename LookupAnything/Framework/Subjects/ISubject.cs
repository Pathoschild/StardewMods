using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Fields;

namespace Pathoschild.LookupAnything.Framework.Subjects
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

        /// <summary>The custom fields to display for this subject (if any).</summary>
        ICustomField[] CustomFields { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size);
    }
}