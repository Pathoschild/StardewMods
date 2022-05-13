using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for NPC friendship hearts.</summary>
    internal class VillagerHeartsValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles reading info from the current save.</summary>
        private readonly TokenSaveReader SaveReader;

        /// <summary>The relationships by NPC.</summary>
        private readonly SortedDictionary<string, string> Values = new(HumanSortComparer.DefaultIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="saveReader">Handles reading info from the current save.</param>
        public VillagerHeartsValueProvider(TokenSaveReader saveReader)
            : base(ConditionType.Hearts, mayReturnMultipleValuesForRoot: false)
        {
            this.SaveReader = saveReader;

            this.EnableInputArguments(required: false, mayReturnMultipleValues: false, maxPositionalArgs: 1);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Values, () =>
            {
                this.Values.Clear();
                if (this.MarkReady(this.SaveReader.IsReady))
                {
                    foreach ((string npc, Friendship? friendship) in this.SaveReader.GetFriendships())
                    {
                        int points = friendship?.Points ?? 0;
                        this.Values[npc] = (points / NPC.friendshipPointsPerHeartLevel).ToString(CultureInfo.InvariantCulture);
                    }
                }
            });
        }

        /// <inheritdoc />
        public override IImmutableSet<string> GetValidPositionalArgs()
        {
            return ImmutableSets.From(this.Values.Keys);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            if (input.HasPositionalArgs)
            {
                return this.Values.TryGetValue(input.GetFirstPositionalArg()!, out string? value)
                    ? ImmutableSets.FromValue(value)
                    : ImmutableSets.Empty;
            }
            else
                return this.Values.Select(pair => $"{pair.Key}:{pair.Value}");
        }
    }
}
