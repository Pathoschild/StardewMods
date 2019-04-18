using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a movie snack.</summary>
    internal class MovieSnackSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The lookup target.</summary>
        private readonly MovieConcession Target;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="item">The underlying target.</param>
        public MovieSnackSubject(GameHelper gameHelper, ITranslationHelper translations, MovieConcession item)
            : base(gameHelper, translations)
        {
            this.Target = item;
            this.Initialize(item.DisplayName, item.getDescription(), L10n.Types.Other());
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            MovieConcession item = this.Target;

            // date's taste
            NPC date = Game1.player.team.movieInvitations.FirstOrDefault(p => p.farmer == Game1.player)?.invitedNPC;
            if (date != null)
            {
                string taste = MovieTheater.GetConcessionTasteForCharacter(date, item);
                yield return new GenericField(this.GameHelper, L10n.MovieSnack.Preference(), L10n.MovieSnack.ForTaste(taste, date.Name));
            }
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
        {
            foreach (IDebugField field in this.GetDebugFieldsFrom(this.Target))
                yield return field;
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            this.Target.drawInMenu(spriteBatch, position, 1, 1f, 1f, StackDrawType.Hide, Color.White, false);
            return true;
        }
    }
}
