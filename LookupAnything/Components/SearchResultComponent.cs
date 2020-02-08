using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components
{
    /// <summary>A clickable component representing a search result.</summary>
    internal class SearchResultComponent : ClickableComponent
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The subject to display.</summary>
        public ISubject Subject { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="subject">The subject to display.</param>
        public SearchResultComponent(ISubject subject)
            : base(Rectangle.Empty, subject.Name)
        {
            this.Subject = subject;
        }

        /// <summary>Draw the search result to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw the search result.</param>
        /// <param name="width">The width to draw.</param>
        /// <param name="highlight">Whether to highlight the search result.</param>
        public Vector2 Draw(SpriteBatch spriteBatch, Vector2 position, int width, bool highlight = false)
        {
            // update bounds
            this.bounds.X = (int)position.X;
            this.bounds.Y = (int)position.Y;
            this.bounds.Width = width;
            this.bounds.Height = 70;
            const int borderWidth = 2;
            int iconSize = 70;
            int topPadding = this.bounds.Height / 2;

            // draw
            if (highlight)
                spriteBatch.DrawLine(this.bounds.X, this.bounds.Y, new Vector2(this.bounds.Width, this.bounds.Height), Color.Beige);
            spriteBatch.DrawLine(this.bounds.X, this.bounds.Y, new Vector2(this.bounds.Width, borderWidth), Color.Black); // border
            spriteBatch.DrawTextBlock(Game1.smallFont, $"{this.Subject.Name} ({this.Subject.Type})", new Vector2(this.bounds.X, this.bounds.Y) + new Vector2(iconSize, topPadding), this.bounds.Width - iconSize); // text
            this.Subject.DrawPortrait(spriteBatch, position, new Vector2(iconSize)); // icon

            // return size
            return new Vector2(this.bounds.Width, this.bounds.Height);
        }
    }
}
