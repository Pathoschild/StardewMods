using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework;

/// <summary>One entry of a field that a screen reader user can step through (like one recipe or one condition).</summary>
/// <param name="Text">The spoken text for the entry.</param>
/// <param name="Context">A heading that applies to the entry (like the recipe type or fish name), spoken after the text when it changes.</param>
/// <param name="Children">Nested entries, if this entry can be expanded.</param>
internal readonly record struct SpokenEntry(string Text, string? Context = null, IReadOnlyList<SpokenEntry>? Children = null);

/// <summary>Builds the text a screen reader speaks for the lookup menu, so layout (line breaks, bullets, icons, colours) is turned into plain sentences.</summary>
internal static class SpokenText
{
    /*********
    ** Fields
    *********/
    /// <summary>Matches a line break in any of the forms the game and mods produce.</summary>
    private static readonly Regex LineBreak = new(@"\r\n|\r|\n", RegexOptions.Compiled);

    /// <summary>The characters which end a sentence, so no extra full stop is added after them.</summary>
    private static readonly char[] SentenceEnds = ['.', '!', '?', ':', ';', ','];

    /// <summary>The characters which mark a bullet at the start of a line.</summary>
    private static readonly char[] BulletChars = ['-', '*', '•'];


    /*********
    ** Public methods
    *********/
    /// <summary>Get the spoken text for formatted text pieces. Pieces are joined as drawn (so spacing between them is kept), line breaks become sentence breaks, and pieces which are only drawn as layout (transparent indents, marker stars) are skipped.</summary>
    /// <param name="pieces">The formatted text pieces.</param>
    public static string? FromFormatted(IEnumerable<IFormattedText>? pieces)
    {
        if (pieces == null)
            return null;

        var raw = new System.Text.StringBuilder();
        foreach (IFormattedText piece in pieces)
        {
            string? text = piece.Text;
            if (string.IsNullOrEmpty(text))
                continue;

            // layout-only pieces: invisible indents and decorative marker stars
            if (piece.Color == Color.Transparent)
                continue;
            if (text.Trim().Length > 0 && text.Trim().All(ch => ch == '*'))
                continue;

            raw.Append(text);
        }

        return SpokenText.Clean(raw.ToString());
    }

    /// <summary>Turn drawn text into speakable text: each line becomes a sentence (so line breaks become a pause), bullets are dropped, and extra spaces are removed.</summary>
    /// <param name="text">The text to clean.</param>
    public static string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        string[] lines = SpokenText.LineBreak
            .Split(text)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
        if (lines.Length == 0)
            return null;

        // a list drawn with a bullet on every line: drop the bullets (a single leading dash could be a minus sign, so it's kept)
        if (lines.Length > 1 && lines.All(line => line.Length > 1 && SpokenText.BulletChars.Contains(line[0])))
            lines = lines.Select(line => line.TrimStart(SpokenText.BulletChars).TrimStart()).Where(line => line.Length > 0).ToArray();

        return SpokenText.Sentences(lines);
    }

    /// <summary>Join text parts as separate sentences, so the screen reader pauses between them.</summary>
    /// <param name="parts">The parts to join.</param>
    public static string Sentences(IEnumerable<string?> parts)
    {
        var result = new System.Text.StringBuilder();
        foreach (string? rawPart in parts)
        {
            string part = (rawPart ?? "").Trim();
            if (part.Length == 0)
                continue;

            if (result.Length > 0)
            {
                char last = result[^1];
                result.Append(SpokenText.SentenceEnds.Contains(last) ? " " : ". ");
            }
            result.Append(part);
        }

        return result.ToString();
    }

    /// <summary>Join values as a spoken list like "A, B and C".</summary>
    /// <param name="values">The values to join.</param>
    public static string AndList(IEnumerable<string> values)
    {
        string[] items = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        return items.Length switch
        {
            0 => "",
            1 => items[0],
            _ => $"{string.Join(I18n.Generic_ListSeparator(), items[..^1])} {I18n.ScreenReader_And()} {items[^1]}"
        };
    }

    /// <summary>Get a condition with whether it is met right now, like "Summer, yes right now".</summary>
    /// <param name="text">The condition text.</param>
    /// <param name="isMet">Whether the condition is met.</param>
    public static string Condition(string text, bool isMet)
    {
        return isMet
            ? I18n.ScreenReader_ConditionMet(text)
            : I18n.ScreenReader_ConditionUnmet(text);
    }

    /// <summary>Get a task with whether it is done, like "Big Barn, done".</summary>
    /// <param name="text">The task text.</param>
    /// <param name="isDone">Whether the task is done.</param>
    public static string Progress(string text, bool isDone)
    {
        return isDone
            ? I18n.ScreenReader_Done(text)
            : I18n.ScreenReader_NotDone(text);
    }

    /// <summary>Get an item name with its count (like "2 Coal" or "2 to 4 Coal").</summary>
    /// <param name="name">The item name.</param>
    /// <param name="minCount">The minimum count.</param>
    /// <param name="maxCount">The maximum count.</param>
    public static string ItemWithCount(string name, int minCount, int maxCount)
    {
        if (minCount != maxCount)
            return I18n.ScreenReader_CountRangeOf(min: minCount, max: maxCount, name: name);
        if (minCount > 1)
            return I18n.ScreenReader_CountOf(count: minCount, name: name);
        return name;
    }

    /// <summary>Get an item name with its quality (like "Gold quality Parsnip"), if it has a quality above normal.</summary>
    /// <param name="name">The item name.</param>
    /// <param name="quality">The item quality.</param>
    public static string ItemWithQuality(string name, int? quality)
    {
        if (quality is > 0 && Enum.IsDefined(typeof(ItemQuality), quality.Value))
            return I18n.ScreenReader_QualityItem(quality: I18n.For((ItemQuality)quality.Value), name: name);
        return name;
    }

    /// <summary>Get the text for a drop-like entry: the item with its count, chance and conditions.</summary>
    /// <param name="itemName">The item name.</param>
    /// <param name="probability">The drop probability between 0 and 1.</param>
    /// <param name="minDrop">The minimum number dropped.</param>
    /// <param name="maxDrop">The maximum number dropped.</param>
    /// <param name="conditions">The human-readable conditions, if any.</param>
    /// <param name="alwaysShowChance">Whether to speak the chance even if it is 100%.</param>
    public static string Drop(string itemName, float probability, int minDrop, int maxDrop, string? conditions, bool alwaysShowChance = false)
    {
        string text = SpokenText.ItemWithCount(itemName, minDrop, maxDrop);
        if (probability > 0f && (probability <= .99f || alwaysShowChance))
            text = I18n.Generic_PercentChanceOf(percent: CommonHelper.GetFormattedPercentageNumber(probability), label: text);
        if (!string.IsNullOrWhiteSpace(conditions))
            text += $", {I18n.ScreenReader_Conditions(conditions)}";
        return text;
    }
}
