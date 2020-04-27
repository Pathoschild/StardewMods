using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pathoschild.Stardew.TractorMod.Framework.Menu
{
    class ConfigMenu : IClickableMenu
    {

        public List<ClickableComponent> optionSlots = new List<ClickableComponent>();
        public int currentItemIndex;

        private List<OptionsElement> options = new List<OptionsElement>();
        private int optionsSlotHeld = -1;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent scrollBar;
        private bool scrolling;
        private Rectangle scrollBarRunner;
        private readonly ModConfig Config;
        private readonly Dictionary<string, bool> CheckStatus = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> IntStatus = new Dictionary<string, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X start location</param>
        /// <param name="y">Y start location</param>
        /// <param name="width">Width of the Menu</param>
        /// <param name="height">Height of the Menu</param>
        /// <param name="config">Modconfig config</param>
        public ConfigMenu(int x, int y, int width, int height, ModConfig config)
          : base(x, y, width, height, true)
        {
            this.Config = config;
            this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), (float)Game1.pixelZoom, false);
            this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), (float)Game1.pixelZoom, false);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + Game1.pixelZoom * 3, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), (float)Game1.pixelZoom, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, this.scrollBar.bounds.Width, height - Game1.tileSize * 2 - this.upArrow.bounds.Height - Game1.pixelZoom * 2);
            for (int index = 0; index < 7; ++index)
            {
                List<ClickableComponent> optionSlots = this.optionSlots;
                ClickableComponent clickableComponent = new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom + index * ((height - Game1.tileSize * 2) / 7), width - Game1.tileSize / 2, (height - Game1.tileSize * 2) / 7 + Game1.pixelZoom), string.Concat((object)index));
                clickableComponent.myID = index;
                int num1 = index < 6 ? index + 1 : -7777;
                clickableComponent.downNeighborID = num1;
                int num2 = index > 0 ? index - 1 : -7777;
                clickableComponent.upNeighborID = num2;
                int num3 = 1;
                clickableComponent.fullyImmutable = num3 != 0;
                optionSlots.Add(clickableComponent);
            }
            this.options.Add(new OptionsElement("Tractor Mod Config"));
            this.options.Add(new OptionsElement("Basic Config"));
            //basic mod configs
            foreach (var property in this.Config.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(bool))
                {
                    this.options.Add(new NamedOptionsCheckbox(property.Name, 0, string.Concat(property.Name)) { isChecked = (bool)property.GetValue(this.Config, null) });
                }
                else if (property.PropertyType == typeof(int))
                {
                    Regex r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");
                    string label = r.Replace(property.Name, " ");
                    int initialValue = (int)property.GetValue(this.Config, null);
                    //Don't do anything with the price
                    if (property.Name.Contains("Price"))
                        continue;
                    else if (property.Name.Contains("Distance"))
                    {
                        List<string> valueOptions = new List<string>
                        {
                            "1",
                            "4",
                            "8",
                            "16"
                        };
                        List<string> displayOptions = new List<string>
                        {
                            "Low",
                            "Medium",
                            "High",
                            "Xtreme"
                        };
                        CustomPlusMinus customPlusMinus1 = new CustomPlusMinus(property.Name, label, 0, valueOptions, displayOptions, initialValue);
                        this.options.Add(customPlusMinus1);
                    }
                    else if (property.Name.Contains("TractorSpeed"))
                    {
                        List<string> valueOptions = new List<string>
                        {
                            "-2",
                            "4",
                            "8",
                            "16"
                        };
                        List<string> displayOptions = new List<string>
                        {
                            "Low",
                            "Medium",
                            "High",
                            "Xtreme"
                        };
                        CustomPlusMinus customPlusMinus1 = new CustomPlusMinus(property.Name, label, 0, valueOptions, displayOptions, initialValue);
                        this.options.Add(customPlusMinus1);
                    }
                    else if (property.Name.Contains("Distance"))
                    {
                        List<string> valueOptions = new List<string>
                        {
                            "1",
                            "4",
                            "8",
                            "16"
                        };
                        List<string> displayOptions = new List<string>
                        {
                            "Low",
                            "Medium",
                            "High",
                            "Xtreme"
                        };
                        CustomPlusMinus customPlusMinus1 = new CustomPlusMinus(property.Name, label, 0, valueOptions, displayOptions, initialValue);
                        this.options.Add(customPlusMinus1);
                    }
                    else if (property.Name.Contains("MagneticRadius"))
                    {
                        List<string> valueOptions = new List<string>
                        {
                            "384",
                            "484",
                            "684",
                            "1000"
                        };
                        List<string> displayOptions = new List<string>
                        {
                            "Low",
                            "Medium",
                            "High",
                            "Xtreme"
                        };
                        CustomPlusMinus customPlusMinus1 = new CustomPlusMinus(property.Name, label, 0, valueOptions, displayOptions, initialValue);
                        this.options.Add(customPlusMinus1);
                    }
                }
            }
            //Attachment Configs
            foreach (var field in this.Config.StandardAttachments.GetType().GetFields())
            {
                this.options.Add(new OptionsElement(field.Name));
                object subValue = field.GetValue(this.Config.StandardAttachments);
                foreach (var property in field.FieldType.GetProperties())
                {
                    this.options.Add(new NamedOptionsCheckbox(property.Name, 0, string.Concat(field.Name, ".", property.Name)) { isChecked = (bool)property.GetValue(subValue) });
                }
            }

            this.SetConfigStatus();
        }

        /// <summary>
        /// initializes the two status lists with the data from the config
        /// </summary>
        private void SetConfigStatus()
        {
            foreach (var field in this.Config.StandardAttachments.GetType().GetFields())
            {
                object subValue = field.GetValue(this.Config.StandardAttachments);
                foreach (var property in field.FieldType.GetProperties())
                {
                    this.CheckStatus.Add(string.Concat(field.Name, ".", property.Name), (bool)property.GetValue(subValue, null));
                }
            }
            foreach (var property in this.Config.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(bool))
                {
                    this.CheckStatus.Add(property.Name, (bool)property.GetValue(this.Config, null));
                }
                if (property.PropertyType == typeof(int))
                {
                    this.IntStatus.Add(property.Name, (int)property.GetValue(this.Config, null));
                }
            }
        }
        

        /// <summary>
        /// Moives the scroll bar to selected position
        /// </summary>
        private void SetScrollBarToCurrentIndex()
        {
            if (this.options.Count <= 0)
                return;
            this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.options.Count - 7 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + Game1.pixelZoom;
            if (this.currentItemIndex != this.options.Count - 7)
                return;
            this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - Game1.pixelZoom;
        }


        /// <summary>
        /// when the left mouse button is held down, used for scroll bar
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.leftClickHeld(x, y);
            if (this.scrolling)
            {
                int y1 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - Game1.tileSize - Game1.pixelZoom * 3 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + Game1.pixelZoom * 5));
                this.currentItemIndex = Math.Min(this.options.Count - 7, Math.Max(0, (int)((double)this.options.Count * (double)((float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height))));
                this.SetScrollBarToCurrentIndex();
                int y2 = this.scrollBar.bounds.Y;
                if (y1 == y2)
                    return;
                Game1.playSound("shiny4");
            }
            else
            {
                if (this.optionsSlotHeld == -1 || this.optionsSlotHeld + this.currentItemIndex >= this.options.Count)
                    return;
                this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickHeld(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
            }
        }

        /// <summary>
        /// When a key is pressed
        /// </summary>
        /// <param name="key">What Key was pressed (Auto passed by Stardew)</param>
        public override void receiveKeyPress(Keys key)
        {
            if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count || Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                if (this.currentlySnappedComponent != null && Game1.options.snappyMenus && (Game1.options.gamepadControls && this.options.Count > this.currentItemIndex + this.currentlySnappedComponent.myID) && this.currentItemIndex + this.currentlySnappedComponent.myID >= 0)
                    this.options[this.currentItemIndex + this.currentlySnappedComponent.myID].receiveKeyPress(key);
                else if (this.options.Count > this.currentItemIndex + this.optionsSlotHeld && this.currentItemIndex + this.optionsSlotHeld >= 0)
                    this.options[this.currentItemIndex + this.optionsSlotHeld].receiveKeyPress(key);
            }
            base.receiveKeyPress(key);
        }

        /// <summary>
        /// When the Scrollwheel is spun
        /// </summary>
        /// <param name="direction">What direction the mouse wheel was spun (Auto passed by Stardew)</param>
        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.currentItemIndex >= Math.Max(0, this.options.Count - 7))
                    return;
                this.DownArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        /// <summary>
        /// When the left mouse button is released
        /// </summary>
        /// <param name="x">X Coordinate of the mouse (Auto Passed by Stardew)</param>
        /// <param name="y">Y Coordinate of the mouse (Auto Passed by Stardew)</param>
        public override void releaseLeftClick(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.releaseLeftClick(x, y);
            if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
                this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickReleased(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
            this.optionsSlotHeld = -1;
            this.scrolling = false;
        }

        /// <summary>
        /// If the Scroll Down arrow is pressed
        /// </summary>
        private void DownArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            this.currentItemIndex = this.currentItemIndex + 1;
            this.SetScrollBarToCurrentIndex();
        }

        /// <summary>
        /// If the scroll up arrow is pressed
        /// </summary>
        private void UpArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            this.currentItemIndex -= 1;
            this.SetScrollBarToCurrentIndex();
        }

        /// <summary>
        /// Handles the Left click input
        /// </summary>
        /// <param name="x">X Coordinate of the mouse (Auto Passed by Stardew)</param>
        /// <param name="y">Y Coordinate of the mouse (Auto Passed by Stardew)</param>
        /// <param name="playSound">Determines if the game will play a sound on click</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (GameMenu.forcePreventClose)
                return;
            if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
            {
                this.DownArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.scrollBar.containsPoint(x, y))
                this.scrolling = true;
            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + Game1.tileSize * 2 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            this.currentItemIndex = Math.Max(0, Math.Min(this.options.Count - 7, this.currentItemIndex));
            for (int index = 0; index < this.optionSlots.Count; ++index)
            {
                if (this.optionSlots[index].bounds.Contains(x, y) && this.currentItemIndex + index < this.options.Count && this.options[this.currentItemIndex + index].bounds.Contains(x - this.optionSlots[index].bounds.X, y - this.optionSlots[index].bounds.Y))
                {

                    //handle the CustomPlusMinus inputs
                    OptionsElement currentElement = this.options[this.currentItemIndex + index] as CustomPlusMinus;
                    if (currentElement != null)
                    {
                        currentElement.receiveLeftClick(x - this.optionSlots[index].bounds.X, y - this.optionSlots[index].bounds.Y);
                        this.optionsSlotHeld = index;
                        this.IntStatus[((CustomPlusMinus)currentElement).Name] = ((CustomPlusMinus)currentElement).GetCurrentOption();
                    }
                    else {
                        //Handle the NamedOptionsCheckbox inputs
                        currentElement = this.options[this.currentItemIndex + index] as NamedOptionsCheckbox;
                        currentElement.receiveLeftClick(x - this.optionSlots[index].bounds.X, y - this.optionSlots[index].bounds.Y);
                        this.optionsSlotHeld = index;
                        bool checkedItem = this.CheckStatus[((NamedOptionsCheckbox)this.options[this.currentItemIndex + index]).Name];
                        checkedItem = !checkedItem;
                        this.CheckStatus[((NamedOptionsCheckbox)this.options[this.currentItemIndex + index]).Name] = checkedItem;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the Config to the changed values
        /// </summary>
        private void SetConfig()
        {
            foreach (var field in this.Config.StandardAttachments.GetType().GetFields())
            {
                object subValue = field.GetValue(this.Config.StandardAttachments);
                foreach (var property in field.FieldType.GetProperties())
                {

                    string comparName = string.Concat(field.Name, ".", property.Name);

                    property.SetValue(subValue, this.CheckStatus[comparName], null);

                }
            }
            foreach (var property in this.Config.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(this.Config, this.CheckStatus[property.Name], null);
                }
                if (property.PropertyType == typeof(int))
                {
                    property.SetValue(this.Config, this.IntStatus[property.Name], null);
                }
            }
        }
        /// <summary>
        /// Runs SetConfig when the menu closes
        /// </summary>
        protected override void cleanupBeforeExit()
        {
            this.SetConfig();
        }

        /// <summary>
        /// Draws all components of the menu
        /// </summary>
        /// <param name="b"></param>
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string)null, false);
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            for (int index = 0; index < this.optionSlots.Count; ++index)
            {
                if (this.currentItemIndex >= 0 && this.currentItemIndex + index < this.options.Count)
                    this.options[this.currentItemIndex + index].draw(b, this.optionSlots[index].bounds.X, this.optionSlots[index].bounds.Y);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (!GameMenu.forcePreventClose)
            {
                this.upArrow.draw(b);
                this.downArrow.draw(b);
                if (this.options.Count > 7)
                {
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, (float)Game1.pixelZoom, false);
                    this.scrollBar.draw(b);
                }
            }

            this.drawMouse(Game1.spriteBatch);

        }
    }
}

