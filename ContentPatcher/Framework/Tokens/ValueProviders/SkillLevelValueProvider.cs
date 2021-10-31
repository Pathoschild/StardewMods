using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the player's skill levels.</summary>
    internal class SkillLevelValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles reading info from the current save.</summary>
        private readonly TokenSaveReader SaveReader;

        /// <summary>The player's current skill levels.</summary>
        private readonly IDictionary<Skill, int> SkillLevels = new Dictionary<Skill, int>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="saveReader">Handles reading info from the current save.</param>
        public SkillLevelValueProvider(TokenSaveReader saveReader)
            : base(ConditionType.SkillLevel, mayReturnMultipleValuesForRoot: true)
        {
            this.SaveReader = saveReader;
            this.EnableInputArguments(required: false, mayReturnMultipleValues: false, maxPositionalArgs: 1);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                IDictionary<Skill, int> oldSkillLevels = new Dictionary<Skill, int>(this.SkillLevels);

                this.SkillLevels.Clear();
                if (this.MarkReady(this.SaveReader.IsReady))
                {
                    var player = this.SaveReader.GetCurrentPlayer();

                    this.SkillLevels[Skill.Combat] = player.CombatLevel;
                    this.SkillLevels[Skill.Farming] = player.FarmingLevel;
                    this.SkillLevels[Skill.Fishing] = player.FishingLevel;
                    this.SkillLevels[Skill.Foraging] = player.ForagingLevel;
                    this.SkillLevels[Skill.Luck] = player.LuckLevel;
                    this.SkillLevels[Skill.Mining] = player.MiningLevel;

                    return
                        this.SkillLevels.Count != oldSkillLevels.Count
                        || this.SkillLevels.Any(entry => !oldSkillLevels.TryGetValue(entry.Key, out int oldLevel) || entry.Value != oldLevel);
                }

                return false;
            });

        }

        /// <inheritdoc />
        public override InvariantHashSet GetValidPositionalArgs()
        {
            return new InvariantHashSet(Enum.GetNames(typeof(Skill)));
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            if (input.HasPositionalArgs)
            {
                return this.TryParseEnum(input.GetFirstPositionalArg(), out Skill skill) && this.SkillLevels.TryGetValue(skill, out int level)
                    ? new[] { level.ToString() }
                    : Enumerable.Empty<string>();
            }
            else
            {
                return this.SkillLevels
                    .Select(pair => $"{pair.Key}:{pair.Value}");
            }
        }
    }
}
