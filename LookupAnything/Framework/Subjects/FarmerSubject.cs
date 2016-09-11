using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a farmer (i.e. player).</summary>
    public class FarmerSubject : BaseSubject
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
        {
            // initialise
            this.Target = farmer;
            this.Initialise(farmer.Name, null, "Player");

            // add gender
            this.AddCustomFields(
                new GenericField("Gender", farmer.IsMale ? "male" : "female"),
                new GenericField("Farm name", farmer.farmName),
                new GenericField("Favourite thing", farmer.favoriteThing),
                new GenericField("Spouse", farmer.spouse, hasValue: farmer.isMarried()),
                new SkillBarField("Farming skill", farmer.experiencePoints[Farmer.farmingSkill], Color.Green, Color.Gray),
                new SkillBarField("Mining skill", farmer.experiencePoints[Farmer.miningSkill], Color.Green, Color.Gray),
                new SkillBarField("Foraging skill", farmer.experiencePoints[Farmer.foragingSkill], Color.Green, Color.Gray),
                new SkillBarField("Fishing skill", farmer.experiencePoints[Farmer.fishingSkill], Color.Green, Color.Gray),
                new SkillBarField("Combat skill", farmer.experiencePoints[Farmer.combatSkill], Color.Green, Color.Gray),
                new GenericField("Luck", $"{this.GetSpiritLuckMessage()}{Environment.NewLine}({(Game1.dailyLuck >= 0 ? "+" : "")}{Game1.dailyLuck * 100}% to many random checks)")
            );
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
            if (Game1.dailyLuck <= -.12)
                return "The spirits are furious today!";
            if (Game1.dailyLuck < -.07)
                return "The spirits are very displeased today.";
            if (Game1.dailyLuck < 0)
                return "The spirits are somewhat annoyed today.";
            if (Game1.dailyLuck == 0)
                return "The spirits are absolutely neutral today. Weird.";
            if (Game1.dailyLuck < 0.07)
                return "The spirits are in good humour today.";
            if (Game1.dailyLuck < 0.12)
                return "The spirits are very happy today.";
            else
                return "The spirits are joyous today!";
        }
    }
}