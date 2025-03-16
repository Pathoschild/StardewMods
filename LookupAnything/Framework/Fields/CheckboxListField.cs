using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which lists checkbox values.</summary>
internal class CheckboxListField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The checkbox values to display.</summary>
    protected CheckboxList[] CheckboxLists;

    /// <summary>The size of each checkbox to draw.</summary>
    protected readonly float CheckboxSize;

    /// <summary>The height of one line of the checkbox list.</summary>
    protected readonly float LineHeight;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="checkboxLists">The checkbox lists to display.</param>
    public CheckboxListField(string label, params CheckboxList[] checkboxLists)
        : this(label)
    {
        this.CheckboxLists = checkboxLists;
    }

    /// <inheritdoc />
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        float topOffset = 0;

        foreach (CheckboxList checkboxList in this.CheckboxLists)
            topOffset += this.DrawCheckboxList(checkboxList, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth).Y;

        return new Vector2(wrapWidth, topOffset - this.LineHeight);
    }


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    protected CheckboxListField(string label)
        : base(label, hasValue: true)
    {
        this.CheckboxLists = [];
        this.CheckboxSize = CommonSprites.Icons.FilledCheckbox.Width * (Game1.pixelZoom / 2);
        this.LineHeight = Math.Max(this.CheckboxSize, Game1.smallFont.MeasureString("ABC").Y);
    }

    /// <summary>Draw a checkbox list.</summary>
    /// <param name="checkboxList">The checkbox list info to render.</param>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="font">The recommended font.</param>
    /// <param name="position">The position at which to draw.</param>
    /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
    /// <returns>Returns the drawn dimensions.</returns>
    protected Vector2 DrawCheckboxList(CheckboxList checkboxList, SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        float topOffset = 0;
        float checkboxSize = this.CheckboxSize;
        float leftOffset = 0;
        float checkboxOffsetY = (this.LineHeight - checkboxSize) / 2;

        if (checkboxList.Intro != null)
        {
            topOffset += this.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, checkboxList.Intro.Text, Color.Black, checkboxList.Intro.Icon, new Vector2(this.LineHeight)).Y;
            leftOffset = 14;
        }

        foreach (Checkbox checkbox in checkboxList.Checkboxes)
        {
            // draw icon
            spriteBatch.Draw(
                texture: CommonSprites.Icons.Sheet,
                position: new Vector2(position.X + leftOffset, position.Y + topOffset + checkboxOffsetY),
                sourceRectangle: checkbox.IsChecked ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: checkboxSize / CommonSprites.Icons.FilledCheckbox.Width,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );

            // draw text
            Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, checkbox.Text, new Vector2(position.X + leftOffset + checkboxSize + 7, position.Y + topOffset), wrapWidth - checkboxSize - 7);

            // update offset for next checkbox
            topOffset += Math.Max(checkboxSize, textSize.Y);
        }

        return new Vector2(position.X, topOffset + this.LineHeight);
    }
}
