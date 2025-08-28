using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.DataLayers.Framework.Components;

/// <summary>The title and legend for a data layer overlay.</summary>
internal class LegendComponent : ClickableComponent
{
    /*********
    ** Fields
    *********/
    /*****
    ** Constants
    ****/
    /// <summary>The pixel padding between the color box and its label.</summary>
    private readonly int LegendColorPadding = 5;

    /// <summary>The padding within the scroll UI.</summary>
    private readonly int ScrollPadding = 5;

    /// <summary>The padding between the border and content.</summary>
    private readonly int Padding = 5;


    /*****
    ** State
    ****/
    /// <summary>The layer name to display.</summary>
    private readonly string LayerName;

    /// <summary>The legend values to display.</summary>
    private readonly LegendEntry[] Legend;


    /*****
    ** UI
    ****/
    /// <summary>The pixel size of a color box in the legend.</summary>
    private readonly int LegendColorSize;

    /// <summary>The content width of the top-left boxes.</summary>
    private readonly int BoxContentWidth;

    /// <summary>The height of the data layer name text.</summary>
    private Point LabelTextSize;

    /// <summary>The current opacity to set for the top-left boxes when drawn, as a value between 0 (transparent) and 1 (opaque).</summary>
    private float Alpha = 1;

    /// <summary>The opacity to set for the top-left boxes when the cursor overlaps it, as a value between 0 (transparent) and 1 (opaque).</summary>
    private readonly float AlphaOnHover;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="x">The X-position from which to render the component.</param>
    /// <param name="y">The Y-position from which to render the component.</param>
    /// <param name="layers">The data layers to render.</param>
    /// <param name="layerName">The current layer name to display.</param>
    /// <param name="legend">The legend values to display.</param>
    /// <param name="alphaOnHover">The opacity to set for the top-left boxes when the cursor overlaps it, as a value between 0 (transparent) and 1 (opaque).</param>
    public LegendComponent(int x, int y, ILayer[] layers, string layerName, LegendEntry[] legend, float alphaOnHover)
        : base(new Rectangle(x, y, 0, 0), nameof(LegendComponent))
    {
        this.LayerName = layerName;
        this.Legend = legend;
        this.AlphaOnHover = alphaOnHover;

        this.LegendColorSize = (int)Game1.smallFont.MeasureString("X").Y;
        this.BoxContentWidth = this.GetMaxContentWidth(layers, this.LegendColorSize);

        this.ReinitializeComponents();
    }

    /// <summary>Render the UI.</summary>
    /// <param name="spriteBatch">The sprite batch being rendered.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        // precalculate values
        int leftOffset = this.bounds.X;
        int topOffset = this.bounds.Y;
        float alpha = this.Alpha;
        Color textColor = alpha < 1
            ? Game1.textColor * alpha
            : Game1.textColor;

        // draw overlay label
        {
            CommonHelper.DrawScroll(spriteBatch, new Vector2(leftOffset, topOffset), new Vector2(this.BoxContentWidth, this.LabelTextSize.Y), out Vector2 contentPos, out Rectangle scrollBounds, padding: this.ScrollPadding, alpha: alpha);

            contentPos += new Vector2((this.BoxContentWidth - this.LabelTextSize.X) / 2f, 0); // center label in box
            spriteBatch.DrawString(Game1.smallFont, this.LayerName, contentPos, textColor);

            topOffset += scrollBounds.Height + this.Padding;
        }

        // draw legend
        if (this.Legend.Any())
        {
            CommonHelper.DrawScroll(spriteBatch, new Vector2(leftOffset, topOffset), new Vector2(this.BoxContentWidth, this.Legend.Length * this.LegendColorSize), out Vector2 contentPos, out Rectangle _, padding: this.ScrollPadding, alpha: alpha);
            for (int i = 0; i < this.Legend.Length; i++)
            {
                LegendEntry value = this.Legend[i];
                int legendX = (int)contentPos.X;
                int legendY = (int)(contentPos.Y + i * this.LegendColorSize);

                spriteBatch.DrawLine(legendX, legendY, new Vector2(this.LegendColorSize), value.Color * alpha);
                spriteBatch.DrawString(Game1.smallFont, value.Name, new Vector2(legendX + this.LegendColorSize + this.LegendColorPadding, legendY + 2), textColor);
            }
        }
    }

    /// <summary>Update the UI on new tick.</summary>
    public void Update()
    {
        // update opacity
        bool isHovered = this.bounds.Contains(Game1.getMousePosition(true));
        float rate = (float)(0.75f / Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds);
        this.Alpha = Math.Clamp(this.Alpha + rate * (isHovered ? -1 : 1), this.AlphaOnHover, 1f);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the maximum content width needed to render the layer labels and legends.</summary>
    /// <param name="layers">The data layers to render.</param>
    /// <param name="legendColorSize">The pixel size of a color box in the legend.</param>
    private int GetMaxContentWidth(ILayer[] layers, int legendColorSize)
    {
        float labelWidth =
            (
                from layer in layers
                select Game1.smallFont.MeasureString(layer.Name).X
            )
            .Max();
        float legendContentWidth =
            (
                from layer in layers
                from entry in layer.Legend
                select Game1.smallFont.MeasureString(entry.Name).X
            )
            .Max() + legendColorSize + this.LegendColorPadding;

        return (int)Math.Max(labelWidth, legendContentWidth);
    }

    /// <summary>Reinitialize the UI components.</summary>
    private void ReinitializeComponents()
    {
        // get label size
        {
            var labelSize = Game1.smallFont.MeasureString(this.LayerName);
            this.LabelTextSize = new Point((int)labelSize.X, (int)labelSize.Y);
        }

        // get scroll dimensions
        CommonHelper.GetScrollDimensions(contentSize: new Vector2(this.BoxContentWidth, this.LabelTextSize.Y), this.ScrollPadding, innerWidth: out _, innerHeight: out _, labelOuterWidth: out int labelOuterWidth, outerHeight: out int labelOuterHeight, borderWidth: out _, borderHeight: out _);
        CommonHelper.GetScrollDimensions(contentSize: new Vector2(this.BoxContentWidth, this.Legend.Length * this.LegendColorSize), this.ScrollPadding, innerWidth: out _, innerHeight: out _, labelOuterWidth: out int legendOuterWidth, outerHeight: out int legendOuterHeight, borderWidth: out _, borderHeight: out _);

        // calculate bounds
        this.bounds.Width = Math.Max(labelOuterWidth, legendOuterWidth);
        this.bounds.Height = labelOuterHeight + this.Padding + legendOuterHeight;
    }
}
