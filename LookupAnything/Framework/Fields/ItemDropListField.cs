using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows a list of item drops.</summary>
internal class ItemDropListField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    protected GameHelper GameHelper;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;

    /// <summary>The possible drops.</summary>
    private readonly Tuple<ItemDropData, Item, SpriteInfo?>[] Drops;

    /// <summary>The text to display before the list, if any.</summary>
    private readonly string? Preface;

    /// <summary>The text to display if there are no items.</summary>
    private readonly string? DefaultText;

    /// <summary>Whether to fade out non-guaranteed drops.</summary>
    private readonly bool FadeNonGuaranteed;

    /// <summary>Whether to cross out non-guaranteed drops.</summary>
    private readonly bool CrossOutNonGuaranteed;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="codex">Provides subject entries.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="drops">The possible drops.</param>
    /// <param name="sort">Whether to sort the resulting list by probability and name.</param>
    /// <param name="fadeNonGuaranteed">Whether to fade out non-guaranteed drops.</param>
    /// <param name="crossOutNonGuaranteed">Whether to cross out non-guaranteed drops.</param>
    /// <param name="defaultText">The text to display if there are no items (or <c>null</c> to hide the field).</param>
    /// <param name="preface">The text to display before the list, if any.</param>
    public ItemDropListField(GameHelper gameHelper, ISubjectRegistry codex, string label, IEnumerable<ItemDropData> drops, bool sort = true, bool fadeNonGuaranteed = false, bool crossOutNonGuaranteed = false, string? defaultText = null, string? preface = null)
        : base(label)
    {
        this.GameHelper = gameHelper;
        this.Codex = codex;

        this.Drops = this.GetEntries(drops, gameHelper).ToArray();
        if (sort)
            this.Drops = [.. this.Drops.OrderByDescending(p => p.Item1.Probability).ThenBy(p => p.Item2.DisplayName)];

        this.HasValue = defaultText != null || this.Drops.Any();
        this.FadeNonGuaranteed = fadeNonGuaranteed;
        this.CrossOutNonGuaranteed = crossOutNonGuaranteed;
        this.Preface = preface;
        this.DefaultText = defaultText;
    }

    /// <inheritdoc />
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        if (!this.Drops.Any())
            return spriteBatch.DrawTextBlock(font, this.DefaultText, position, wrapWidth);

        this.LinkTextAreas.Clear();
        float height = 0;

        // draw preface
        if (!string.IsNullOrWhiteSpace(this.Preface))
        {
            Vector2 prefaceSize = spriteBatch.DrawTextBlock(font, this.Preface, position, wrapWidth);
            height += (int)prefaceSize.Y;
        }

        // list drops
        Vector2 iconSize = new(font.MeasureString("ABC").Y);
        foreach ((ItemDropData drop, Item item, SpriteInfo? sprite) in this.Drops)
        {
            // get data
            bool isGuaranteed = drop.Probability > .99f;
            bool shouldFade = this.FadeNonGuaranteed && !isGuaranteed;
            bool shouldCrossOut = this.CrossOutNonGuaranteed && !isGuaranteed;
            ISubject? subject = this.Codex.GetByEntity(item, null);
            Color textColor = (subject is not null ? Color.Blue : Color.Black) * (shouldFade ? 0.75f : 1f);

            // draw icon
            spriteBatch.DrawSpriteWithin(sprite, position.X, position.Y + height, iconSize, shouldFade ? Color.White * 0.5f : Color.White);

            // draw text
            string text = isGuaranteed ? item.DisplayName : I18n.Generic_PercentChanceOf(percent: CommonHelper.GetFormattedPercentageNumber(drop.Probability), label: item.DisplayName);
            if (drop.MinDrop != drop.MaxDrop)
                text += $" ({I18n.Generic_Range(min: drop.MinDrop, max: drop.MaxDrop)})";
            else if (drop.MinDrop > 1)
                text += $" ({drop.MinDrop})";
            Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(iconSize.X + 5, height + 5), wrapWidth, textColor);

            // track clickable link
            if (subject is not null)
            {
                Rectangle pixelArea = new((int)(position.X + iconSize.X + 5), (int)((int)position.Y + height), (int)textSize.X, (int)textSize.Y);
                this.LinkTextAreas.Add(new LinkTextArea(subject, pixelArea));
            }

            // cross out item if it definitely won't drop
            if (shouldCrossOut)
                spriteBatch.DrawLine(position.X + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), this.FadeNonGuaranteed ? Color.Gray : Color.Black);

            // draw conditions
            if (drop.Conditions != null)
            {
                string conditionText = I18n.ConditionsSummary(conditions: HumanReadableConditionParser.Format(drop.Conditions));
                height += textSize.Y + 5;
                textSize = spriteBatch.DrawTextBlock(font, conditionText, position + new Vector2(iconSize.X + 5, height + 5), wrapWidth);

                if (shouldCrossOut)
                    spriteBatch.DrawLine(position.X + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), this.FadeNonGuaranteed ? Color.Gray : Color.Black);
            }

            height += textSize.Y + 5;
        }

        // return size
        return new Vector2(wrapWidth, height);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the internal drop list entries.</summary>
    /// <param name="drops">The possible drops.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    private IEnumerable<Tuple<ItemDropData, Item, SpriteInfo?>> GetEntries(IEnumerable<ItemDropData> drops, GameHelper gameHelper)
    {
        foreach (ItemDropData drop in drops)
        {
            Item item = ItemRegistry.Create(drop.ItemId);
            SpriteInfo? sprite = gameHelper.GetSprite(item);
            yield return Tuple.Create(drop, item, sprite);
        }
    }
}
