using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field shown as an extended property in the lookup UI.</summary>
internal interface ICustomField
{
    /*********
    ** Accessors
    *********/
    /// <summary>A short field label.</summary>
    string Label { get; }

    /// <summary>The field value.</summary>
    IFormattedText[]? Value { get; }

    /// <summary>Whether the field should be displayed.</summary>
    bool HasValue { get; }

    /// <summary>Whether the field may have a clickable link returned by <see cref="TryGetLinkAt"/>.</summary>
    bool MayHaveLinks { get; }

    /// <summary>If the field is currently collapsed, the link to click to expand it.</summary>
    LinkField? ExpandLink { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Draw the value (or return <c>null</c> to render the <see cref="Value"/> using the default format).</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="font">The recommended font.</param>
    /// <param name="position">The position at which to draw.</param>
    /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
    /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="Value"/> using the default format.</returns>
    Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth);

    /// <summary>Get the subject that should be opened when the player clicks a position within the field, if any.</summary>
    /// <param name="x">The X pixel position to check.</param>
    /// <param name="y">The Y pixel position to check.</param>
    /// <param name="subject">The subject linked from the given position.</param>
    /// <returns>Returns whether a subject was found at the given position.</returns>
    bool TryGetLinkAt(int x, int y, [NotNullWhen(true)] out ISubject? subject);
}
