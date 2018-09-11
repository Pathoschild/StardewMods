using System;
using System.Collections.Generic;
using System.Globalization;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for NPC friendship hearts.</summary>
    internal class VillagerHeartsToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The relationships by NPC.</summary>
        private readonly IDictionary<TokenName, string> Values = new Dictionary<TokenName, string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public VillagerHeartsToken()
            : base(ConditionType.Hearts.ToString(), canHaveMultipleValues: false, requiresSubkeys: true) { }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.Values.Clear();
            if (Context.IsWorldReady)
            {
                foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
                    this.Values[new TokenName(this.Name.Key, pair.Key)] = (pair.Value.Points / NPC.friendshipPointsPerHeartLevel).ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public override IEnumerable<TokenName> GetSubkeys()
        {
            return this.Values.Keys;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or this token require a subkeys and <paramref name="name"/> does not specify one.</exception>
        public override IEnumerable<string> GetValues(TokenName? name = null)
        {
            this.AssertTokenName(name);

            if (this.Values.TryGetValue(name.Value, out string value))
                yield return value;
        }
    }
}
