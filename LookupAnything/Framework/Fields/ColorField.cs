using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows a color preview icon.</summary>
internal class ColorField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The color to represent.</summary>
    private readonly Color Color;

    /// <summary>The dye strength.</summary>
    private readonly int Strength;

    /// <summary>Whether the field should display a shifting prismatic color scheme.</summary>
    private readonly bool IsPrismatic;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="item">The dye-producing item.</param>
    public ColorField(string label, Item item)
        : base(label)
    {
        // get color
        if (item.Name == "Prismatic Shard")
        {
            this.IsPrismatic = true;
            this.HasValue = true;
        }
        else
        {
            Color? color = TailoringMenu.GetDyeColor(item);
            if (color.HasValue)
            {
                this.Color = color.Value;
                this.HasValue = true;
            }
        }

        // get dye strength
        if (this.HasValue)
        {
            if (item.HasContextTag("dye_strong"))
                this.Strength = 3;
            else if (item.HasContextTag("dye_medium"))
                this.Strength = 2;
            else
                this.Strength = 1;
        }
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        // the sighted view shows one to three swatches of the dye colour (more swatches = stronger dye)
        if (this.IsPrismatic)
            return I18n.ScreenReader_DyePrismatic();

        string colorName = ColorField.GetColorName(this.Color);
        string? strength = this.Strength switch
        {
            1 => I18n.ScreenReader_DyeStrengthWeak(),
            2 => I18n.ScreenReader_DyeStrengthMedium(),
            3 => I18n.ScreenReader_DyeStrengthStrong(),
            _ => null
        };

        return strength != null
            ? I18n.ScreenReader_DyeStrength(color: colorName, strength: strength)
            : I18n.ScreenReader_DyeColor(colorName);
    }

    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth, float visibleHeight)
    {
        // get icon size
        float textHeight = font.MeasureString("ABC").Y;
        Vector2 iconSize = new Vector2(textHeight);

        // draw preview icon & text
        float offset = 0;
        Color color = this.IsPrismatic ? Utility.GetPrismaticColor() : this.Color;
        for (int i = 0; i < this.Strength; i++)
        {
            spriteBatch.DrawSpriteWithin(CommonHelper.Pixel, new Rectangle(0, 0, 1, 1), position.X + offset, position.Y, iconSize, color);
            offset += iconSize.X + 2;
        }

        // return size
        return new Vector2(wrapWidth, iconSize.Y + 5);
    }

    /*********
    ** Private methods
    *********/
    /// <summary>Get an approximate spoken name for a colour.</summary>
    /// <param name="color">The colour to name.</param>
    private static string GetColorName(Color color)
    {
        return I18n.GetByKey($"screen-reader.color.{ColorField.GetColorKey(color)}");
    }

    /// <summary>Get the translation key suffix for the approximate name of a colour.</summary>
    /// <param name="color">The colour to name.</param>
    private static string GetColorKey(Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));
        int delta = max - min;

        if (max < 35)
            return "black";

        if (delta < 18)
        {
            if (max > 215)
                return "white";
            if (max > 150)
                return "light-gray";
            if (max > 90)
                return "gray";
            return "dark-gray";
        }

        float red = color.R / 255f;
        float green = color.G / 255f;
        float blue = color.B / 255f;
        float maxChannel = Math.Max(red, Math.Max(green, blue));
        float minChannel = Math.Min(red, Math.Min(green, blue));
        float channelDelta = maxChannel - minChannel;
        float brightness = maxChannel;
        float saturation = maxChannel <= 0f ? 0f : channelDelta / maxChannel;
        float hue = 0f;

        if (channelDelta > 0f)
        {
            if (Math.Abs(maxChannel - red) < 0.0001f)
                hue = 60f * (((green - blue) / channelDelta) % 6f);
            else if (Math.Abs(maxChannel - green) < 0.0001f)
                hue = 60f * (((blue - red) / channelDelta) + 2f);
            else
                hue = 60f * (((red - green) / channelDelta) + 4f);

            if (hue < 0f)
                hue += 360f;
        }

        if (saturation < 0.22f)
            return brightness > 0.75f ? "pale-gray" : "gray";

        return hue switch
        {
            < 15 or >= 345 => brightness < 0.35f ? "dark-red" : "red",
            < 40 => brightness < 0.4f ? "brown" : "orange",
            < 70 => "yellow",
            < 100 => "lime",
            < 150 => "green",
            < 190 => "cyan",
            < 235 => "blue",
            < 280 => "purple",
            < 320 => "pink",
            _ => "red"
        };
    }
}
