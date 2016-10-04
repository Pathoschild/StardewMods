using System;
using System.Linq;
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

        /// <summary>The bounds within which to draw the form.</summary>
        private Rectangle Bounds;

        /// <summary>The editable chest name.</summary>
        private readonly TextBox NameField;

        /// <summary>The field order.</summary>
        private readonly TextBox OrderField;

        /// <summary>The editable category name.</summary>
        private readonly TextBox CategoryField;

        /// <summary>The button which saves the settings.</summary>
        private readonly ClickableTextureComponent SaveButton;


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
            // initialise
            this.Chest = chest;
            this.CalculateDimensions();

            // build components
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.NameField = new TextBox(Sprites.Textbox.Sheet, null, Game1.smallFont, Color.Black) { Width = longTextWidth, Text = this.Chest.Name };
            this.CategoryField = new TextBox(Sprites.Textbox.Sheet, null, Game1.smallFont, Color.Black) { Width = longTextWidth, Text = this.Chest.Category };
            this.OrderField = new TextBox(Sprites.Textbox.Sheet, null, Game1.smallFont, Color.Black) { Width = (int)Game1.smallFont.MeasureString("9999999").X, Text = this.Chest.Order?.ToString() };
            this.SaveButton = new ClickableTextureComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "OK", "OK", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, IClickableMenu.borderWithDownArrowIndex), 1f, false);
        }

        /// <summary>The method invoked when the player left-clicks on the form.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Returns whether the edit UI is still active.</returns>
        public void ReceiveLeftClick(int x, int y)
        {
            // name field
            if (this.GetBounds(this.NameField).Contains(x, y))
                this.NameField.SelectMe();

            // category field
            else if (this.GetBounds(this.CategoryField).Contains(x, y))
                this.CategoryField.SelectMe();

            else if(this.GetBounds(this.OrderField).Contains(x, y))
                this.OrderField.SelectMe();

            // save button
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
            // get initial measurements
            SpriteFont font = Game1.smallFont;
            int padding = Game1.tileSize / 2;
            float topOffset = padding;

            // background
            sprites.DrawMenuBackground(this.Bounds);

            // blurb
            {
                Vector2 size = sprites.DrawTextBlock(font, $"This chest is in the '{this.Chest.LocationName}' location and has {this.Chest.Chest.items.Count} items.", new Vector2(this.Bounds.X + padding, this.Bounds.Y + topOffset), this.Bounds.Width - Game1.tileSize);
                topOffset += size.Y;
            }

            // editable fields
            var fields = new[] { Tuple.Create("Name:", this.NameField), Tuple.Create("Category:", this.CategoryField), Tuple.Create("Order:", this.OrderField) };
            int maxLabelWidth = (int)fields.Select(p => font.MeasureString(p.Item1).X).Max();
            foreach (var field in fields)
            {
                // get data
                string label = field.Item1;
                TextBox textbox = field.Item2;

                // draw label
                Vector2 labelSize = sprites.DrawTextBlock(font, label, new Vector2(this.Bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(label).X), this.Bounds.Y + topOffset + 7), this.Bounds.Width);
                textbox.X = this.Bounds.X + padding + maxLabelWidth + 10;
                textbox.Y = this.Bounds.Y + (int)topOffset;
                textbox.Draw(sprites);

                // update offset
                topOffset += Math.Max(labelSize.Y + 7, textbox.Height);
            }
            
            // save button
            this.SaveButton.bounds = new Rectangle(this.Bounds.X + padding, this.Bounds.Y + (int)topOffset, this.SaveButton.bounds.Width, this.SaveButton.bounds.Height);
            this.SaveButton.draw(sprites);
            topOffset += this.SaveButton.bounds.Height;
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
            // get order value
            int? order = null;
            {
                int parsed;
                if (int.TryParse(this.OrderField.Text, out parsed))
                    order = parsed;
            }

            // update chest
            this.Chest.Update(this.NameField.Text, this.CategoryField.Text, order, this.Chest.IsIgnored);
        }

        /****
        ** Drawing
        ****/
        /// <summary>Build the UI layout.</summary>
        private void CalculateDimensions()
        {
            int width = Game1.tileSize * 13;
            int height = Game1.tileSize * 7 + Game1.tileSize / 2;
            int x = Game1.viewport.Width / 2 - width / 2;
            int y = Game1.viewport.Height / 2 - height / 2 + Game1.tileSize;
            this.Bounds = new Rectangle(x, y, width, height);
        }

        /// <summary>Get the bounds for an input element.</summary>
        /// <param name="element">The input element.</param>
        private Rectangle GetBounds(TextBox element)
        {
            return new Rectangle(element.X, element.Y, element.Width, element.Height);
        }
    }
}
