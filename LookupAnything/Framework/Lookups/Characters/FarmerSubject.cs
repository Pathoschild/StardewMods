using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters
{
    /// <summary>Describes a farmer (i.e. player).</summary>
    internal class FarmerSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The lookup target.</summary>
        private readonly SFarmer Target;

        /// <summary>Whether this is being displayed on the load menu, before the save data is fully initialized.</summary>
        private readonly bool IsLoadMenu;

        /// <summary>The raw save data for this player, if <see cref="IsLoadMenu"/> is true.</summary>
        private readonly Lazy<XElement> RawSaveData;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="farmer">The lookup target.</param>
        /// <param name="isLoadMenu">Whether this is being displayed on the load menu, before the save data is fully initialized.</param>
        public FarmerSubject(GameHelper gameHelper, SFarmer farmer, bool isLoadMenu = false)
            : base(gameHelper, farmer.Name, null, I18n.Type_Player())
        {
            this.Target = farmer;
            this.IsLoadMenu = isLoadMenu;
            this.RawSaveData = isLoadMenu
                ? new Lazy<XElement>(() => this.ReadSaveFile(farmer.slotName))
                : null;
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            SFarmer target = this.Target;

            // basic info
            yield return new GenericField(I18n.Player_Gender(), target.IsMale ? I18n.Player_Gender_Male() : I18n.Player_Gender_Female());
            yield return new GenericField(I18n.Player_FarmName(), target.farmName.Value);
            yield return new GenericField(I18n.Player_FarmMap(), this.GetFarmType());
            yield return new GenericField(I18n.Player_FavoriteThing(), target.favoriteThing.Value);
            yield return new GenericField(Game1.player.spouse == "Krobus" ? I18n.Player_Housemate() : I18n.Player_Spouse(), this.GetSpouseName());

            // saw a movie this week
            if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
                yield return new GenericField(I18n.Player_WatchedMovieThisWeek(), this.Stringify(target.lastSeenMovieWeek.Value >= Game1.Date.TotalSundayWeeks));

            // skills
            int maxSkillPoints = this.Constants.PlayerMaxSkillPoints;
            int[] skillPointsPerLevel = this.Constants.PlayerSkillPointsPerLevel;
            yield return new SkillBarField(I18n.Player_FarmingSkill(), target.experiencePoints[SFarmer.farmingSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField(I18n.Player_MiningSkill(), target.experiencePoints[SFarmer.miningSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField(I18n.Player_ForagingSkill(), target.experiencePoints[SFarmer.foragingSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField(I18n.Player_FishingSkill(), target.experiencePoints[SFarmer.fishingSkill], maxSkillPoints, skillPointsPerLevel);
            yield return new SkillBarField(I18n.Player_CombatSkill(), target.experiencePoints[SFarmer.combatSkill], maxSkillPoints, skillPointsPerLevel);

            // luck
            string luckSummary = I18n.Player_Luck_Summary(percent: (Game1.player.DailyLuck >= 0 ? "+" : "") + Math.Round(Game1.player.DailyLuck * 100, 2));
            yield return new GenericField(I18n.Player_Luck(), $"{this.GetSpiritLuckMessage()}{Environment.NewLine}({luckSummary})");

            // save version
            if (this.IsLoadMenu)
                yield return new GenericField(I18n.Player_SaveFormat(), this.GetSaveFormat(this.RawSaveData.Value));
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
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
            SFarmer target = this.Target;

            if (this.IsLoadMenu)
                target.FarmerRenderer.draw(spriteBatch, new FarmerSprite.AnimationFrame(0, 0, false, false), 0, new Rectangle(0, 0, 16, 32), position, Vector2.Zero, 0.8f, 2, Color.White, 0.0f, 1f, target);
            else
            {
                FarmerSprite sprite = target.FarmerSprite;
                target.FarmerRenderer.draw(spriteBatch, sprite.CurrentAnimationFrame, sprite.CurrentFrame, sprite.SourceRect, position, Vector2.Zero, 0.8f, Color.White, 0, 1f, target);
            }

            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a summary of the player's luck today.</summary>
        /// <remarks>Derived from <see cref="GameLocation.answerDialogueAction"/>.</remarks>
        private string GetSpiritLuckMessage()
        {
            // inject daily luck if not loaded yet
            if (this.IsLoadMenu)
            {
                string rawDailyLuck = this.RawSaveData.Value?.Element("dailyLuck")?.Value;
                if (rawDailyLuck == null)
                    return null;

                Game1.player.team.sharedDailyLuck.Value = double.Parse(rawDailyLuck);
            }

            // get daily luck message
            return new TV().getFortuneForecast(this.Target);
        }

        /// <summary>Get the human-readable farm type selected by the player.</summary>
        private string GetFarmType()
        {
            // get farm type
            int farmType = Game1.whichFarm;
            if (this.IsLoadMenu)
            {
                string rawType = this.RawSaveData.Value?.Element("whichFarm")?.Value;
                farmType = rawType != null ? int.Parse(rawType) : -1;
            }

            // get type name
            return farmType switch
            {
                -1 => null,

                Farm.combat_layout => GameI18n.GetString("Strings\\UI:Character_FarmCombat").Replace("_", Environment.NewLine),
                Farm.default_layout => GameI18n.GetString("Strings\\UI:Character_FarmStandard").Replace("_", Environment.NewLine),
                Farm.forest_layout => GameI18n.GetString("Strings\\UI:Character_FarmForaging").Replace("_", Environment.NewLine),
                Farm.mountains_layout => GameI18n.GetString("Strings\\UI:Character_FarmMining").Replace("_", Environment.NewLine),
                Farm.riverlands_layout => GameI18n.GetString("Strings\\UI:Character_FarmFishing").Replace("_", Environment.NewLine),
                Farm.fourCorners_layout => GameI18n.GetString("Strings\\UI:Character_FarmFourCorners").Replace("_", Environment.NewLine),
                Farm.beach_layout => GameI18n.GetString("Strings\\UI:Character_FarmBeach").Replace("_", Environment.NewLine),

                _ => I18n.Player_FarmMap_Custom()
            };
        }

        /// <summary>Get the player's spouse name, if they're married.</summary>
        /// <returns>Returns the spouse name, or <c>null</c> if they're not married.</returns>
        private string GetSpouseName()
        {
            if (this.IsLoadMenu)
                return this.Target.spouse;

            long? spousePlayerID = this.Target.team.GetSpouse(this.Target.UniqueMultiplayerID);
            SFarmer spousePlayer = spousePlayerID.HasValue ? Game1.getFarmerMaybeOffline(spousePlayerID.Value) : null;

            return spousePlayer?.displayName ?? Game1.player.getSpouse()?.displayName;
        }

        /// <summary>Load the raw save file as an XML document.</summary>
        /// <param name="slotName">The slot file to read.</param>
        private XElement ReadSaveFile(string slotName)
        {
            if (slotName == null)
                return null; // not available (e.g. farmhand)

            // get file
            FileInfo file = new FileInfo(Path.Combine(StardewModdingAPI.Constants.SavesPath, slotName, slotName));
            if (!file.Exists)
                return null;

            // read contents
            string text = File.ReadAllText(file.FullName);
            return XElement.Parse(text);
        }

        /// <summary>Get the last game version which wrote to a save file.</summary>
        /// <param name="saveData">The save data to check.</param>
        private string GetSaveFormat(XElement saveData)
        {
            // >=1.4
            string version = saveData.Element("gameVersion")?.Value;
            if (!string.IsNullOrWhiteSpace(version))
                return version;

            // 1.4
            if (saveData.Element("hasApplied1_4_UpdateChanges") != null)
                return "1.4";

            // 1.3
            if (saveData.Element("hasApplied1_3_UpdateChanges") != null)
                return "1.3";

            // 1.1/1.2
            if (saveData.Element("whichFarm") != null)
                return "1.1 - 1.2";

            // 1.0
            return "1.0";
        }
    }
}
