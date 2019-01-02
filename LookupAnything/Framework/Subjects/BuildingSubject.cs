using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a constructed building.</summary>
    internal class BuildingSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The lookup target.</summary>
        private readonly Building Target;

        /// <summary>The building's source rectangle in its spritesheet.</summary>
        private readonly Rectangle SourceRectangle;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="building">The lookup target.</param>
        /// <param name="sourceRectangle">The building's source rectangle in its spritesheet.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public BuildingSubject(GameHelper gameHelper, Building building, Rectangle sourceRectangle, ITranslationHelper translations, IReflectionHelper reflectionHelper)
            : base(gameHelper, building.buildingType.Value, null, translations.Get(L10n.Types.Building), translations)
        {
            this.Reflection = reflectionHelper;
            this.Target = building;
            this.SourceRectangle = sourceRectangle;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            Building building = this.Target;
            yield break;
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
        {
            Building target = this.Target;

            // pinned fields
            yield return new GenericDebugField("building type", target.buildingType.Value, pinned: true);
            yield return new GenericDebugField("days of construction left", target.daysOfConstructionLeft.Value, pinned: true);
            yield return new GenericDebugField("name of indoors", target.nameOfIndoors, pinned: true);

            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(target))
                yield return field;
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        /// <remarks>Derived from <see cref="Building.drawInMenu"/>, modified to draw within the target size.</remarks>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            Building target = this.Target;
            spriteBatch.Draw(target.texture.Value, position, this.SourceRectangle, target.color.Value, 0.0f, Vector2.Zero, size.X / this.SourceRectangle.Width, SpriteEffects.None, 0.89f);
            return true;
        }
    }
}
