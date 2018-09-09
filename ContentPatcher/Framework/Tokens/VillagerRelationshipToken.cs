using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for NPC relationship types.</summary>
    internal class VillagerRelationshipToken : BaseToken
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
        public VillagerRelationshipToken()
            : base(ConditionType.Relationship.ToString(), canHaveMultipleValues: false, requiresSubkeys: true) { }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.Values.Clear();
            if (Context.IsWorldReady)
            {
                foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
                    this.Values[pair.Key] = pair.Value.Status.ToString();
            }
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public override IEnumerable<string> GetSubkeys()
        {
            return this.Values.Keys;
        }

        /// <summary>Get the current token values for a subkey, if <see cref="IToken.RequiresSubkeys"/> is true.</summary>
        /// <param name="subkey">The subkey to check.</param>
        /// <exception cref="InvalidOperationException">This token does not support subkeys (see <see cref="IToken.RequiresSubkeys"/>).</exception>
        public override IEnumerable<string> GetValues(string subkey)
        {
            if (this.Values.TryGetValue(subkey, out string value))
                yield return value;
        }
    }
}
