using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows a list of checkbox values.</summary>
internal class CheckboxListField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The checkbox values to display.</summary>
    protected CheckboxList CheckboxList;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="checkboxList">The checkbox labels and values to display.</param>
    public CheckboxListField(string label, CheckboxList checkboxList)
        : this(label)
    {
        this.CheckboxList = checkboxList;
    }

    /// <inheritdoc />
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        float topOffset = 0;
        float checkboxSize = CommonSprites.Icons.FilledCheckbox.Width * (Game1.pixelZoom / 2);
        float lineHeight = Math.Max(checkboxSize, Game1.smallFont.MeasureString("ABC").Y);
        float checkboxOffset = (lineHeight - checkboxSize) / 2;

        if (this.CheckboxList.Intro != null)
            topOffset += spriteBatch.DrawTextBlock(font, this.CheckboxList.Intro, position, wrapWidth).Y;

        foreach ((bool isChecked, IFormattedText[] label) in this.CheckboxList.Checkboxes)
        {
            // draw icon
            spriteBatch.Draw(
                texture: CommonSprites.Icons.Sheet,
                position: new Vector2(position.X, position.Y + topOffset + checkboxOffset),
                sourceRectangle: isChecked ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: checkboxSize / CommonSprites.Icons.FilledCheckbox.Width,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );

            // draw text
            Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, label, new Vector2(position.X + checkboxSize + 7, position.Y + topOffset), wrapWidth - checkboxSize - 7);

            // update offset
            topOffset += Math.Max(checkboxSize, textSize.Y);
        }

        return new Vector2(wrapWidth, topOffset);
    }


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    protected CheckboxListField(string label)
        : base(label, hasValue: true)
    {
        this.CheckboxList = new CheckboxList();
    }
}
