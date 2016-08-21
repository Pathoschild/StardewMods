using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>The base class for object metadata.</summary>
    public abstract class BaseSubject : ISubject
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The display name.</summary>
        public string Name { get; protected set; }

        /// <summary>The object description (if applicable).</summary>
        public string Description { get; protected set; }

        /// <summary>The object type.</summary>
        public string Type { get; protected set; }

        /// <summary>The item price when sold or shipped (if applicable).</summary>
        public int? SalePrice { get; protected set; }

        /// <summary>How much each NPC likes receiving this item as a gift (if applicable).</summary>
        public IDictionary<GiftTaste, NPC[]> GiftTastes { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public abstract bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size);


        /*********
        ** Protected methods
        *********/
        /// <summary>Initialise the metadata. This method is provided for convenience; the properties can also be set directly.</summary>
        /// <param name="name">The display name.</param>
        /// <param name="description">The object description (if applicable).</param>
        /// <param name="type">The object type.</param>
        /// <param name="salePrice">The item price when sold or shipped (if applicable).</param>
        /// <param name="giftTastes">How much each NPC likes receiving this item as a gift (if applicable).</param>
        protected void Initialise(string name, string description, string type, int? salePrice = null, IDictionary<GiftTaste, NPC[]> giftTastes = null)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.SalePrice = salePrice;
            this.GiftTastes = giftTastes;
        }
    }
}