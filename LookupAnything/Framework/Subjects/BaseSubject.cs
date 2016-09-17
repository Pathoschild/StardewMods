using System.Collections.Generic;
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


        /*********
        ** Public methods
        *********/
        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public abstract IEnumerable<ICustomField> GetData(Metadata metadata);

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public abstract bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        protected BaseSubject() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The display name.</param>
        /// <param name="description">The object description (if applicable).</param>
        /// <param name="type">The object type.</param>
        protected BaseSubject(string name, string description, string type)
        {
            this.Initialise(name, description, type);
        }

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
    }
}