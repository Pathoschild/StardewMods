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
        ** Fields
        *********/
        /// <summary>Get whether the player data is available in the current context.</summary>
        private readonly Func<bool> IsPlayerDataAvailable;

        /// <summary>The player's current skill levels.</summary>
        private readonly IDictionary<Skill, int> SkillLevels = new Dictionary<Skill, int>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SkillLevelToken(Func<bool> isPlayerDataAvailable)
            : base(ConditionType.SkillLevel.ToString(), canHaveMultipleRootValues: true)
        {
            this.IsPlayerDataAvailable = isPlayerDataAvailable;
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.SkillLevels.Clear();
            this.IsValidInContext = this.IsPlayerDataAvailable();
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

        /// <summary>Get the allowed subkeys (or <c>null</c> if any value is allowed).</summary>
        protected override InvariantHashSet GetAllowedSubkeys()
        {
            return new InvariantHashSet(Enum.GetNames(typeof(Skill)));
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
    }
}
