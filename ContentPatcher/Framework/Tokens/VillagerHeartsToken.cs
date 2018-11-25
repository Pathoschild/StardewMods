using System;
using System.Collections.Generic;
using System.Globalization;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
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
        private readonly InvariantDictionary<string> Values = new InvariantDictionary<string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public VillagerHeartsToken()
            : base(ConditionType.Hearts.ToString(), canHaveMultipleRootValues: false)
        {
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.Values.Clear();
            this.IsValidInContext = Context.IsWorldReady;
            if (this.IsValidInContext)
            {
                foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
                    this.Values[pair.Key] = (pair.Value.Points / NPC.friendshipPointsPerHeartLevel).ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public override IEnumerable<TokenName> GetSubkeys()
        {
            foreach (string key in this.Values.Keys)
                yield return new TokenName(this.Name.Key, key);
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);

            if (name.HasSubkey())
            {
                if (this.Values.TryGetValue(name.Subkey, out string value))
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
