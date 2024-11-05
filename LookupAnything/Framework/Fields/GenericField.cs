using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A generic metadata field shown as an extended property in the lookup UI.</summary>
internal class GenericField : ICustomField
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Label { get; protected set; }

    /// <inheritdoc />
    public LinkField? ExpandLink { get; protected set; }

    /// <inheritdoc />
    public IFormattedText[]? Value { get; protected set; }

    /// <inheritdoc />
    public bool HasValue { get; protected set; }

    /// <inheritdoc />
    public IList<LinkTextArea>? LinkTextAreas { get; protected set; }

    /// <summary>The <see cref="ISubjectRegistry.GetSubjectByEntity"/> method, for use in populating <see cref="this.LinkTextAreas"/>.</summary>
    protected Func<object, GameLocation?, ISubject?>? GetSubjectByEntity { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericField(string label, string? value, bool? hasValue = null)
    {
        this.Label = label;
        this.Value = this.FormatValue(value);
        this.HasValue = hasValue ?? this.Value?.Any() == true;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericField(string label, IFormattedText value, bool? hasValue = null)
        : this(label, new[] { value }, hasValue) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericField(string label, IEnumerable<IFormattedText> value, bool? hasValue = null)
    {
        this.Label = label;
        this.Value = value.ToArray();
        this.HasValue = hasValue ?? this.Value?.Any() == true;
        this.LinkTextAreas = null;
    }

    /// <summary>Draw the value (or return <c>null</c> to render the <see cref="Value"/> using the default format).</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="font">The recommended font.</param>
    /// <param name="position">The position at which to draw.</param>
    /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
    /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="Value"/> using the default format.</returns>
    public virtual Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        return null;
    }

    /// <summary>Collapse the field content into an expandable link if it contains at least the given number of results.</summary>
    /// <param name="minResultsForCollapse">The minimum results needed before the field is collapsed.</param>
    /// <param name="countForLabel">The total number of results represented by the content (including grouped entries like "11 unrevealed items").</param>
    public virtual void CollapseIfLengthExceeds(int minResultsForCollapse, int countForLabel)
    {
        if (this.Value?.Length >= minResultsForCollapse)
        {
            this.CollapseByDefault(I18n.Generic_ShowXResults(count: countForLabel));
        }
    }

    /// <summary>Collapse the field by default, so the user needs to click a link to expand it.</summary>
    /// <param name="linkText">The link text to show.</param>
    public void CollapseByDefault(string linkText)
    {
        this.ExpandLink = new LinkField(this.Label, linkText, () =>
        {
            this.ExpandLink = null;
            return null;
        });
    }


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="hasValue">Whether the field should be displayed.</param>
    protected GenericField(string label, bool hasValue = false)
        : this(label, null as string, hasValue) { }

    /// <summary>Wrap text into a list of formatted snippets.</summary>
    /// <param name="value">The text to wrap.</param>
    protected IFormattedText[] FormatValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? [new FormattedText(value)]
            : [];
    }

    /// <summary>Get the display value for sale price data.</summary>
    /// <param name="saleValue">The flat sale price.</param>
    /// <param name="stackSize">The number of items in the stack.</param>
    public static string? GetSaleValueString(int saleValue, int stackSize)
    {
        return GenericField.GetSaleValueString(new Dictionary<ItemQuality, int> { [ItemQuality.Normal] = saleValue }, stackSize);
    }

    /// <summary>Get the display value for sale price data.</summary>
    /// <param name="saleValues">The sale price data.</param>
    /// <param name="stackSize">The number of items in the stack.</param>
    public static string? GetSaleValueString(IDictionary<ItemQuality, int>? saleValues, int stackSize)
    {
        // can't be sold
        if (saleValues == null || !saleValues.Any() || saleValues.Values.All(p => p == 0))
            return null;

        // one quality
        if (saleValues.Count == 1)
        {
            string result = I18n.Generic_Price(price: saleValues.First().Value);
            if (stackSize > 1 && stackSize <= Constant.MaxStackSizeForPricing)
                result += $" ({I18n.Generic_PriceForStack(price: saleValues.First().Value * stackSize, count: stackSize)})";
            return result;
        }

        // prices by quality
        List<string> priceStrings = [];
        for (ItemQuality quality = ItemQuality.Normal; ; quality = quality.GetNext())
        {
            if (saleValues.ContainsKey(quality))
            {
                priceStrings.Add(quality == ItemQuality.Normal
                    ? I18n.Generic_Price(price: saleValues[quality])
                    : I18n.Generic_PriceForQuality(price: saleValues[quality], quality: I18n.For(quality))
                );
            }

            if (quality.GetNext() == quality)
                break;
        }
        return I18n.List(priceStrings);
    }

    /// <summary>
    /// Check if item should be added to link text areas, if added/updated, increment the index.
    /// Make assumption that the linkable items in the field will not change over lifetime of menu, and that each item
    /// will be processed by <see cref="DrawValue"/> in the same order on every draw cycle.
    /// </summary>
    /// <param name="entity">Entity to try to get subject and link to</param>
    /// <param name="idx">Index of the link in <see cref="this.LinkTextAreas"/></param>
    /// <returns></returns>
    protected virtual bool TryGetOrAddLinkTextArea(object? entity, ref int idx, [NotNullWhen(true)] out LinkTextArea? linkTextArea)
    {
        linkTextArea = null;
        if (this.GetSubjectByEntity == null || this.LinkTextAreas == null || entity == null)
            return false;
        if (this.GetSubjectByEntity(entity, null) is not ISubject subject)
            return false;
        if (this.LinkTextAreas.Count == idx)
            this.LinkTextAreas.Add(new(subject));
        else if (this.LinkTextAreas.Count < idx) // misalignment in index and LinkTextAreas, abort
            return false;
        linkTextArea = this.LinkTextAreas[idx];
        idx++;
        return true;
    }
}
