using System.Collections.Generic;
using System.Globalization;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for NPC friendship hearts.</summary>
    internal class VillagerHeartsValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The relationships by NPC.</summary>
        private readonly InvariantDictionary<string> Values = new InvariantDictionary<string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public VillagerHeartsValueProvider()
            : base(ConditionType.Hearts, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: false, mayReturnMultipleValues: false, maxPositionalArgs: 1);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Values, () =>
            {
                this.Values.Clear();
                if (this.MarkReady(Context.IsWorldReady))
                {
                    // met NPCs
                    foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
                        this.Values[pair.Key] = (pair.Value.Points / NPC.friendshipPointsPerHeartLevel).ToString(CultureInfo.InvariantCulture);

                    // unmet NPCs
                    foreach (NPC npc in this.GetSocialVillagers())
                    {
                        if (!this.Values.ContainsKey(npc.Name))
                            this.Values[npc.Name] = "0";
                    }
                }
            });
        }

        /// <inheritdoc />
        public override InvariantHashSet GetValidPositionalArgs()
        {
            return new InvariantHashSet(this.Values.Keys);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            if (input.HasPositionalArgs)
            {
                if (this.Values.TryGetValue(input.GetFirstPositionalArg(), out string value))
                    yield return value;
            }
            else
            {
                foreach (var pair in this.Values)
                    yield return $"{pair.Key}:{pair.Value}";
            }
        }
    }
}
