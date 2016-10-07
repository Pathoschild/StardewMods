using System;
using System.Linq;
using ChestsAnywhere.Common;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Menus
{
    /// <summary>A UI which lets the player edit a chest's data.</summary>
    internal class EditChestForm : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The chest to edit.</summary>
        private readonly ManagedChest Chest;

        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>The editable chest name.</summary>
        private ValidatedTextBox NameField;

        /// <summary>The field order.</summary>
        private ValidatedTextBox OrderField;

        /// <summary>The editable category name.</summary>
        private ValidatedTextBox CategoryField;

        /// <summary>The checkbox which indicates whether to hide the chest.</summary>
        private Checkbox HideChestField;

        /// <summary>The button which saves the settings.</summary>
        private ClickableTextureComponent SaveButton;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest to edit.</param>
        /// <param name="config">The mod configuration.</param>
        public EditChestForm(ManagedChest chest, ModConfig config)
        {
            this.Chest = chest;
            this.Config = config;
            this.ReinitialiseComponents();
        }

        /// <summary>The method invoked when the player left-clicks on the form.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Returns whether the edit UI is still active.</returns>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // name field
            if (this.NameField.GetBounds().Contains(x, y))
                this.NameField.Select();

            // category field
            else if (this.CategoryField.GetBounds().Contains(x, y))
                this.CategoryField.Select();

            // order field
            else if (this.OrderField.GetBounds().Contains(x, y))
                this.OrderField.Select();

            // 'hide chest' checkbox
            else if (this.HideChestField.GetBounds().Contains(x, y))
                this.HideChestField.Toggle();

            // save button
            else if (this.SaveButton.containsPoint(x, y))
            {
                this.Save();
                this.exitThisMenu();
            }
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="input">The key that was pressed.</param>
        public override void receiveKeyPress(Keys input)
        {
            this.ReceiveKey(input, this.Config.Keyboard);
        }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="input">The button that was pressed.</param>
        public override void receiveGamePadButton(Buttons input)
        {
            this.ReceiveKey(input, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <typeparam name="T">The key type.</typeparam>
        /// <param name="input">The key that was pressed.</param>
        /// <param name="config">The input configuration.</param>
        public void ReceiveKey<T>(T input, InputMapConfiguration<T> config)
        {
            // ignore invalid input
            if (!config.IsValidKey(input))
                return;

            // handle input
            if (input.Equals(Keys.Escape))
                this.exitThisMenu();
        }

        /// <summary>The method invoked when the player left-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        /// <summary>The method invoked when the game window is resized.</summary>
        /// <param name="oldBounds">The previous window dimensions.</param>
        /// <param name="newBounds">The new window dimensions.</param>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.ReinitialiseComponents();
        }

        /// <summary>The method invoked when the cursor is hovered over the control.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        public override void performHoverAction(int x, int y)
        {
            this.SaveButton.tryHover(x, y);
        }

        /// <summary>Render the UI.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        public override void draw(SpriteBatch batch)
        {
            // get initial measurements
            SpriteFont font = Game1.smallFont;
            const int gutter = 10;
            int padding = Game1.tileSize / 2;
            float topOffset = padding;
            int maxLabelWidth = (int)new[] { "Location:", "Name:", "Category:", "Order:" }.Select(p => font.MeasureString(p).X).Max();

            // background
            batch.DrawMenuBackground(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));

            // Location name
            {
                Vector2 labelSize = batch.DrawTextBlock(font, $"Location:", new Vector2(this.xPositionOnScreen + padding + (int)(maxLabelWidth - font.MeasureString("Location:").X), this.yPositionOnScreen + topOffset), this.width);
                batch.DrawTextBlock(font, this.Chest.LocationName, new Vector2(this.xPositionOnScreen + padding + maxLabelWidth + gutter, this.yPositionOnScreen + topOffset), this.width);
                topOffset += labelSize.Y;
            }

            // editable text fields
            var fields = new[] { Tuple.Create("Name:", this.NameField), Tuple.Create("Category:", this.CategoryField), Tuple.Create("Order:", this.OrderField) }.Where(p => p != null);
            foreach (var field in fields)
            {
                // get data
                string label = field.Item1;
                ValidatedTextBox textbox = field.Item2;

                // draw label
                Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(this.xPositionOnScreen + padding + (int)(maxLabelWidth - font.MeasureString(label).X), this.yPositionOnScreen + topOffset + 7), this.width);
                textbox.X = this.xPositionOnScreen + padding + maxLabelWidth + gutter;
                textbox.Y = this.yPositionOnScreen + (int)topOffset;
                textbox.Draw(batch);

                // update offset
                topOffset += Math.Max(labelSize.Y + 7, textbox.Height);
            }

            // hide chest checkbox
            {
                this.HideChestField.X = this.xPositionOnScreen + padding;
                this.HideChestField.Y = this.yPositionOnScreen + (int)topOffset;
                this.HideChestField.Width = 24;
                this.HideChestField.Draw(batch);
                Vector2 labelSize = batch.DrawTextBlock(font, "Hide this chest" + (this.HideChestField.Value ? " (you'll need to find the chest to undo this!)" : ""), new Vector2(this.xPositionOnScreen + padding + 7 + this.HideChestField.Width, this.yPositionOnScreen + topOffset), this.width, this.HideChestField.Value ? Color.Red : Color.Black);
                topOffset += Math.Max(this.HideChestField.Width, labelSize.Y);
            }

            // save button
            this.SaveButton.bounds = new Rectangle(this.xPositionOnScreen + padding, this.yPositionOnScreen + (int)topOffset, this.SaveButton.bounds.Width, this.SaveButton.bounds.Height);
            this.SaveButton.draw(batch);

            // exit button
            base.draw(batch);

            // cursor
            this.drawMouse(batch);
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
            this.Chest.Update(this.NameField.Text, this.CategoryField.Text, order, this.HideChestField.Value);
        }

        /****
        ** Drawing
        ****/
        /// <summary>Build the UI layout.</summary>
        private void ReinitialiseComponents()
        {
            // calculate dimensions
            this.width = Game1.tileSize * 13;
            this.height = Game1.tileSize * 7 + Game1.tileSize / 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2 + Game1.tileSize;

            // build components
            this.initializeUpperRightCloseButton();
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.NameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Name };
            this.CategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Category };
            this.OrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit) { Width = (int)Game1.smallFont.MeasureString("9999999").X, Text = this.Chest.Order?.ToString() };
            this.HideChestField = new Checkbox(this.Chest.IsIgnored);
            this.SaveButton = new ClickableTextureComponent("save-chest", new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), null, "OK", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, IClickableMenu.borderWithDownArrowIndex), 1f);
        }
    }
}
