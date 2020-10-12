using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups
{
    /// <summary>Provides lookup data for in-game entities.</summary>
    internal interface ILookupProvider
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get positional metadata about possible lookup targets.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="lookupTile">The tile being looked up.</param>
        public IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile);

        /// <summary>Get a subject which provides lookup info.</summary>
        /// <param name="menu">The active menu.</param>
        /// <param name="cursorX">The cursor's viewport-relative X coordinate.</param>
        /// <param name="cursorY">The cursor's viewport-relative Y coordinate.</param>
        public ISubject GetSubject(IClickableMenu menu, int cursorX, int cursorY);

        /// <summary>Get the subject for an in-game entity, if available.</summary>
        /// <param name="entity">The entity instance.</param>
        public ISubject GetSubjectFor(object entity);

        /// <summary>Get all known subjects for the search UI.</summary>
        public IEnumerable<ISubject> GetSearchSubjects();
    }
}
