using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for the player's skill levels.</summary>
    internal class SkillLevelToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The player's current skill levels.</summary>
        private readonly IDictionary<Skill, int> SkillLevels = new Dictionary<Skill, int>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SkillLevelToken()
            : base(ConditionType.SkillLevel.ToString(), canHaveMultipleRootValues: true)
        {
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.SkillLevels.Clear();
            this.IsValidInContext = Context.IsWorldReady;
            if (this.IsValidInContext)
            {
                this.SkillLevels[Skill.Combat] = Game1.player.CombatLevel;
                this.SkillLevels[Skill.Farming] = Game1.player.FarmingLevel;
                this.SkillLevels[Skill.Fishing] = Game1.player.FishingLevel;
                this.SkillLevels[Skill.Foraging] = Game1.player.ForagingLevel;
                this.SkillLevels[Skill.Luck] = Game1.player.LuckLevel;
                this.SkillLevels[Skill.Mining] = Game1.player.MiningLevel;
            }
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public override IEnumerable<TokenName> GetSubkeys()
        {
            foreach (Skill skill in this.SkillLevels.Keys)
                yield return new TokenName(ConditionType.SkillLevel, skill.ToString());
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);

            if (name.HasSubkey())
            {
                if (this.TryParseEnum(name.Subkey, out Skill skill) && this.SkillLevels.TryGetValue(skill, out int level))
                    yield return level.ToString();
            }
            else
            {
                foreach (var pair in this.SkillLevels)
                    yield return $"{pair.Key}:{pair.Value}";
            }
        }

        /// <summary>Perform custom validation on a set of input values.</summary>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryCustomValidation(InvariantHashSet values, out string error)
        {
            if (!base.TryCustomValidation(values, out error))
                return false;

            string[] invalidValues = this.GetInvalidValues(values).ToArray();
            if (invalidValues.Any())
            {
                error = $"can't parse some values ({string.Join(", ", invalidValues)}) as skill types; must be a predefined name ({string.Join(", ", Enum.GetNames(typeof(Skill)).OrderByIgnoreCase(p => p))}).";
                return false;
            }

            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the values which can't be parsed as a profession ID.</summary>
        /// <param name="values">The values to check.</param>
        private IEnumerable<string> GetInvalidValues(IEnumerable<string> values)
        {
            foreach (string value in values)
            {
                if (!this.TryParseEnum(value, out Skill _))
                    yield return value;
            }
        }
    }
}
