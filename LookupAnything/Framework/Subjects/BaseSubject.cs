using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>The base class for object metadata.</summary>
    internal abstract class BaseSubject : ISubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides translations stored in the mod folder.</summary>
        protected ITranslationHelper Text { get; }

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        protected GameHelper GameHelper { get; }

        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        protected Metadata Metadata => this.GameHelper.Metadata;

        /// <summary>Constant values hardcoded by the game.</summary>
        protected ConstantData Constants => this.Metadata.Constants;

        /// <summary>Provides subject entries for target values.</summary>
        protected SubjectFactory Codex { get; }


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
        public abstract IEnumerable<ICustomField> GetData();

        /// <summary>Get raw debug data to display for this subject.</summary>
        public abstract IEnumerable<IDebugField> GetDebugFields();

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
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        protected BaseSubject(SubjectFactory codex, GameHelper gameHelper, ITranslationHelper translations)
        {
            this.Codex = codex;
            this.GameHelper = gameHelper;
            this.Text = translations;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="name">The display name.</param>
        /// <param name="description">The object description (if applicable).</param>
        /// <param name="type">The object type.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        protected BaseSubject(SubjectFactory codex, GameHelper gameHelper, string name, string description, string type, ITranslationHelper translations)
            : this(codex, gameHelper, translations)
        {
            this.Initialize(name, description, type);
        }

        /// <summary>Initialize the base values.</summary>
        /// <param name="name">The display name.</param>
        /// <param name="description">The object description (if applicable).</param>
        /// <param name="type">The object type.</param>
        protected void Initialize(string name, string description, string type)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
        }

        /// <summary>Get all debug fields by reflecting over an instance.</summary>
        /// <param name="obj">The object instance over which to reflect.</param>
        protected IEnumerable<IDebugField> GetDebugFieldsFrom(object obj)
        {
            if (obj == null)
                yield break;

            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                // get fields & properties
                var fields =
                    (
                        from field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        where !field.IsLiteral // exclude constants
                        select new { field.Name, Type = field.FieldType, Value = this.GetDebugValue(obj, field) }
                    )
                    .Concat(
                        from property in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        where property.CanRead
                        select new { property.Name, Type = property.PropertyType, Value = this.GetDebugValue(obj, property) }
                    )
                    .OrderBy(field => field.Name, StringComparer.OrdinalIgnoreCase);

                // yield valid values
                IDictionary<string, string> seenValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in fields)
                {
                    if (seenValues.TryGetValue(field.Name, out string value) && value == field.Value)
                        continue; // key/value pair differs only in the key case
                    if (field.Value == field.Type.ToString())
                        continue; // can't be displayed

                    seenValues[field.Name] = field.Value;
                    yield return new GenericDebugField($"{type.Name}::{field.Name}", field.Value);
                }
            }
        }

        /// <summary>Get a human-readable representation of a value.</summary>
        /// <param name="value">The underlying value.</param>
        protected string Stringify(object value)
        {
            return this.Text.Stringify(value);
        }

        /// <summary>Get a relative date value like 'tomorrow' or 'in 5 days'.</summary>
        /// <param name="date">The date to represent.</param>
        protected string GetRelativeDateStr(SDate date)
        {
            return this.GetRelativeDateStr(date.DaysSinceStart - SDate.Now().DaysSinceStart);
        }

        /// <summary>Get a relative date value like 'tomorrow' or 'in 5 days'.</summary>
        /// <param name="days">The day offset.</param>
        protected string GetRelativeDateStr(int days)
        {
            switch (days)
            {
                case -1:
                    return L10n.Generic.Yesterday();

                case 0:
                    return L10n.Generic.Now();

                case 1:
                    return L10n.Generic.Tomorrow();

                default:
                    if (days > 0)
                        return L10n.Generic.InXDays(count: days);
                    return L10n.Generic.XDaysAgo(count: -days);
            }
        }

        /// <summary>Get a human-readable value for a debug value.</summary>
        /// <param name="obj">The object whose values to read.</param>
        /// <param name="field">The field to read.</param>
        private string GetDebugValue(object obj, FieldInfo field)
        {
            try
            {
                return this.Stringify(field.GetValue(obj));
            }
            catch (Exception ex)
            {
                return $"error reading field: {ex.Message}";
            }
        }

        /// <summary>Get a human-readable value for a debug value.</summary>
        /// <param name="obj">The object whose values to read.</param>
        /// <param name="property">The property to read.</param>
        private string GetDebugValue(object obj, PropertyInfo property)
        {
            try
            {
                return this.Stringify(property.GetValue(obj));
            }
            catch (Exception ex)
            {
                return $"error reading property: {ex.Message}";
            }
        }
    }
}
