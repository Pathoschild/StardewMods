using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Fields;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>The base class for object metadata.</summary>
    internal abstract class BaseSubject : ISubject
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

        /// <summary>The custom fields to display for this subject (if any).</summary>
        public ICustomField[] CustomFields { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public abstract bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size);


        /*********
        ** Protected methods
        *********/
        /// <summary>Initialise the base values.</summary>
        /// <param name="name">The display name.</param>
        /// <param name="description">The object description (if applicable).</param>
        /// <param name="type">The object type.</param>
        protected void Initialise(string name, string description, string type)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
        }

        /// <summary>Add custom fields to the list.</summary>
        /// <param name="fields">The fields to add.</param>
        protected void AddCustomFields(params ICustomField[] fields)
        {
            this.CustomFields = this.CustomFields?.Concat(fields).ToArray() ?? fields;
        }
    }
}