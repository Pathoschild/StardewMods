using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

/// <summary>A value provider for the player's skill levels.</summary>
internal class SkillLevelValueProvider : BaseValueProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles reading info from the current save.</summary>
    private readonly TokenSaveReader SaveReader;

    /// <summary>The player's current skill levels.</summary>
    private readonly Dictionary<Skill, int> SkillLevels = [];

    /// <summary>The valid skill values.</summary>
    private readonly IInvariantSet ValidValues = InvariantSets.From(Enum.GetNames(typeof(Skill)));


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
            Dictionary<Skill, int> oldSkillLevels = new(this.SkillLevels);

            this.SkillLevels.Clear();
            if (this.MarkReady(this.SaveReader.IsReady))
            {
                Farmer? player = this.SaveReader.GetCurrentPlayer();

                this.SkillLevels[Skill.Combat] = player?.CombatLevel ?? 0;
                this.SkillLevels[Skill.Farming] = player?.FarmingLevel ?? 0;
                this.SkillLevels[Skill.Fishing] = player?.FishingLevel ?? 0;
                this.SkillLevels[Skill.Foraging] = player?.ForagingLevel ?? 0;
                this.SkillLevels[Skill.Luck] = player?.LuckLevel ?? 0;
                this.SkillLevels[Skill.Mining] = player?.MiningLevel ?? 0;

                return
                    this.SkillLevels.Count != oldSkillLevels.Count
                    || this.SkillLevels.Any(entry => !oldSkillLevels.TryGetValue(entry.Key, out int oldLevel) || entry.Value != oldLevel);
            }

            return false;
        });

    }

    /// <inheritdoc />
    public override IInvariantSet GetValidPositionalArgs()
    {
        return this.ValidValues;
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetValues(IInputArguments input)
    {
        this.AssertInput(input);

        if (input.HasPositionalArgs)
        {
            return this.TryParseEnum(input.GetFirstPositionalArg(), out Skill skill) && this.SkillLevels.TryGetValue(skill, out int level)
                ? InvariantSets.FromValue(level)
                : InvariantSets.Empty;
        }
        else
        {
            return this.SkillLevels
                .Select(pair => $"{pair.Key}:{pair.Value}");
        }
    }
}
