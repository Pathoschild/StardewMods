using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a farmer (i.e. player).</summary>
    internal class FarmerSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The lookup target.</summary>
        private readonly Farmer Target;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farmer">The lookup target.</param>
        public FarmerSubject(Farmer farmer)
            : base(farmer.Name, null, "Player")
        {
            this.Target = farmer;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            Farmer farmer = this.Target;

            int maxSkillPoints = metadata.Constants.PlayerMaxSkillPoints;
            int[] skillPointsPerLevel = metadata.Constants.PlayerSkillPointsPerLevel;
            yield return new GenericField("Gender", farmer.IsMale ? "male" : "female");
            yield return new GenericField("Farm name", farmer.farmName);
            yield return new GenericField("Favourite thing", farmer.favoriteThing);
            yield return new GenericField("Spouse", farmer.spouse, hasValue: farmer.isMarried());
            yield return new SkillBarField("Farming skill", farmer.experiencePoints[Farmer.farmingSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField("Mining skill", farmer.experiencePoints[Farmer.miningSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField("Foraging skill", farmer.experiencePoints[Farmer.foragingSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField("Fishing skill", farmer.experiencePoints[Farmer.fishingSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField("Combat skill", farmer.experiencePoints[Farmer.combatSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new GenericField("Luck", $"{this.GetSpiritLuckMessage()}{Environment.NewLine}({(Game1.dailyLuck >= 0 ? "+" : "")}{Game1.dailyLuck * 100}% to many random checks)");
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Farmer farmer = this.Target;
            FarmerSprite sprite = farmer.FarmerSprite;

            farmer.FarmerRenderer.draw(spriteBatch, sprite.CurrentAnimationFrame, sprite.CurrentFrame, sprite.SourceRect, position, Vector2.Zero, 0.8f, Color.White, 0, 1f, farmer);
            return true;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Get a summary of the player's luck today/</summary>
        /// <remarks>Derived from <see cref="GameLocation.answerDialogueAction"/>.</remarks>
        private string GetSpiritLuckMessage()
        {
            TV tv = new TV();
            MethodInfo method = GameHelper.GetPrivateMethod(tv, "getFortuneForecast");
            return (string)method.Invoke(tv, new object[0]);
        }
    }
}