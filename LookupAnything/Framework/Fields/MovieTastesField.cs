using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows how much each NPC likes watching a movie.</summary>
    internal class MovieTastesField : GenericField
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        public MovieTastesField(string label, IDictionary<GiftTaste, string[]> giftTastes, GiftTaste showTaste)
            : base(label, MovieTastesField.GetText(giftTastes, showTaste)) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the text to display.</summary>
        /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        private static string GetText(IDictionary<GiftTaste, string[]> giftTastes, GiftTaste showTaste)
        {
            if (!giftTastes.ContainsKey(showTaste))
                return null;

            string[] names = giftTastes[showTaste].OrderBy(p => p).ToArray();
            return string.Join(", ", names);
        }
    }
}
