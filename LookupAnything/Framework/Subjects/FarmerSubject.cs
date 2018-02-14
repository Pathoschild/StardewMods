using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a farmer (i.e. player).</summary>
    internal class FarmerSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The lookup target.</summary>
        private readonly SFarmer Target;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farmer">The lookup target.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public FarmerSubject(SFarmer farmer, ITranslationHelper translations, IReflectionHelper reflectionHelper)
            : base(farmer.Name, null, translations.Get(L10n.Types.Player), translations)
        {
            this.Target = farmer;
            this.Reflection = reflectionHelper;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            SFarmer farmer = this.Target;

            int maxSkillPoints = metadata.Constants.PlayerMaxSkillPoints;
            int[] skillPointsPerLevel = metadata.Constants.PlayerSkillPointsPerLevel;
            string luckSummary = this.Translate(L10n.Player.LuckSummary, new { percent = (Game1.dailyLuck >= 0 ? "+" : "") + (Game1.dailyLuck * 100) });

            yield return new GenericField(this.Translate(L10n.Player.Gender), this.Translate(farmer.IsMale ? L10n.Player.GenderMale : L10n.Player.GenderFemale));
            yield return new GenericField(this.Translate(L10n.Player.FarmName), farmer.farmName);
            yield return new GenericField(this.Translate(L10n.Player.FavoriteThing), farmer.favoriteThing);
            yield return new GenericField(this.Translate(L10n.Player.Spouse), farmer.isMarried() ? Game1.getCharacterFromName(farmer.spouse).displayName : null);
            yield return new SkillBarField(this.Translate(L10n.Player.FarmingSkill), farmer.experiencePoints[SFarmer.farmingSkill], maxSkillPoints, skillPointsPerLevel, this.Text);
            yield return new SkillBarField(this.Translate(L10n.Player.MiningSkill), farmer.experiencePoints[SFarmer.miningSkill], maxSkillPoints, skillPointsPerLevel, this.Text);
            yield return new SkillBarField(this.Translate(L10n.Player.ForagingSkill), farmer.experiencePoints[SFarmer.foragingSkill], maxSkillPoints, skillPointsPerLevel, this.Text);
            yield return new SkillBarField(this.Translate(L10n.Player.FishingSkill), farmer.experiencePoints[SFarmer.fishingSkill], maxSkillPoints, skillPointsPerLevel, this.Text);
            yield return new SkillBarField(this.Translate(L10n.Player.CombatSkill), farmer.experiencePoints[SFarmer.combatSkill], maxSkillPoints, skillPointsPerLevel, this.Text);
            yield return new GenericField(this.Translate(L10n.Player.Luck), $"{this.GetSpiritLuckMessage()}{Environment.NewLine}({luckSummary})");
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
        {
            SFarmer target = this.Target;

            // pinned fields
            yield return new GenericDebugField("immunity", target.immunity, pinned: true);
            yield return new GenericDebugField("resilience", target.resilience, pinned: true);
            yield return new GenericDebugField("magnetic radius", target.MagneticRadius, pinned: true);

            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(target))
                yield return field;
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            SFarmer farmer = this.Target;
            FarmerSprite sprite = farmer.FarmerSprite;

            farmer.FarmerRenderer.draw(spriteBatch, sprite.CurrentAnimationFrame, sprite.CurrentFrame, sprite.SourceRect, position, Vector2.Zero, 0.8f, Color.White, 0, 1f, farmer);
            return true;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Get a summary of the player's luck today.</summary>
        /// <remarks>Derived from <see cref="GameLocation.answerDialogueAction"/>.</remarks>
        private string GetSpiritLuckMessage()
        {
            TV tv = new TV();
            return this.Reflection.GetMethod(tv, "getFortuneForecast").Invoke<string>();
        }
    }
}
