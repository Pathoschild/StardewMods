using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FishPond;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of drops for a fish pond grouped by population gate.</summary>
    internal class FishPondDropsField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        protected GameHelper GameHelper;

        /// <summary>The possible drops.</summary>
        private readonly FishPondDrop[] Drops;

        /// <summary>The text to display before the list, if any.</summary>
        private readonly string Preface;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="currentPopulation">The current population for showing unlocked drops.</param>
        /// <param name="data">The fish pond data.</param>
        /// <param name="preface">>The text to display before the list, if any.</param>
        public FishPondDropsField(GameHelper gameHelper, string label, int currentPopulation, FishPondData data, string preface)
            : base(label)
        {
            this.GameHelper = gameHelper;
            this.Drops = this.GetEntries(currentPopulation, data, gameHelper).ToArray();
            this.HasValue = this.Drops.Any();
            this.Preface = preface;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
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
                    // draw icon
                    spriteBatch.DrawSpriteWithin(drop.Sprite, position.X + innerIndent, position.Y + height, iconSize, Color.White * (disabled ? 0.5f : 1f));

                    // draw text
                    string text = I18n.Generic_PercentChanceOf(percent: (int)(Math.Round(drop.Probability, 4) * 100), label: drop.SampleItem.DisplayName);
                    if (drop.MinDrop != drop.MaxDrop)
                        text += $" ({I18n.Generic_Range(min: drop.MinDrop, max: drop.MaxDrop)})";
                    else if (drop.MinDrop > 1)
                        text += $" ({drop.MinDrop})";
                    Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(innerIndent + iconSize.X + 5, height + 5), wrapWidth, disabled ? Color.Gray : Color.Black);

                    // cross out if it's guaranteed not to drop
                    if (isPrevDropGuaranteed)
                        spriteBatch.DrawLine(position.X + innerIndent + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), Color.Gray);

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
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <remarks>Derived from <see cref="FishPond.dayUpdate"/> and <see cref="FishPond.GetFishProduce"/>.</remarks>
        private IEnumerable<FishPondDrop> GetEntries(int currentPopulation, FishPondData data, GameHelper gameHelper)
        {
            foreach (FishPondDropData drop in gameHelper.GetFishPondDrops(data))
            {
                bool isUnlocked = currentPopulation >= drop.MinPopulation;
                SObject item = this.GameHelper.GetObjectBySpriteIndex(drop.ItemID);
                SpriteInfo? sprite = gameHelper.GetSprite(item);
                yield return new FishPondDrop(drop, item, sprite, isUnlocked);
            }
        }
    }
}
