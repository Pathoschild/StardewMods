using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Items.ItemData;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides common utility methods for interacting with the game code shared by my various mods.</summary>
    internal static class CommonHelper
    {
        /*********
        ** Fields
        *********/
        /// <summary>A blank pixel which can be colorized and stretched to draw geometric shapes.</summary>
        private static readonly Lazy<Texture2D> LazyPixel = new Lazy<Texture2D>(() =>
        {
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        });

        /// <summary>The width of the borders drawn by <see cref="DrawTab"/>.</summary>
        public const int ButtonBorderWidth = 4 * Game1.pixelZoom;


        /*********
        ** Accessors
        *********/
        /// <summary>A blank pixel which can be colorized and stretched to draw geometric shapes.</summary>
        public static Texture2D Pixel => CommonHelper.LazyPixel.Value;

        /// <summary>The width of the horizontal and vertical scroll edges (between the origin position and start of content padding).</summary>
        public static readonly Vector2 ScrollEdgeSize = new Vector2(CommonSprites.Scroll.TopLeft.Width * Game1.pixelZoom, CommonSprites.Scroll.TopLeft.Height * Game1.pixelZoom);


        /*********
        ** Public methods
        *********/
        /****
        ** Enums
        ****/
        /// <summary>Get the values in an enum.</summary>
        /// <typeparam name="TValue">The enum value type.</typeparam>
        public static IEnumerable<TValue> GetEnumValues<TValue>() where TValue : struct
        {
            return Enum.GetValues(typeof(TValue)).Cast<TValue>();
        }

        /****
        ** Game
        ****/
        /// <summary>Get all game locations.</summary>
        /// <param name="includeTempLevels">Whether to include temporary mine/dungeon locations.</param>
        public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
        {
            var locations = Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );

            if (includeTempLevels)
                locations = locations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels);

            return locations;
        }

        /// <summary>Get a player's current tile position.</summary>
        /// <param name="player">The player to check.</param>
        public static Vector2 GetPlayerTile(Farmer player)
        {
            Vector2 position = player?.Position ?? Vector2.Zero;
            return new Vector2((int)(position.X / Game1.tileSize), (int)(position.Y / Game1.tileSize)); // note: player.getTileLocationPoint() isn't reliable in many cases, e.g. right after a warp when riding a horse
        }

        /// <summary>Get the item type for an item to disambiguate IDs.</summary>
        /// <param name="item">The item to check.</param>
        public static ItemType GetItemType(this Item item)
        {
            switch (item)
            {
                case Boots _:
                    return ItemType.Boots;

                case Clothing _:
                    return ItemType.Clothing;

                case Furniture _:
                    return ItemType.Furniture;

                case Hat _:
                    return ItemType.Hat;

                case MeleeWeapon _:
                case Slingshot _:
                    return ItemType.Weapon;

                case Ring _:
                    return ItemType.Ring;

                case Tool _:
                    return ItemType.Tool;

                case Wallpaper wallpaper:
                    return wallpaper.isFloor.Value
                        ? ItemType.Flooring
                        : ItemType.Wallpaper;

                case SObject obj:
                    return obj.bigCraftable.Value
                        ? ItemType.BigCraftable
                        : ItemType.Object;

                default:
                    return ItemType.Unknown;
            }
        }

        /****
        ** Fonts
        ****/
        /// <summary>Get the dimensions of a space character.</summary>
        /// <param name="font">The font to measure.</param>
        public static float GetSpaceWidth(SpriteFont font)
        {
            return font.MeasureString("A B").X - font.MeasureString("AB").X;
        }

        /****
        ** UI
        ****/
        /// <summary>Draw a sprite to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="sheet">The sprite sheet containing the sprite.</param>
        /// <param name="sprite">The sprite coordinates and dimensions in the sprite sheet.</param>
        /// <param name="x">The X-position at which to draw the sprite.</param>
        /// <param name="y">The X-position at which to draw the sprite.</param>
        /// <param name="width">The width to draw.</param>
        /// <param name="height">The height to draw.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public static void Draw(this SpriteBatch batch, Texture2D sheet, Rectangle sprite, int x, int y, int width, int height, Color? color = null)
        {
            batch.Draw(sheet, new Rectangle(x, y, width, height), sprite, color ?? Color.White);
        }

        /// <summary>Draw a pretty hover box for the given text.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="label">The text to display.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The maximum width to display.</param>
        public static Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, in Vector2 position, float wrapWidth)
        {
            const int paddingSize = 27;
            const int gutterSize = 20;

            Vector2 labelSize = spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(gutterSize), wrapWidth); // draw text to get wrapped text dimensions
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)position.X, (int)position.Y, (int)labelSize.X + paddingSize + gutterSize, (int)labelSize.Y + paddingSize, Color.White);
            spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(gutterSize), wrapWidth); // draw again over texture box

            return labelSize + new Vector2(paddingSize);
        }

        /// <summary>Draw a tab texture to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="x">The X position at which to draw.</param>
        /// <param name="y">The Y position at which to draw.</param>
        /// <param name="innerWidth">The width of the button's inner content.</param>
        /// <param name="innerHeight">The height of the button's inner content.</param>
        /// <param name="innerDrawPosition">The position at which the content should be drawn.</param>
        /// <param name="align">The button's horizontal alignment relative to <paramref name="x"/>. The possible values are 0 (left), 1 (center), or 2 (right).</param>
        /// <param name="alpha">The button opacity, as a value from 0 (transparent) to 1 (opaque).</param>
        /// <param name="forIcon">Whether the button will contain an icon instead of text.</param>
        /// <param name="drawShadow">Whether to draw a shadow under the tab.</param>
        public static void DrawTab(SpriteBatch spriteBatch, int x, int y, int innerWidth, int innerHeight, out Vector2 innerDrawPosition, int align = 0, float alpha = 1, bool forIcon = false, bool drawShadow = true)
        {
            // calculate outer coordinates
            int outerWidth = innerWidth + CommonHelper.ButtonBorderWidth * 2;
            int outerHeight = innerHeight + Game1.tileSize / 3;
            int offsetX = align switch
            {
                1 => -outerWidth / 2,
                2 => -outerWidth,
                _ => 0
            };

            // calculate inner coordinates
            {
                int iconOffsetX = forIcon ? -Game1.pixelZoom : 0;
                int iconOffsetY = forIcon ? 2 * -Game1.pixelZoom : 0;
                innerDrawPosition = new Vector2(x + CommonHelper.ButtonBorderWidth + offsetX + iconOffsetX, y + CommonHelper.ButtonBorderWidth + iconOffsetY);
            }

            // draw texture
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x + offsetX, y, outerWidth, outerHeight + Game1.tileSize / 16, Color.White * alpha, drawShadow: drawShadow);
        }

        /// <summary>Draw a button background.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="position">The top-left pixel coordinate at which to draw the button.</param>
        /// <param name="contentSize">The button content's pixel size.</param>
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The button's outer bounds.</param>
        /// <param name="padding">The padding between the content and border.</param>
        public static void DrawButton(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 0)
        {
            CommonHelper.DrawContentBox(
                spriteBatch: spriteBatch,
                texture: CommonSprites.Button.Sheet,
                background: CommonSprites.Button.Background,
                top: CommonSprites.Button.Top,
                right: CommonSprites.Button.Right,
                bottom: CommonSprites.Button.Bottom,
                left: CommonSprites.Button.Left,
                topLeft: CommonSprites.Button.TopLeft,
                topRight: CommonSprites.Button.TopRight,
                bottomRight: CommonSprites.Button.BottomRight,
                bottomLeft: CommonSprites.Button.BottomLeft,
                position: position,
                contentSize: contentSize,
                contentPos: out contentPos,
                bounds: out bounds,
                padding: padding
            );
        }

        /// <summary>Draw a scroll background.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="position">The top-left pixel coordinate at which to draw the scroll.</param>
        /// <param name="contentSize">The scroll content's pixel size.</param>
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The scroll's outer bounds.</param>
        /// <param name="padding">The padding between the content and border.</param>
        public static void DrawScroll(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 5)
        {
            CommonHelper.DrawContentBox(
                spriteBatch: spriteBatch,
                texture: CommonSprites.Scroll.Sheet,
                background: in CommonSprites.Scroll.Background,
                top: CommonSprites.Scroll.Top,
                right: CommonSprites.Scroll.Right,
                bottom: CommonSprites.Scroll.Bottom,
                left: CommonSprites.Scroll.Left,
                topLeft: CommonSprites.Scroll.TopLeft,
                topRight: CommonSprites.Scroll.TopRight,
                bottomRight: CommonSprites.Scroll.BottomRight,
                bottomLeft: CommonSprites.Scroll.BottomLeft,
                position: position,
                contentSize: contentSize,
                contentPos: out contentPos,
                bounds: out bounds,
                padding: padding
            );
        }

        /// <summary>Draw a generic content box like a scroll or button.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="background">The source rectangle for the background.</param>
        /// <param name="top">The source rectangle for the top border.</param>
        /// <param name="right">The source rectangle for the right border.</param>
        /// <param name="bottom">The source rectangle for the bottom border.</param>
        /// <param name="left">The source rectangle for the left border.</param>
        /// <param name="topLeft">The source rectangle for the top-left corner.</param>
        /// <param name="topRight">The source rectangle for the top-right corner.</param>
        /// <param name="bottomRight">The source rectangle for the bottom-right corner.</param>
        /// <param name="bottomLeft">The source rectangle for the bottom-left corner.</param>
        /// <param name="position">The top-left pixel coordinate at which to draw the button.</param>
        /// <param name="contentSize">The button content's pixel size.</param>
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The box's outer bounds.</param>
        /// <param name="padding">The padding between the content and border.</param>
        public static void DrawContentBox(SpriteBatch spriteBatch, Texture2D texture, in Rectangle background, in Rectangle top, in Rectangle right, in Rectangle bottom, in Rectangle left, in Rectangle topLeft, in Rectangle topRight, in Rectangle bottomRight, in Rectangle bottomLeft, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding)
        {
            CommonHelper.GetContentBoxDimensions(topLeft, contentSize, padding, out int innerWidth, out int innerHeight, out int outerWidth, out int outerHeight, out int cornerWidth, out int cornerHeight);
            int x = (int)position.X;
            int y = (int)position.Y;

            // draw scroll background
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y + cornerHeight, innerWidth, innerHeight), background, Color.White);

            // draw borders
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y, innerWidth, cornerHeight), top, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y + cornerHeight + innerHeight, innerWidth, cornerHeight), bottom, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x, y + cornerHeight, cornerWidth, innerHeight), left, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight, cornerWidth, innerHeight), right, Color.White);

            // draw corners
            spriteBatch.Draw(texture, new Rectangle(x, y, cornerWidth, cornerHeight), topLeft, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), bottomLeft, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y, cornerWidth, cornerHeight), topRight, Color.White);
            spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), bottomRight, Color.White);

            // set out params
            contentPos = new Vector2(x + cornerWidth + padding, y + cornerHeight + padding);
            bounds = new Rectangle(x, y, outerWidth, outerHeight);
        }

        /// <summary>Show an informational message to the player.</summary>
        /// <param name="message">The message to show.</param>
        /// <param name="duration">The number of milliseconds during which to keep the message on the screen before it fades (or <c>null</c> for the default time).</param>
        public static void ShowInfoMessage(string message, int? duration = null)
        {
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type) { noIcon = true, timeLeft = duration ?? HUDMessage.defaultTime });
        }

        /// <summary>Show an error message to the player.</summary>
        /// <param name="message">The message to show.</param>
        public static void ShowErrorMessage(string message)
        {
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
        }

        /// <summary>Calculate the outer dimension for a content box.</summary>
        /// <param name="contentSize">The size of the content within the box.</param>
        /// <param name="padding">The padding within the content area.</param>
        /// <param name="innerWidth">The width of the inner content area, including padding.</param>
        /// <param name="innerHeight">The height of the inner content area, including padding.</param>
        /// <param name="labelOuterWidth">The outer pixel width.</param>
        /// <param name="outerHeight">The outer pixel height.</param>
        /// <param name="borderWidth">The width of the left and right border textures.</param>
        /// <param name="borderHeight">The height of the top and bottom border textures.</param>
        public static void GetScrollDimensions(Vector2 contentSize, int padding, out int innerWidth, out int innerHeight, out int labelOuterWidth, out int outerHeight, out int borderWidth, out int borderHeight)
        {
            CommonHelper.GetContentBoxDimensions(CommonSprites.Scroll.TopLeft, contentSize, padding, out innerWidth, out innerHeight, out labelOuterWidth, out outerHeight, out borderWidth, out borderHeight);
        }

        /// <summary>Calculate the outer dimension for a content box.</summary>
        /// <param name="topLeft">The source rectangle for the top-left corner of the content box.</param>
        /// <param name="contentSize">The size of the content within the box.</param>
        /// <param name="padding">The padding within the content area.</param>
        /// <param name="innerWidth">The width of the inner content area, including padding.</param>
        /// <param name="innerHeight">The height of the inner content area, including padding.</param>
        /// <param name="outerWidth">The outer pixel width.</param>
        /// <param name="outerHeight">The outer pixel height.</param>
        /// <param name="borderWidth">The width of the left and right border textures.</param>
        /// <param name="borderHeight">The height of the top and bottom border textures.</param>
        public static void GetContentBoxDimensions(Rectangle topLeft, Vector2 contentSize, int padding, out int innerWidth, out int innerHeight, out int outerWidth, out int outerHeight, out int borderWidth, out int borderHeight)
        {
            borderWidth = topLeft.Width * Game1.pixelZoom;
            borderHeight = topLeft.Height * Game1.pixelZoom;
            innerWidth = (int)(contentSize.X + padding * 2);
            innerHeight = (int)(contentSize.Y + padding * 2);
            outerWidth = innerWidth + borderWidth * 2;
            outerHeight = innerHeight + borderHeight * 2;
        }

        /****
        ** Drawing
        ****/
        /// <summary>Draw a sprite to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="x">The X-position at which to start the line.</param>
        /// <param name="y">The X-position at which to start the line.</param>
        /// <param name="size">The line dimensions.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public static void DrawLine(this SpriteBatch batch, float x, float y, in Vector2 size, in Color? color = null)
        {
            batch.Draw(CommonHelper.Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), color ?? Color.White);
        }

        /// <summary>Draw a block of text to the screen with the specified wrap width.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The width at which to wrap the text.</param>
        /// <param name="color">The text color.</param>
        /// <param name="bold">Whether to draw bold text.</param>
        /// <param name="scale">The font scale.</param>
        /// <returns>Returns the text dimensions.</returns>
        public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string text, in Vector2 position, float wrapWidth, in Color? color = null, bool bold = false, float scale = 1)
        {
            if (text == null)
                return new Vector2(0, 0);

            // get word list
            List<string> words = new List<string>();
            foreach (string word in text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // split on newlines
                string wordPart = word;
                int newlineIndex;
                while ((newlineIndex = wordPart.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
                {
                    if (newlineIndex == 0)
                    {
                        words.Add(Environment.NewLine);
                        wordPart = wordPart.Substring(Environment.NewLine.Length);
                    }
                    else if (newlineIndex > 0)
                    {
                        words.Add(wordPart.Substring(0, newlineIndex));
                        words.Add(Environment.NewLine);
                        wordPart = wordPart.Substring(newlineIndex + Environment.NewLine.Length);
                    }
                }

                // add remaining word (after newline split)
                if (wordPart.Length > 0)
                    words.Add(wordPart);
            }

            // track draw values
            float xOffset = 0;
            float yOffset = 0;
            float lineHeight = font.MeasureString("ABC").Y * scale;
            float spaceWidth = CommonHelper.GetSpaceWidth(font) * scale;
            float blockWidth = 0;
            float blockHeight = lineHeight;
            foreach (string word in words)
            {
                // check wrap width
                float wordWidth = font.MeasureString(word).X * scale;
                if (word == Environment.NewLine || ((wordWidth + xOffset) > wrapWidth && (int)xOffset != 0))
                {
                    xOffset = 0;
                    yOffset += lineHeight;
                    blockHeight += lineHeight;
                }
                if (word == Environment.NewLine)
                    continue;

                // draw text
                Vector2 wordPosition = new Vector2(position.X + xOffset, position.Y + yOffset);
                if (bold)
                    Utility.drawBoldText(batch, word, font, wordPosition, color ?? Color.Black, scale);
                else
                    batch.DrawString(font, word, wordPosition, color ?? Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

                // update draw values
                if (xOffset + wordWidth > blockWidth)
                    blockWidth = xOffset + wordWidth;
                xOffset += wordWidth + spaceWidth;
            }

            // return text position & dimensions
            return new Vector2(blockWidth, blockHeight);
        }

        /****
        ** Error handling
        ****/
        /// <summary>Intercept errors thrown by the action.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">A callback invoked if an error is intercepted.</param>
        public static void InterceptErrors(this IMonitor monitor, string verb, Action action, Action<Exception> onError = null)
        {
            monitor.InterceptErrors(verb, null, action, onError);
        }

        /// <summary>Intercept errors thrown by the action.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="detailedVerb">A more detailed form of <see cref="verb"/> if applicable. This is displayed in the log, so it can be more technical and isn't constrained by the sprite font.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="onError">A callback invoked if an error is intercepted.</param>
        public static void InterceptErrors(this IMonitor monitor, string verb, string detailedVerb, Action action, Action<Exception> onError = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                monitor.InterceptError(ex, verb, detailedVerb);
                onError?.Invoke(ex);
            }
        }

        /// <summary>Log an error and warn the user.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up"). This is displayed on the screen, so it should be simple and avoid characters that might not be available in the sprite font.</param>
        /// <param name="detailedVerb">A more detailed form of <see cref="verb"/> if applicable. This is displayed in the log, so it can be more technical and isn't constrained by the sprite font.</param>
        public static void InterceptError(this IMonitor monitor, Exception ex, string verb, string detailedVerb = null)
        {
            detailedVerb ??= verb;
            monitor.Log($"Something went wrong {detailedVerb}:\n{ex}", LogLevel.Error);
            CommonHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The error log has the technical details.");
        }

        /****
        ** File handling
        ****/
        /// <summary>Get the MD5 hash for a file.</summary>
        /// <param name="absolutePath">The absolute file path.</param>
        public static string GetFileHash(string absolutePath)
        {
            using FileStream stream = File.OpenRead(absolutePath);
            using MD5 md5 = MD5.Create();

            byte[] hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
