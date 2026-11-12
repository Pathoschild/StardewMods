using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows experience points for a skill.</summary>
/// <remarks>Skill calculations reverse-engineered from <see cref="StardewValley.Farmer.checkForLevelGain"/>.</remarks>
internal class SkillBarField : PercentageBarField
{
    /*********
    ** Fields
    *********/
    /// <summary>The experience points needed for each skill level.</summary>
    private readonly int[] SkillPointsPerLevel;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="experience">The current progress value.</param>
    /// <param name="maxSkillPoints">The maximum experience points for a skill.</param>
    /// <param name="skillPointsPerLevel">The experience points needed for each skill level.</param>
    public SkillBarField(string label, int experience, int maxSkillPoints, int[] skillPointsPerLevel)
        : base(label, experience, maxSkillPoints, Color.Green, Color.Gray, null)
    {
        this.SkillPointsPerLevel = skillPointsPerLevel;
    }

    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        return this.GetProgressText(out _);
    }

    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth, float visibleHeight)
    {
        int[] pointsPerLevel = this.SkillPointsPerLevel;

        // generate text
        string text = this.GetProgressText(out int currentLevel);

        // draw bars
        const int barWidth = 25;
        float leftOffset = 0;
        int barHeight = 0;
        for (int level = 0; level < pointsPerLevel.Length; level++)
        {
            float progress;
            if (level < currentLevel)
                progress = 1f;
            else if (level > currentLevel)
                progress = 0f;
            else
            {
                int levelExp = pointsPerLevel[level];
                progress = Math.Min(1f, this.CurrentValue / (levelExp * 1f));
            }

            Vector2 barSize = this.DrawBar(spriteBatch, position + new Vector2(leftOffset, 0), progress, this.FilledColor, this.EmptyColor, barWidth);
            barHeight = (int)barSize.Y;
            leftOffset += barSize.X + 2;
        }

        // draw text
        Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(leftOffset, 0), wrapWidth - leftOffset);
        return new Vector2(leftOffset + textSize.X, Math.Max(barHeight, textSize.Y));
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the text describing the skill level and the experience needed for the next one.</summary>
    /// <param name="currentLevel">The current skill level.</param>
    private string GetProgressText(out int currentLevel)
    {
        int[] pointsPerLevel = this.SkillPointsPerLevel;

        int nextLevelExp = pointsPerLevel.FirstOrDefault(p => p - this.CurrentValue > 0);
        int pointsForNextLevel = nextLevelExp > 0 ? nextLevelExp - this.CurrentValue : 0;
        currentLevel = nextLevelExp > 0 ? Array.IndexOf(pointsPerLevel, nextLevelExp) : pointsPerLevel.Length;
        return pointsForNextLevel > 0
            ? I18n.Player_Skill_Progress(level: currentLevel, expNeeded: pointsForNextLevel)
            : I18n.Player_Skill_ProgressLast(level: currentLevel);
    }
}
