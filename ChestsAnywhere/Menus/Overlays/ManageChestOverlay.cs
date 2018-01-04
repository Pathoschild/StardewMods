using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays
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
        /// <summary>Provides translations stored in the mod's folder.</summary>
        private readonly ITranslationHelper Translations;

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
            get => this._activeElement;
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

        /// <summary>Whether the chest menu is ready to close.</summary>
        private bool CanCloseChest => this.Menu.readyToClose();


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
        /// <param name="translations">Provides translations stored in the mod's folder.</param>
        public ManageChestOverlay(ItemGrabMenu menu, ManagedChest chest, ManagedChest[] chests, ModConfig config, ITranslationHelper translations)
            : base(keepAlive: () => Game1.activeClickableMenu is ItemGrabMenu)
        {
            // menu
            this.Menu = menu;
            this.MenuInventoryMenu = menu.ItemsToGrabMenu;
            this.DefaultChestHighlighter = menu.inventory.highlightMethod;
            this.DefaultInventoryHighlighter = this.MenuInventoryMenu.highlightMethod;

            // chests & config
            this.Chest = chest;
            this.Chests = chests;
            this.Groups = chests.Select(p => p.GetGroup()).Distinct().OrderBy(p => p).ToArray();
            this.Config = config;

            // translations
            this.Translations = translations;

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
                float navOpacity = this.CanCloseChest ? 1f : 0.5f;

                // tabs
                this.ChestTab.Draw(batch, navOpacity);
                this.GroupTab?.Draw(batch, navOpacity);

                // tab dropdowns
                if (this.ActiveElement == Element.ChestList)
                    this.ChestSelector.Draw(batch, navOpacity);
                if (this.ActiveElement == Element.GroupList)
                    this.GroupSelector.Draw(batch, navOpacity);

                // edit button
                if (this.Chest.Container.IsEditable)
                    this.EditButton.draw(batch, Color.White * navOpacity, 1f);
                this.SortInventoryButton.draw(batch, Color.White * navOpacity, 1f);
            }

            // edit mode
            else
            {
                // get translations
                string locationLabel = this.Translations.Get("label.location") + ":";
                string nameLabel = this.Translations.Get("label.name") + ":";
                string categoryLabel = this.Translations.Get("label.category") + ":";
                string orderLabel = this.Translations.Get("label.order") + ":";

                // get initial measurements
                SpriteFont font = Game1.smallFont;
                const int gutter = 10;
                int padding = Game1.pixelZoom * 10;
                float topOffset = padding;
                int maxLabelWidth = (int)new[] { locationLabel, nameLabel, categoryLabel, orderLabel }.Select(p => font.MeasureString(p).X).Max();

                // background
                batch.DrawMenuBackground(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height));

                // Location name
                {
                    string locationName = this.Chest.Location.Name;
                    if (this.Chest.Tile != Vector2.Zero)
                        locationName += " (" + this.Translations.Get("label.location.tile", new { x = this.Chest.Tile.X, y = this.Chest.Tile.Y }) + ")";

                    Vector2 labelSize = batch.DrawTextBlock(font, locationLabel, new Vector2(bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(locationLabel).X), bounds.Y + topOffset), bounds.Width);
                    batch.DrawTextBlock(font, locationName, new Vector2(bounds.X + padding + maxLabelWidth + gutter, bounds.Y + topOffset), bounds.Width);
                    topOffset += labelSize.Y;
                }

                // editable text fields
                var fields = new[] { Tuple.Create(nameLabel, this.EditNameField), Tuple.Create(categoryLabel, this.EditCategoryField), Tuple.Create(orderLabel, this.EditOrderField) }.Where(p => p != null);
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
                    string label = this.Translations.Get(this.EditHideChestField.Value ? "label.hide-chest-hidden" : "label.hide-chest");
                    Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(bounds.X + padding + 7 + this.EditHideChestField.Width, bounds.Y + topOffset), this.Menu.width, this.EditHideChestField.Value ? Color.Red : Color.Black);
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

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveButtonPress(SButton input)
        {
            if (this.IsInitialising())
                return false;

            bool canNavigate = this.CanCloseChest;
            var controls = this.Config.Controls;
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    if (controls.Toggle.Contains(input) || input == SButton.Escape || input == SButton.ControllerB)
                    {
                        if (canNavigate)
                            this.Exit();
                    }
                    else if (controls.PrevChest.Contains(input) && canNavigate)
                        this.SelectPreviousChest();
                    else if (controls.NextChest.Contains(input) && canNavigate)
                        this.SelectNextChest();
                    else if (controls.PrevCategory.Contains(input) && canNavigate)
                        this.SelectPreviousCategory();
                    else if (controls.NextCategory.Contains(input) && canNavigate)
                        this.SelectNextCategory();
                    else if (controls.EditChest.Contains(input) && canNavigate)
                        this.OpenEdit();
                    else if (controls.SortItems.Contains(input))
                        this.SortInventory();
                    else
                        return false;
                    return true;

                case Element.ChestList:
                case Element.GroupList:
                case Element.EditForm:
                    if (input == SButton.Escape || input == SButton.ControllerB)
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
                    bool canNavigate = this.CanCloseChest;
                    if (this.Chest.Container.IsEditable && this.EditButton.containsPoint(x, y) && canNavigate)
                        this.OpenEdit();
                    else if (this.SortInventoryButton.containsPoint(x, y))
                        this.SortInventory();
                    else if (this.ChestTab.containsPoint(x, y) && canNavigate)
                        this.ActiveElement = Element.ChestList;
                    else if (this.GroupTab?.containsPoint(x, y) == true && canNavigate)
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
                    if (this.Chest.Container.IsEditable)
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
            if (this.Chest.Container.IsEditable)
            {
                Rectangle sprite = Sprites.Icons.SpeechBubble;
                float zoom = Game1.pixelZoom / 2f;
                Rectangle buttonBounds = new Rectangle(this.ChestTab.bounds.X + this.ChestTab.bounds.Width, this.ChestTab.bounds.Y, (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.EditButton = new ClickableTextureComponent("edit-chest", buttonBounds, null, this.Translations.Get("button.edit-chest"), Sprites.Icons.Sheet, sprite, zoom);
            }

            // sort inventory button overlay (based on OK button position)
            {
                Rectangle sprite = Sprites.Buttons.Organize;
                ClickableTextureComponent okButton = this.Menu.okButton;
                float zoom = Game1.pixelZoom;
                Rectangle buttonBounds = new Rectangle(okButton.bounds.X, (int)(okButton.bounds.Y - sprite.Height * zoom - 5 * zoom), (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.SortInventoryButton = new ClickableTextureComponent("sort-inventory", buttonBounds, null, this.Translations.Get("button.sort-inventory"), Sprites.Icons.Sheet, sprite, zoom);
            }

            // edit form
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.EditNameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Name };
            this.EditCategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Category };
            this.EditOrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit) { Width = (int)Game1.smallFont.MeasureString("9999999").X, Text = this.Chest.Order?.ToString() };
            this.EditHideChestField = new Checkbox(this.Chest.IsIgnored);
            this.EditSaveButton = new ClickableTextureComponent("save-chest", new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), null, this.Translations.Get("button.ok"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, IClickableMenu.borderWithDownArrowIndex), 1f);
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
                if (int.TryParse(this.EditOrderField.Text, out int parsed))
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

        /// <summary>Get the index of a chest in the selected group.</summary>
        /// <param name="chest">The chest to find.</param>
        /// <param name="chests">The chests to search.</param>
        private int GetChestIndex(ManagedChest chest, IEnumerable<ManagedChest> chests)
        {
            int i = 0;
            foreach (ManagedChest cur in chests)
            {
                if (cur.Container.IsSameAs(chest.Container))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>Switch to the previous chest in the list.</summary>
        private void SelectPreviousChest()
        {
            ManagedChest[] chests = this.GetChestsFromCategory(this.SelectedGroup);
            int curIndex = this.GetChestIndex(this.Chest, chests);
            this.SelectChest(chests[curIndex != 0 ? curIndex - 1 : chests.Length - 1]);
        }

        /// <summary>Switch to the next chest in the list.</summary>
        private void SelectNextChest()
        {
            ManagedChest[] chests = this.GetChestsFromCategory(this.SelectedGroup);
            int curIndex = this.GetChestIndex(this.Chest, chests);
            this.SelectChest(chests[(curIndex + 1) % chests.Length]);
        }

        /// <summary>Switch to the previous category.</summary>
        private void SelectPreviousCategory()
        {
            int curIndex = Array.IndexOf(this.Groups, this.SelectedGroup);
            string group = this.Groups[curIndex != 0 ? curIndex - 1 : this.Groups.Length - 1];
            this.SelectChest(this.Chests.First(chest => chest.GetGroup() == group));
        }

        /// <summary>Switch to the next category.</summary>
        private void SelectNextCategory()
        {
            int curIndex = Array.IndexOf(this.Groups, this.SelectedGroup);
            string group = this.Groups[(curIndex + 1) % this.Groups.Length];
            this.SelectChest(this.Chests.First(chest => chest.GetGroup() == group));
        }

        /// <summary>Reset and display the edit screen.</summary>
        private void OpenEdit()
        {
            this.EditNameField.Text = this.Chest.Name;
            this.EditCategoryField.Text = this.Chest.GetGroup();
            this.EditOrderField.Text = this.Chest.Order?.ToString();
            this.EditHideChestField.Value = this.Chest.IsIgnored;

            this.ActiveElement = Element.EditForm;
        }

        /// <summary>Get the chests in a given category.</summary>
        /// <param name="category">The chest category.</param>
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
