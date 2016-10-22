using System;
using System.Linq;
using System.Reflection;
using ChestsAnywhere.Common;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Menus.Overlays
{
    /// <summary>An overlay for <see cref="ItemGrabMenu"/> which lets the player navigate and edit chests.</summary>
    internal class ManageChestOverlay : BaseOverlay
    {
        /*********
        ** Properties
        *********/
        /****
        ** Data
        ****/
        /// <summary>The available chests.</summary>
        private readonly ManagedChest[] Chests;

        /// <summary>The selected chest.</summary>
        private readonly ManagedChest Chest;

        /// <summary>The number of draw cycles since the menu was initialised.</summary>
        private int DrawCount;

        /// <summary>The backing field for <see cref="ActiveElement"/>; shouldn't be edited directly.</summary>
        private Element _activeElement;

        /// <summary>The overlay element which should receive input.</summary>
        private Element ActiveElement
        {
            get { return this._activeElement; }
            set
            {
                this._activeElement = value;
                this.SetItemsClickable(this._activeElement == Element.Menu);
            }
        }

        /// <summary>The unique chest categories.</summary>
        private readonly string[] Groups;

        /// <summary>The name of the selected group.</summary>
        private string SelectedGroup => this.Chest.GetGroup();

        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>Whether to show the group tab.</summary>
        private bool ShowGroupTab => this.Groups.Length > 1;

        /****
        ** Menu management
        ****/
        /// <summary>The underlying chest menu.</summary>
        private readonly ItemGrabMenu Menu;

        /// <summary>The underlying menu's player inventory submenu.</summary>
        private readonly InventoryMenu MenuInventoryMenu;

        /// <summary>The default highlight function for the chest items.</summary>
        private readonly InventoryMenu.highlightThisItem DefaultChestHighlighter;

        /// <summary>The default highlight function for the player inventory items.</summary>
        private readonly InventoryMenu.highlightThisItem DefaultInventoryHighlighter;

        /****
        ** Access UI
        ****/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font = Game1.smallFont;

        /// <summary>The chest selector tab.</summary>
        private Tab ChestTab;

        /// <summary>The group selector tab.</summary>
        private Tab GroupTab;

        /// <summary>The chest selector dropdown.</summary>
        private DropList<ManagedChest> ChestSelector;

        /// <summary>The group selector dropdown.</summary>
        private DropList<string> GroupSelector;

        /// <summary>The edit button.</summary>
        private ClickableTextureComponent EditButton;

        /// <summary>The button which sorts the player inventory.</summary>
        private ClickableTextureComponent SortInventoryButton;

        /****
        ** Edit UI
        ****/
        /// <summary>The editable chest name.</summary>
        private ValidatedTextBox EditNameField;

        /// <summary>The field order.</summary>
        private ValidatedTextBox EditOrderField;

        /// <summary>The editable category name.</summary>
        private ValidatedTextBox EditCategoryField;

        /// <summary>The checkbox which indicates whether to hide the chest.</summary>
        private Checkbox EditHideChestField;

        /// <summary>The button which saves the edit form.</summary>
        private ClickableTextureComponent EditSaveButton;

        /// <summary>The top-right button which closes the edit form.</summary>
        public ClickableTextureComponent EditExitButton;


        /*********
        ** Accessors
        *********/
        /// <summary>An event raised when the player selects a chest.</summary>
        public event Action<ManagedChest> OnChestSelected;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="menu">The underlying chest menu.</param>
        /// <param name="chest">The selected chest.</param>
        /// <param name="chests">The available chests.</param>
        /// <param name="config">The mod configuration.</param>
        public ManageChestOverlay(ItemGrabMenu menu, ManagedChest chest, ManagedChest[] chests, ModConfig config)
            : base(keepAlive: () => Game1.activeClickableMenu is ItemGrabMenu)
        {
            // menu
            this.Menu = menu;
            this.MenuInventoryMenu = (InventoryMenu)typeof(ItemGrabMenu).GetField("ItemsToGrabMenu", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(menu);
            if (this.MenuInventoryMenu == null)
                throw new InvalidOperationException("The menu doesn't seem to have a player inventory.");
            this.DefaultChestHighlighter = menu.inventory.highlightMethod;
            this.DefaultInventoryHighlighter = this.MenuInventoryMenu.highlightMethod;

            // chests & config
            this.Chest = chest;
            this.Chests = chests;
            this.Groups = chests.Select(p => p.GetGroup()).Distinct().ToArray();
            this.Config = config;

            // components
            this.ReinitialiseComponents();
        }

        /// <summary>Sort the player's inventory.</summary>
        public void SortInventory()
        {
            ItemGrabMenu.organizeItemsInList(Game1.player.items);
            Game1.playSound("Ship");
        }

        /// <summary>Switch to the specified chest.</summary>
        /// <param name="chest">The chest to select.</param>
        public void SelectChest(ManagedChest chest)
        {
            this.OnChestSelected?.Invoke(chest);
        }

        /// <summary>Release all resources.</summary>
        public override void Dispose()
        {
            this.OnChestSelected = null;
            base.Dispose();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected override void Draw(SpriteBatch batch)
        {
            this.DrawCount++;
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // access mode
            if (!this.ActiveElement.HasFlag(Element.EditForm))
            {
                // tabs
                this.ChestTab.Draw(batch);
                this.GroupTab?.Draw(batch);

                // tab dropdowns
                if (this.ActiveElement == Element.ChestList)
                    this.ChestSelector.Draw(batch);
                if (this.ActiveElement == Element.GroupList)
                    this.GroupSelector.Draw(batch);

                // edit button
                this.EditButton.draw(batch);
                this.SortInventoryButton.draw(batch);
            }

            // edit mode
            else
            {
                // get initial measurements
                SpriteFont font = Game1.smallFont;
                const int gutter = 10;
                int padding = Game1.pixelZoom * 10;
                float topOffset = padding;
                int maxLabelWidth = (int)new[] { "Location:", "Name:", "Category:", "Order:" }.Select(p => font.MeasureString(p).X).Max();

                // background
                batch.DrawMenuBackground(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height));

                // Location name
                {
                    Vector2 labelSize = batch.DrawTextBlock(font, "Location:", new Vector2(bounds.X + padding + (int)(maxLabelWidth - font.MeasureString("Location:").X), bounds.Y + topOffset), bounds.Width);
                    batch.DrawTextBlock(font, this.Chest.LocationName, new Vector2(bounds.X + padding + maxLabelWidth + gutter, bounds.Y + topOffset), bounds.Width);
                    topOffset += labelSize.Y;
                }

                // editable text fields
                var fields = new[] { Tuple.Create("Name:", this.EditNameField), Tuple.Create("Category:", this.EditCategoryField), Tuple.Create("Order:", this.EditOrderField) }.Where(p => p != null);
                foreach (var field in fields)
                {
                    // get data
                    string label = field.Item1;
                    ValidatedTextBox textbox = field.Item2;

                    // draw label
                    Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(label).X), bounds.Y + topOffset + 7), bounds.Width);
                    textbox.X = bounds.X + padding + maxLabelWidth + gutter;
                    textbox.Y = bounds.Y + (int)topOffset;
                    textbox.Draw(batch);

                    // update offset
                    topOffset += Math.Max(labelSize.Y + 7, textbox.Height);
                }

                // hide chest checkbox
                {
                    this.EditHideChestField.X = bounds.X + padding;
                    this.EditHideChestField.Y = bounds.Y + (int)topOffset;
                    this.EditHideChestField.Width = 24;
                    this.EditHideChestField.Draw(batch);
                    Vector2 labelSize = batch.DrawTextBlock(font, "Hide this chest" + (this.EditHideChestField.Value ? " (you'll need to find the chest to undo this!)" : ""), new Vector2(bounds.X + padding + 7 + this.EditHideChestField.Width, bounds.Y + topOffset), this.Menu.width, this.EditHideChestField.Value ? Color.Red : Color.Black);
                    topOffset += Math.Max(this.EditHideChestField.Width, labelSize.Y);
                }

                // save button
                this.EditSaveButton.bounds = new Rectangle(bounds.X + padding, bounds.Y + (int)topOffset, this.EditSaveButton.bounds.Width, this.EditSaveButton.bounds.Height);
                this.EditSaveButton.draw(batch);

                // exit button
                this.EditExitButton.draw(batch);
            }

            // cursor
            this.DrawCursor();
        }


        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player resizes the game windoww.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected override void ReceiveGameWindowResized(xTile.Dimensions.Rectangle oldBounds, xTile.Dimensions.Rectangle newBounds)
        {
            this.ChestSelector.ReceiveGameWindowResized();
            this.GroupSelector?.ReceiveGameWindowResized();
            this.ReinitialiseComponents();
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="input">The key that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveKeyPress(Keys input)
        {
            return this.ReceiveKey(input, this.Config.Keyboard);
        }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveButtonPress(Buttons input)
        {
            // Get the current state of the connected Gamepad for Player 1 (The Player)
            var gamepad = GamePad.GetState(PlayerIndex.One);

            // Check if button pressed matches the select button for LeftClick
            if (gamepad.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.X))
                return ReceiveLeftClick(Game1.getMouseX(), Game1.getMouseY());

            // If it doesn't match the select button, proceed to check the button in ReceiveKey
            return this.ReceiveKey(input, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses a controller trigger.</summary>
        /// <param name="input">The trigger that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveTriggerPress(Buttons input)
        {
            return this.ReceiveKey(input, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <typeparam name="T">The key type.</typeparam>
        /// <param name="input">The key that was pressed.</param>
        /// <param name="config">The input configuration.</param>
        /// <returns>Whether the key has been handled and shouldn't be propagated further.</returns>
        public bool ReceiveKey<T>(T input, InputMapConfiguration<T> config)
        {
            // ignore invalid input
            if (this.IsInitialising() || !config.IsValidKey(input))
                return false;

            switch (this.ActiveElement)
            {
                case Element.Menu:
                    if (input.Equals(config.Toggle) || input.Equals(Keys.Escape) || input.Equals(Game1.options.menuButton))
                        this.Exit();
                    else if (input.Equals(config.PrevChest))
                        this.SelectPreviousChest();
                    else if (input.Equals(config.NextChest))
                        this.SelectNextChest();
                    else if (input.Equals(config.PrevCategory))
                        this.SelectPreviousCategory();
                    else if (input.Equals(config.NextCategory))
                        this.SelectNextCategory();
                    else if (input.Equals(config.SortItems))
                        this.SortInventory();
                    else
                        return false;
                    return true;

                case Element.ChestList:
                case Element.GroupList:
                case Element.EditForm:
                    if (input.Equals(Keys.Escape))
                        this.ActiveElement = Element.Menu;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>The method invoked when the player scrolls the dropdown using the mouse wheel.</summary>
        /// <param name="amount">The scroll direction.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveScrollWheelAction(int amount)
        {
            switch (this.ActiveElement)
            {
                case Element.ChestList:
                    this.ChestSelector.ReceiveScrollWheelAction(amount);
                    return true;

                case Element.GroupList:
                    this.GroupSelector?.ReceiveScrollWheelAction(amount);
                    return true;

                case Element.EditForm:
                    return true; // suppress input

                default:
                    return false;
            }
        }

        /// <summary>The method invoked when the player left-clicks.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveLeftClick(int x, int y)
        {
            switch (this.ActiveElement)
            {
                // edit form
                case Element.EditForm:
                    // name field
                    if (this.EditNameField.GetBounds().Contains(x, y))
                        this.EditNameField.Select();

                    // category field
                    else if (this.EditCategoryField.GetBounds().Contains(x, y))
                        this.EditCategoryField.Select();

                    // order field
                    else if (this.EditOrderField.GetBounds().Contains(x, y))
                        this.EditOrderField.Select();

                    // 'hide chest' checkbox
                    else if (this.EditHideChestField.GetBounds().Contains(x, y))
                        this.EditHideChestField.Toggle();

                    // save button
                    else if (this.EditSaveButton.containsPoint(x, y))
                    {
                        this.SaveEdit();
                        this.ActiveElement = Element.Menu;
                    }

                    // exit button
                    else if (this.EditExitButton.containsPoint(x, y))
                        this.ActiveElement = Element.Menu;

                    return true; // handle all clicks while open

                // chest list
                case Element.ChestList:
                    // close dropdown
                    this.ActiveElement = Element.Menu;

                    // select chest
                    if (this.ChestSelector.containsPoint(x, y))
                    {
                        ManagedChest chest = this.ChestSelector.Select(x, y);
                        if (chest != null)
                        {
                            this.SelectChest(chest);
                            this.ReinitialiseComponents();
                        }
                    }
                    return true; // handle all clicks while open

                // group list
                case Element.GroupList:
                    // close dropdown
                    this.ActiveElement = Element.Menu;

                    // select group
                    if (this.GroupSelector.containsPoint(x, y))
                    {
                        string group = this.GroupSelector.Select(x, y);
                        if (group != null && group != this.SelectedGroup)
                        {
                            this.SelectChest(this.Chests.First(chest => chest.GetGroup() == group));
                            this.ReinitialiseComponents();
                        }
                    }
                    return true; // handle all clicks while open

                // buttons & dropdown
                default:
                    if (this.EditButton.containsPoint(x, y))
                    {
                        this.EditNameField.Text = this.Chest.Name;
                        this.EditCategoryField.Text = this.Chest.GetGroup();
                        this.EditOrderField.Text = this.Chest.Order?.ToString();
                        this.EditHideChestField.Value = this.Chest.IsIgnored;

                        this.ActiveElement = Element.EditForm;
                    }
                    else if (this.SortInventoryButton.containsPoint(x, y))
                        this.SortInventory();
                    else if (this.ChestTab.containsPoint(x, y))
                        this.ActiveElement = Element.ChestList;
                    else if (this.GroupTab?.containsPoint(x, y) == true)
                        this.ActiveElement = Element.GroupList;
                    else
                        return false;
                    return true;
            }
        }

        /// <summary>The method invoked when the cursor is hovered.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveCursorHover(int x, int y)
        {
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    this.EditButton.tryHover(x, y);
                    this.SortInventoryButton.tryHover(x, y);
                    return false;

                case Element.EditForm:
                    this.EditSaveButton.tryHover(x, y);
                    this.EditExitButton.tryHover(x, y);
                    return true;

                case Element.ChestList:
                case Element.GroupList:
                    return true; // suppress menu hover

                default:
                    return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialise the edit-chest overlay for rendering.</summary>
        private void ReinitialiseComponents()
        {
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // group dropdown
            if (this.ShowGroupTab)
            {
                // tab
                Vector2 tabSize = Tab.GetTabSize(this.Font, this.SelectedGroup);
                this.GroupTab = new Tab(this.SelectedGroup, bounds.Right - (int)tabSize.X - Game1.tileSize, bounds.Y - Game1.pixelZoom * 25, true, this.Font);

                // dropdown
                this.GroupSelector = new DropList<string>(this.SelectedGroup, this.Groups, group => group, this.GroupTab.bounds.Right, this.GroupTab.bounds.Bottom, false, this.Font);
            }

            // chest dropdown
            {
                // tab
                this.ChestTab = new Tab(this.Chest.Name, bounds.X, bounds.Y - Game1.pixelZoom * 25, true, this.Font);

                // dropdown
                ManagedChest[] chests = this.Chests.Where(chest => !this.ShowGroupTab || chest.GetGroup() == this.SelectedGroup).ToArray();
                this.ChestSelector = new DropList<ManagedChest>(this.Chest, chests, chest => chest.Name, this.ChestTab.bounds.X, this.ChestTab.bounds.Bottom, true, this.Font);
            }

            // edit chest button overlay (based on chest dropdown position)
            {
                Rectangle sprite = Sprites.Icons.SpeechBubble;
                float zoom = Game1.pixelZoom / 2f;
                Rectangle buttonBounds = new Rectangle(this.ChestTab.bounds.X + this.ChestTab.bounds.Width, this.ChestTab.bounds.Y, (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.EditButton = new ClickableTextureComponent("edit-chest", buttonBounds, null, "edit chest", Sprites.Icons.Sheet, sprite, zoom);
            }

            // sort inventory button overlay (based on OK button position)
            {
                Rectangle sprite = Sprites.Buttons.Organize;
                ClickableTextureComponent okButton = this.Menu.okButton;
                float zoom = Game1.pixelZoom;
                Rectangle buttonBounds = new Rectangle(okButton.bounds.X, (int)(okButton.bounds.Y - sprite.Height * zoom - 5 * zoom), (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.SortInventoryButton = new ClickableTextureComponent("sort-inventory", buttonBounds, null, "sort inventory", Sprites.Icons.Sheet, sprite, zoom);
            }

            // edit form
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.EditNameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Name };
            this.EditCategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Category };
            this.EditOrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit) { Width = (int)Game1.smallFont.MeasureString("9999999").X, Text = this.Chest.Order?.ToString() };
            this.EditHideChestField = new Checkbox(this.Chest.IsIgnored);
            this.EditSaveButton = new ClickableTextureComponent("save-chest", new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), null, "OK", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, IClickableMenu.borderWithDownArrowIndex), 1f);
            this.EditExitButton = new ClickableTextureComponent(new Rectangle(bounds.Right - 9 * Game1.pixelZoom, bounds.Y - Game1.pixelZoom * 2, Sprites.Icons.ExitButton.Width * Game1.pixelZoom, Sprites.Icons.ExitButton.Height * Game1.pixelZoom), Sprites.Icons.Sheet, Sprites.Icons.ExitButton, Game1.pixelZoom);

            // adjust menu to fit
            this.Menu.trashCan.bounds.Y = this.SortInventoryButton.bounds.Y - this.Menu.trashCan.bounds.Height - 2 * Game1.pixelZoom;
        }

        /// <summary>Save the form input.</summary>
        private void SaveEdit()
        {
            // get order value
            int? order = null;
            {
                int parsed;
                if (int.TryParse(this.EditOrderField.Text, out parsed))
                    order = parsed;
            }

            // update chest
            this.Chest.Update(this.EditNameField.Text, this.EditCategoryField.Text, order, this.EditHideChestField.Value);
            this.OnChestSelected?.Invoke(this.Chest);
        }

        /// <summary>Exit the chest menu.</summary>
        private void Exit()
        {
            this.Dispose();
            this.Menu.exitThisMenu();
        }

        /// <summary>Switch to the previous chest in the list.</summary>
        private void SelectPreviousChest()
        {
            ManagedChest[] chests = GetChestsFromCategory(SelectedGroup);
            int cur = Array.IndexOf(chests, this.Chest);
            this.SelectChest(chests[cur != 0 ? cur - 1 : chests.Length - 1]);
        }

        /// <summary>Switch to the next chest in the list.</summary>
        private void SelectNextChest()
        {
            ManagedChest[] chests = GetChestsFromCategory(SelectedGroup);
            int cur = Array.IndexOf(chests, this.Chest);
            this.SelectChest(chests[(cur + 1) % chests.Length]);
        }

        private void SelectPreviousCategory()
        {
            int cur = Array.IndexOf(Groups, SelectedGroup);
            this.SelectChest(Chests.First(chest => chest.GetGroup() == Groups[cur != 0 ? cur - 1 : Groups.Length - 1]));
        }

        private void SelectNextCategory()
        {
            int cur = Array.IndexOf(Groups, SelectedGroup);
            this.SelectChest(Chests.First(chest => chest.GetGroup() == Groups[(cur + 1) % Groups.Length]));
        }

        /// <summary>Returns all chests of the specified category / group</summary>
        private ManagedChest[] GetChestsFromCategory(string category)
        {
            return this.Chests.Where(chest => chest.GetGroup() == category).ToArray();
        }

        /// <summary>Get whether the menu is initialising itself.</summary>
        private bool IsInitialising()
        {
            return this.DrawCount < 10;
        }

        /// <summary>Set whether the chest or inventory items should be clickable.</summary>
        private void SetItemsClickable(bool clickable)
        {
            if (clickable)
            {
                this.Menu.inventory.highlightMethod = this.DefaultChestHighlighter;
                this.MenuInventoryMenu.highlightMethod = this.DefaultInventoryHighlighter;
            }
            else
            {
                this.Menu.inventory.highlightMethod = item => false;
                this.MenuInventoryMenu.highlightMethod = item => false;
            }
        }
    }
}
