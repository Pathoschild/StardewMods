using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups
{
    /// <inheritdoc />
    internal abstract class BaseLookupProvider : ILookupProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        protected readonly IReflectionHelper Reflection;

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        protected readonly GameHelper GameHelper;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            yield break;
        }

        /// <inheritdoc />
        public virtual ISubject GetSubject(IClickableMenu menu, int cursorX, int cursorY)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual ISubject GetSubjectFor(object entity)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual IEnumerable<ISubject> GetSearchSubjects()
        {
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        protected BaseLookupProvider(IReflectionHelper reflection, GameHelper gameHelper)
        {
            this.Reflection = reflection;
            this.GameHelper = gameHelper;
        }
    }
}
