using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FishPonds;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows a list of drops for a fish pond grouped by population gate.</summary>
internal class FishPondDropsField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    protected GameHelper GameHelper;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;

    /// <summary>The possible drops.</summary>
    private readonly FishPondDrop[] Drops;

    /// <summary>The text to display before the list, if any.</summary>
    private readonly string Preface;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="codex">Provides subject entries.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="currentPopulation">The current population for showing unlocked drops.</param>
    /// <param name="data">The fish pond data.</param>
    /// <param name="fish">The fish in the fish pond, if any.</param>
    /// <param name="preface">The text to display before the list, if any.</param>
    public FishPondDropsField(GameHelper gameHelper, ISubjectRegistry codex, string label, int currentPopulation, FishPondData data, SObject? fish, string preface)
        : base(label)
    {
        this.GameHelper = gameHelper;
        this.Codex = codex;
        this.Drops = this.GetEntries(currentPopulation, data, fish, gameHelper).OrderBy(drop => drop.Precedence).ThenByDescending(drop => drop.MinPopulation).ToArray();
        this.HasValue = this.Drops.Any();
        this.Preface = preface;
    }

    /// <inheritdoc />
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        this.LinkTextAreas.Clear();
        float height = 0;

        // draw preface
        if (!string.IsNullOrWhiteSpace(this.Preface))
        {
            Vector2 prefaceSize = spriteBatch.DrawTextBlock(font, this.Preface, position, wrapWidth);
            height += (int)prefaceSize.Y;
        }

        // calculate sizes
        float checkboxSize = CommonSprites.Icons.FilledCheckbox.Width * (Game1.pixelZoom / 2);
        float lineHeight = Math.Max(checkboxSize, Game1.smallFont.MeasureString("ABC").Y);
        float checkboxOffset = (lineHeight - checkboxSize) / 2;
        float outerIndent = checkboxSize + 7;
        float innerIndent = outerIndent * 2;

        // list drops
        Vector2 iconSize = new Vector2(font.MeasureString("ABC").Y);
        int lastGroup = -1;
        bool isPrevDropGuaranteed = false;
        foreach (FishPondDrop drop in this.Drops)
        {
            bool disabled = !drop.IsUnlocked || isPrevDropGuaranteed;

            // draw group checkbox + requirement
            if (lastGroup != drop.MinPopulation)
            {
                lastGroup = drop.MinPopulation;

                spriteBatch.Draw(
                    texture: CommonSprites.Icons.Sheet,
                    position: new Vector2(position.X + outerIndent, position.Y + height + checkboxOffset),
                    sourceRectangle: drop.IsUnlocked ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox,
                    color: Color.White * (disabled ? 0.5f : 1f),
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: checkboxSize / CommonSprites.Icons.FilledCheckbox.Width,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
                Vector2 textSize = spriteBatch.DrawTextBlock(
                    font: Game1.smallFont,
                    text: I18n.Building_FishPond_Drops_MinFish(count: drop.MinPopulation),
                    position: new Vector2(position.X + outerIndent + checkboxSize + 7, position.Y + height),
                    wrapWidth: wrapWidth - checkboxSize - 7,
                    color: disabled ? Color.Gray : Color.Black
                );

                // cross out if it's guaranteed not to drop
                if (isPrevDropGuaranteed)
                    spriteBatch.DrawLine(position.X + outerIndent + checkboxSize + 7, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), Color.Gray);

                height += Math.Max(checkboxSize, textSize.Y);
            }

            // draw drop
            bool isGuaranteed = drop.Probability > .99f;
            {
                ISubject? subject = this.Codex.GetByEntity(drop.SampleItem, null);
                Color textColor = (subject is not null ? Color.Blue : Color.Black) * (disabled ? 0.75f : 1f);

                // draw icon
                spriteBatch.DrawSpriteWithin(drop.Sprite, position.X + innerIndent, position.Y + height, iconSize, Color.White * (disabled ? 0.5f : 1f));

                // draw text
                float textIndent = position.X + innerIndent + iconSize.X + 5;
                string text = I18n.Generic_PercentChanceOf(percent: CommonHelper.GetFormattedPercentageNumber(drop.Probability), label: drop.SampleItem.DisplayName);
                if (drop.MinDrop != drop.MaxDrop)
                    text += $" ({I18n.Generic_Range(min: drop.MinDrop, max: drop.MaxDrop)})";
                else if (drop.MinDrop > 1)
                    text += $" ({drop.MinDrop})";
                Vector2 textSize = spriteBatch.DrawTextBlock(font, text, new Vector2(textIndent, position.Y + height + 5), wrapWidth, textColor);

                // track clickable link
                if (subject is not null)
                {
                    Rectangle pixelArea = new((int)(position.X + innerIndent + iconSize.X + 5), (int)(position.Y + height + iconSize.Y / 2), (int)textSize.X, (int)textSize.Y);
                    this.LinkTextAreas.Add(new LinkTextArea(subject, pixelArea));
                }

                // cross out if it's guaranteed not to drop
                if (isPrevDropGuaranteed)
                    spriteBatch.DrawLine(position.X + innerIndent + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), Color.Gray);

                // draw conditions
                if (drop.Conditions != null)
                {
                    string conditionText = I18n.ConditionsSummary(conditions: HumanReadableConditionParser.Format(drop.Conditions));
                    height += textSize.Y + 5;
                    textSize = spriteBatch.DrawTextBlock(font, conditionText, new Vector2(textIndent, position.Y + height + 5), wrapWidth);

                    if (isPrevDropGuaranteed)
                        spriteBatch.DrawLine(position.X + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), disabled ? Color.Gray : Color.Black);
                }

                height += textSize.Y + 5;
            }

            // stop if drop is guaranteed
            if (drop.IsUnlocked && isGuaranteed)
                isPrevDropGuaranteed = true;
        }

        // return size
        return new Vector2(wrapWidth, height);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a fish pond's possible drops by population.</summary>
    /// <param name="currentPopulation">The current population for showing unlocked drops.</param>
    /// <param name="data">The fish pond data.</param>
    /// <param name="fish">The fish in the fish pond, if any.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <remarks>Derived from <see cref="FishPond.dayUpdate"/> and <see cref="FishPond.GetFishProduce"/>.</remarks>
    private IEnumerable<FishPondDrop> GetEntries(int currentPopulation, FishPondData data, SObject? fish, GameHelper gameHelper)
    {
        foreach (FishPondDropData rawDrop in gameHelper.GetFishPondDrops(data))
        {
            // filter conditions
            FishPondDropData drop = rawDrop;
            if (fish != null && drop.Conditions != null)
            {
                string? conditions = drop.Conditions;
                if (!this.FilterConditions(fish, ref conditions))
                    continue; // can never match for this fish

                if (conditions != drop.Conditions)
                    drop = new FishPondDropData(drop.MinPopulation, drop.Precedence, drop.SampleItem, drop.MinDrop, drop.MaxDrop, drop.Probability, conditions);
            }

            // build drop record
            bool isUnlocked = currentPopulation >= drop.MinPopulation;
            SpriteInfo? sprite = gameHelper.GetSprite(drop.SampleItem);
            yield return new FishPondDrop(drop, drop.SampleItem, sprite, isUnlocked);
        }
    }

    /// <summary>Get whether the given conditions can ever be true for a fish, and remove immutably true conditions from the query.</summary>
    /// <param name="fish">The fish for which to filter conditions.</param>
    /// <param name="gameStateQuery">The game state query to filter.</param>
    /// <returns>Returns whether the game state query can ever be true.</returns>
    private bool FilterConditions(SObject fish, ref string? gameStateQuery)
    {
        // immutable query
        if (GameStateQuery.IsImmutablyTrue(gameStateQuery))
        {
            gameStateQuery = null;
            return true;
        }
        if (GameStateQuery.IsImmutablyFalse(gameStateQuery))
            return false;

        // filter conditions in query
        List<string> conditions = [.. GameStateQuery.SplitRaw(gameStateQuery)];
        int prevCount = conditions.Count;
        for (int i = conditions.Count - 1; i >= 0; i--)
        {
            GameStateQuery.ParsedGameStateQuery[] parsed = GameStateQuery.Parse(conditions[i]);
            if (parsed.Length != 1)
                continue;

            switch (parsed[0].Query[0].ToUpperInvariant())
            {
                case nameof(GameStateQuery.DefaultResolvers.ITEM_CATEGORY):
                case nameof(GameStateQuery.DefaultResolvers.ITEM_HAS_EXPLICIT_OBJECT_CATEGORY):
                case nameof(GameStateQuery.DefaultResolvers.ITEM_ID):
                case nameof(GameStateQuery.DefaultResolvers.ITEM_ID_PREFIX):
                case nameof(GameStateQuery.DefaultResolvers.ITEM_NUMERIC_ID):
                case nameof(GameStateQuery.DefaultResolvers.ITEM_OBJECT_TYPE):
                case nameof(GameStateQuery.DefaultResolvers.ITEM_TYPE):
                    if (!GameStateQuery.CheckConditions(conditions[i], inputItem: fish))
                        return false;

                    conditions.RemoveAt(i);
                    break;
            }
        }

        if (conditions.Count == 0)
            gameStateQuery = null;
        else if (conditions.Count != prevCount)
            gameStateQuery = string.Join(", ", conditions);

        return true;
    }
}
