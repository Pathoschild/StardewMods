using ChestsAnywhere.Common;
using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Components
{
    /// <summary>A UI which lets the player edit a chest's data.</summary>
    internal class EditChestForm
    {
        /*********
        ** Properties
        *********/
        /// <summary>The chest to edit.</summary>
        private readonly ManagedChest Chest;

        /// <summary>The name input box.</summary>
        private TextBox NameInput;

        /// <summary>The button which saves the settings.</summary>
        private ClickableTextureComponent SaveButton;

        /// <summary>The bounds within which to draw the form.</summary>
        private Rectangle Bounds;

        /// <summary>The blurb shown when in the edit UI.</summary>
        private readonly string ChestBlurb;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the edit form is active.</summary>
        public bool IsActive { get; private set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest to edit.</param>
        public EditChestForm(ManagedChest chest)
        {
            this.Chest = chest;
            this.ChestBlurb = $"This chest is in the '{chest.Location}' location and has {chest.Chest.items.Count} items.";
            this.BuildLayout();
        }

        /// <summary>The method invoked when the player left-clicks on the form.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Returns whether the edit UI is still active.</returns>
        public void ReceiveLeftClick(int x, int y)
        {
            if (this.GetBounds(this.NameInput).Contains(x, y))
                this.NameInput.SelectMe();
            else if (this.SaveButton.containsPoint(x, y))
            {
                this.Save();
                this.IsActive = false;
            }
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public void Draw(SpriteBatch sprites)
        {
            SpriteFont font = Game1.smallFont;
            int padding = Game1.tileSize / 2;

            // background
            sprites.DrawMenuBackground(this.Bounds);

            // blurb
            sprites.DrawTextBlock(font, this.ChestBlurb, new Vector2(this.Bounds.X + padding, this.Bounds.Y + Game1.tileSize / 2), this.Bounds.Width - Game1.tileSize);

            // name
            this.NameInput.Draw(sprites);
            sprites.DrawTextBlock(font, "Name:", new Vector2(this.Bounds.X + padding, this.NameInput.Y + 7), this.Bounds.Width);

            // save button
            this.SaveButton.draw(sprites);

            // tips
            sprites.DrawTextBlock(font, "Tip: change the order of chests by adding a piped number to the name, like |1|. The number won't be shown in the dropdown.", new Vector2(this.Bounds.X + padding, this.SaveButton.bounds.Y + this.SaveButton.bounds.Height + 10), this.Bounds.Width - padding * 2, Color.Gray);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Behaviour
        ****/
        /// <summary>Save the form input.</summary>
        private void Save()
        {
            this.Chest.Chest.Name = this.NameInput.Text;
        }

        /****
        ** Drawing
        ****/
        /// <summary>Build the UI layout.</summary>
        private void BuildLayout()
        {
            // form bounds
            int width = Game1.tileSize * 13;
            int height = Game1.tileSize * 7 + Game1.tileSize / 2;
            int x = Game1.viewport.Width / 2 - width / 2;
            int y = Game1.viewport.Height / 2 - height / 2 + Game1.tileSize;
            this.Bounds = new Rectangle(x, y, width, height);
            int padding = Game1.tileSize / 2;

            // blurb
            int blurbHeight = (int)Game1.smallFont.MeasureString(this.ChestBlurb).Y;

            // name
            {
                this.NameInput = new TextBox(Sprites.Textbox.Sheet, null, Game1.smallFont, Color.Black)
                {
                    X = this.Bounds.X + padding + (int)Game1.smallFont.MeasureString("Name:").X,
                    Y = this.Bounds.Y + padding + blurbHeight,
                    Width = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X,
                    Text = this.Chest.Chest.Name
                };
            }

            // save button
            this.SaveButton = new ClickableTextureComponent(new Rectangle(this.Bounds.X + padding, this.Bounds.Y + blurbHeight + this.NameInput.Height + Game1.tileSize, Game1.tileSize, Game1.tileSize), "OK", "OK", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, IClickableMenu.borderWithDownArrowIndex), 1f, false);
        }

        /// <summary>Get the bounds for an input element.</summary>
        /// <param name="element">The input element.</param>
        private Rectangle GetBounds(TextBox element)
        {
            return new Rectangle(element.X, element.Y, element.Width, element.Height);
        }
    }
}
