using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows a list of linked item names with icons.</summary>
internal class ItemIconListField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The text to show before the item list, if any.</summary>
    private readonly string? IntroText;

    /// <summary>The items to draw.</summary>
    private readonly Tuple<Item, SpriteInfo?>[] Items;

    /// <summary>Get the name to show for an item, or <c>null</c> to use the item's display name.</summary>
    private readonly Func<Item, string?>? FormatItemName;

    /// <summary>Whether to draw the stack size on the item icon.</summary>
    private readonly bool ShowStackSize;

    /// <summary>The pixel indent to apply before each entry in the list.</summary>
    private readonly int IconIndent;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="items">The items to display.</param>
    /// <param name="showStackSize">Whether to draw the stack size on the item icon.</param>
    /// <param name="introText">The text to show before the item list, if any.</param>
    /// <param name="formatItemName">Get the name to show for an item, or <c>null</c> to use the item's display name.</param>
    /// <param name="iconIndent">The pixel indent to apply before each entry in the list.</param>
    public ItemIconListField(GameHelper gameHelper, string label, IEnumerable<Item?>? items, bool showStackSize, string? introText = null, Func<Item, string?>? formatItemName = null, int iconIndent = 0)
        : base(label, hasValue: items != null)
    {
        this.Items = items?.WhereNotNull().Select(item => Tuple.Create(item, gameHelper.GetSprite(item))).ToArray() ?? [];
        this.HasValue = this.Items.Any();
        this.ShowStackSize = showStackSize;
        this.IntroText = introText;
        this.FormatItemName = formatItemName;
        this.IconIndent = iconIndent;
    }

    /// <inheritdoc />
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth, float visibleHeight)
    {
        // get icon size
        float textHeight = font.MeasureString("ABC").Y;
        Vector2 iconSize = new Vector2(textHeight);

        // draw intro
        int topOffset = 0;
        if (this.IntroText != null)
        {
            Vector2 textSize = spriteBatch.DrawTextBlock(font, this.IntroText, position, wrapWidth);
            topOffset += (int)Math.Max(iconSize.Y, textSize.Y) + 10;
        }

        // draw list
        const int padding = 5;
        int leftOffset = this.IconIndent;
        foreach ((Item item, SpriteInfo? sprite) in this.Items)
        {
            if (topOffset > visibleHeight)
                break;

            // draw icon
            spriteBatch.DrawSpriteWithin(sprite, position.X + leftOffset, position.Y + topOffset, iconSize);
            if (this.ShowStackSize && item.Stack > 1)
            {
                float scale = 2f; //sprite.SourceRectangle.Width / iconSize.X;
                Vector2 sizePos = position + new Vector2(leftOffset + iconSize.X - Utility.getWidthOfTinyDigitString(item.Stack, scale), iconSize.Y + topOffset - 6f * scale);
                Utility.drawTinyDigits(item.Stack, spriteBatch, sizePos, scale: scale, layerDepth: 1f, Color.White);
            }

            // draw text
            string displayText = this.FormatItemName?.Invoke(item) ?? item.DisplayName;
            Vector2 textSize = spriteBatch.DrawTextBlock(font, displayText, position + new Vector2(leftOffset + iconSize.X + padding, topOffset), wrapWidth);

            topOffset += (int)Math.Max(iconSize.Y, textSize.Y) + padding;
        }

        // return size
        return new Vector2(wrapWidth, topOffset + padding);
    }
}
