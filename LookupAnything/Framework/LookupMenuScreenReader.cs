using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Pathoschild.Stardew.Common.Integrations.StardewAccess;
using Pathoschild.Stardew.LookupAnything.Components;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework;

/// <summary>Lets a screen reader user move through the fields of the lookup menu with the game's movement keys. The text of each field comes from the field itself (see <see cref="ICustomField.GetSpokenText"/>), so everything a sighted player reads from icons, colours and layout is put into words in one place.</summary>
internal static class LookupMenuScreenReader
{
    /*********
    ** Fields
    *********/
    /// <summary>The full type name of the lookup menu, as registered with Stardew Access.</summary>
    private const string LookupMenuTypeName = "Pathoschild.Stardew.LookupAnything.Components.LookupMenu";

    /// <summary>The Stardew Access integration.</summary>
    private static StardewAccessIntegration? StardewAccess;

    /// <summary>The SMAPI input helper.</summary>
    private static IInputHelper? InputHelper;

    /// <summary>The link text of fields the player expanded, so they can be collapsed again.</summary>
    private static readonly Dictionary<object, string> CollapsedLinkText = new(ReferenceEqualityComparer.Instance);

    /// <summary>The index of the selected field, or -1 for the header.</summary>
    private static int _selectedIndex = -1;

    /// <summary>The state of the field whose entries are being stepped through, if any.</summary>
    private static ExpandedFieldState? _expandedFieldState;


    /*********
    ** Public methods
    *********/
    /// <summary>Register the menu with Stardew Access.</summary>
    /// <param name="stardewAccess">The Stardew Access integration.</param>
    /// <param name="inputHelper">The SMAPI input helper.</param>
    internal static void Initialize(StardewAccessIntegration stardewAccess, IInputHelper inputHelper)
    {
        StardewAccess = stardewAccess;
        InputHelper = inputHelper;
        StardewAccess.RegisterCustomMenuAsAccessible(LookupMenuTypeName);
    }

    /// <summary>Handle the active menu changing.</summary>
    /// <param name="newMenu">The new menu.</param>
    internal static void OnMenuChanged(IClickableMenu? newMenu)
    {
        if (newMenu is not LookupMenu)
        {
            Reset();
            return;
        }

        _selectedIndex = 0;
        CollapsedLinkText.Clear();
        _expandedFieldState = null;
        SpeakCurrentField(newMenu, announceSubject: true);
    }

    /// <summary>Handle buttons being pressed while the lookup menu is open.</summary>
    /// <param name="e">The event data.</param>
    internal static void HandleButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (e is null || !e.Pressed.Any())
            return;

        if (Game1.activeClickableMenu is not LookupMenu menu)
            return;

        if (IsTextInputActive())
            return;

        bool upPressed = IsConfiguredButtonPressed(Game1.options.moveUpButton, e.Pressed);
        bool downPressed = IsConfiguredButtonPressed(Game1.options.moveDownButton, e.Pressed);
        if (upPressed || downPressed)
        {
            SuppressConfiguredButtons(Game1.options.moveUpButton, e.Pressed);
            SuppressConfiguredButtons(Game1.options.moveDownButton, e.Pressed);

            int direction = upPressed && !downPressed ? -1 : 1;
            if (_expandedFieldState != null)
                MoveExpandedSelection(menu, direction);
            else
                MoveSelection(menu, direction);
            return;
        }

        bool rightPressed = IsConfiguredButtonPressed(Game1.options.moveRightButton, e.Pressed);
        if (rightPressed)
        {
            SuppressConfiguredButtons(Game1.options.moveRightButton, e.Pressed);
            if (_expandedFieldState == null && TryEnterExpandedField(menu))
                SpeakExpandedFieldEntry(menu, previousIndex: null);
            else if (_expandedFieldState == null && TryExpandCurrentField(menu))
                SpeakCurrentField(menu, announceSubject: false);
            return;
        }

        bool leftPressed = IsConfiguredButtonPressed(Game1.options.moveLeftButton, e.Pressed);
        if (leftPressed)
        {
            SuppressConfiguredButtons(Game1.options.moveLeftButton, e.Pressed);
            if (_expandedFieldState != null && TryExitExpandedField())
                SpeakCurrentField(menu, announceSubject: false);
            else if (TryCollapseCurrentField(menu))
                SpeakCurrentField(menu, announceSubject: false);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Reset the navigation state.</summary>
    private static void Reset()
    {
        _selectedIndex = -1;
        _expandedFieldState = null;
        CollapsedLinkText.Clear();
    }

    /// <summary>Move the selection to the next or previous field.</summary>
    /// <param name="menu">The lookup menu.</param>
    /// <param name="direction">The direction to move (-1 for up, 1 for down).</param>
    private static void MoveSelection(LookupMenu menu, int direction)
    {
        ICustomField[] fields = menu.ShownFields;
        if (fields.Length == 0)
            return;

        bool hasHeader = HasNavigableHeader(menu);
        if (_selectedIndex < (hasHeader ? -1 : 0) || _selectedIndex >= fields.Length)
            _selectedIndex = 0;

        int itemCount = fields.Length + (hasHeader ? 1 : 0);
        int currentPosition = hasHeader
            ? _selectedIndex + 1
            : _selectedIndex;

        currentPosition += direction;
        if (currentPosition < 0)
            currentPosition = itemCount - 1;
        else if (currentPosition >= itemCount)
            currentPosition = 0;

        _selectedIndex = hasHeader
            ? currentPosition - 1
            : currentPosition;

        SpeakCurrentField(menu, announceSubject: false);
    }

    /// <summary>Move to the next or previous entry of the field being stepped through.</summary>
    /// <param name="menu">The lookup menu.</param>
    /// <param name="direction">The direction to move (-1 for up, 1 for down).</param>
    private static void MoveExpandedSelection(LookupMenu menu, int direction)
    {
        if (_expandedFieldState == null || _expandedFieldState.Entries.Count == 0)
            return;

        int previousIndex = _expandedFieldState.EntryIndex;
        _expandedFieldState.EntryIndex += direction;
        if (_expandedFieldState.EntryIndex < 0)
            _expandedFieldState.EntryIndex = _expandedFieldState.Entries.Count - 1;
        else if (_expandedFieldState.EntryIndex >= _expandedFieldState.Entries.Count)
            _expandedFieldState.EntryIndex = 0;

        SpeakExpandedFieldEntry(menu, previousIndex);
    }

    /// <summary>Speak the selected field (or the header).</summary>
    /// <param name="menu">The lookup menu.</param>
    /// <param name="announceSubject">Whether to speak the subject header before the field.</param>
    private static void SpeakCurrentField(IClickableMenu menu, bool announceSubject)
    {
        if (menu is not LookupMenu lookupMenu)
            return;

        ICustomField[] fields = lookupMenu.ShownFields;
        if (fields.Length == 0)
            return;

        string subjectInfo = GetSubjectInfo(lookupMenu);
        if (_selectedIndex == -1 && !string.IsNullOrWhiteSpace(subjectInfo))
        {
            StardewAccess?.SayWithMenuChecker(subjectInfo, true, $"lookup-header:{subjectInfo}");
            return;
        }

        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
            _selectedIndex = 0;

        ICustomField field = fields[_selectedIndex];
        string label = SpokenText.Clean(field.Label) ?? field.Label;
        string? value = GetFieldValueText(field);

        // for lists, the entry count comes first so the player knows how long the text is before hearing it
        int entryCount = field.GetSpokenEntries().Count;
        if (entryCount >= 2)
            label = AppendPart(label, I18n.ScreenReader_ListEntryCount(entryCount));
        string text = string.IsNullOrWhiteSpace(value) ? label : $"{label}, {value}";

        if (announceSubject && !string.IsNullOrWhiteSpace(subjectInfo))
            text = SpokenText.Sentences([subjectInfo, text]);

        StardewAccess?.SayWithMenuChecker(text, true, $"lookup-top:{_selectedIndex}:{text}");
    }

    /// <summary>Speak the selected entry of the field being stepped through.</summary>
    /// <param name="menu">The lookup menu.</param>
    /// <param name="previousIndex">The entry index before the move, if any.</param>
    private static void SpeakExpandedFieldEntry(IClickableMenu menu, int? previousIndex)
    {
        if (_expandedFieldState == null || _expandedFieldState.Entries.Count == 0)
            return;

        _selectedIndex = _expandedFieldState.FieldIndex;
        if (_expandedFieldState.EntryIndex < 0 || _expandedFieldState.EntryIndex >= _expandedFieldState.Entries.Count)
            _expandedFieldState.EntryIndex = 0;

        SpokenEntry entry = _expandedFieldState.Entries[_expandedFieldState.EntryIndex];
        string position = I18n.ScreenReader_ListPosition(current: _expandedFieldState.EntryIndex + 1, total: _expandedFieldState.Entries.Count);

        // only repeat the heading when it changes
        string? context = entry.Context;
        if (previousIndex is >= 0 && previousIndex < _expandedFieldState.Entries.Count)
        {
            string? previousContext = _expandedFieldState.Entries[previousIndex.Value].Context;
            if (string.Equals(context, previousContext, StringComparison.Ordinal))
                context = null;
        }

        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(entry.Text))
            parts.Add(entry.Text);
        if (!string.IsNullOrWhiteSpace(context))
            parts.Add(context);
        parts.Add(position);

        string text = string.Join(", ", parts);

        StardewAccess?.SayWithMenuChecker(text, true, $"lookup-entry:{_expandedFieldState.FieldIndex}:{_expandedFieldState.EntryIndex}:{text}");
    }

    /// <summary>Get the spoken text for the subject header: name, type and description.</summary>
    /// <param name="menu">The lookup menu.</param>
    private static string GetSubjectInfo(LookupMenu menu)
    {
        ISubject subject = menu.ShownSubject;

        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(subject.Name))
            parts.Add(subject.Name);
        if (!string.IsNullOrWhiteSpace(subject.Type))
            parts.Add(subject.Type);

        // the menu shows the description on one line
        string? description = SpokenText.Clean(subject.Description);
        if (!string.IsNullOrWhiteSpace(description))
            parts.Add(description);

        return string.Join(", ", parts);
    }

    /// <summary>Get whether the subject header can be selected.</summary>
    /// <param name="menu">The lookup menu.</param>
    private static bool HasNavigableHeader(LookupMenu menu)
        => !string.IsNullOrWhiteSpace(GetSubjectInfo(menu));

    /// <summary>Expand the selected field if it's collapsed.</summary>
    /// <param name="menu">The lookup menu.</param>
    private static bool TryExpandCurrentField(LookupMenu menu)
    {
        ICustomField[] fields = menu.ShownFields;
        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
            return false;

        ICustomField field = fields[_selectedIndex];
        if (field.ExpandLink == null)
            return false;

        string? linkText = SpokenText.FromFormatted(field.ExpandLink.Value);
        if (!string.IsNullOrWhiteSpace(linkText))
            CollapsedLinkText[field] = linkText;

        field.TryGetLinkAt(0, 0, out _); // clicking the expand link expands the field
        return true;
    }

    /// <summary>Collapse the selected field if the player expanded it before.</summary>
    /// <param name="menu">The lookup menu.</param>
    private static bool TryCollapseCurrentField(LookupMenu menu)
    {
        ICustomField[] fields = menu.ShownFields;
        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
            return false;

        ICustomField field = fields[_selectedIndex];
        if (field.ExpandLink != null)
            return false;

        if (!CollapsedLinkText.TryGetValue(field, out string? linkText) || string.IsNullOrWhiteSpace(linkText))
            return false;

        return TryCollapseField(field, linkText);
    }

    /// <summary>Start stepping through the entries of the selected field.</summary>
    /// <param name="menu">The lookup menu.</param>
    private static bool TryEnterExpandedField(LookupMenu menu)
    {
        ICustomField[] fields = menu.ShownFields;
        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
            return false;

        ICustomField field = fields[_selectedIndex];
        IReadOnlyList<SpokenEntry> entries = field.GetSpokenEntries();
        if (entries.Count < 2)
            return false;

        string? collapseLinkText = null;
        bool restoreCollapsedState = false;
        if (field.ExpandLink != null)
        {
            collapseLinkText = SpokenText.FromFormatted(field.ExpandLink.Value);
            restoreCollapsedState = TryExpandCurrentField(menu);
            if (!restoreCollapsedState)
                return false;
        }

        _expandedFieldState = new ExpandedFieldState(field, _selectedIndex, entries, collapseLinkText, restoreCollapsedState);
        return true;
    }

    /// <summary>Stop stepping through the entries of a field.</summary>
    private static bool TryExitExpandedField()
    {
        if (_expandedFieldState == null)
            return false;

        ExpandedFieldState state = _expandedFieldState;
        _expandedFieldState = null;
        _selectedIndex = state.FieldIndex;

        if (state.RestoreCollapsedState && !string.IsNullOrWhiteSpace(state.CollapseLinkText))
        {
            TryCollapseField(state.Field, state.CollapseLinkText);
            CollapsedLinkText.Remove(state.Field);
        }

        return true;
    }

    /// <summary>Collapse a field back into its expand link.</summary>
    /// <param name="field">The field to collapse.</param>
    /// <param name="linkText">The text of the expand link.</param>
    private static bool TryCollapseField(ICustomField field, string linkText)
    {
        if (field is not GenericField genericField)
            return false;

        genericField.CollapseByDefault(linkText);
        return true;
    }

    /// <summary>Get the spoken value of a field.</summary>
    /// <param name="field">The field to read.</param>
    private static string? GetFieldValueText(ICustomField field)
    {
        // collapsed fields show only their expand link
        if (field.ExpandLink != null)
            return SpokenText.FromFormatted(field.ExpandLink.Value);

        return field.GetSpokenText();
    }

    /// <summary>Append a part to some spoken text.</summary>
    /// <param name="text">The spoken text.</param>
    /// <param name="suffix">The part to add.</param>
    private static string AppendPart(string text, string suffix)
    {
        return string.IsNullOrWhiteSpace(suffix)
            ? text
            : $"{text}, {suffix}";
    }

    /// <summary>Get whether a configured game button was pressed.</summary>
    /// <param name="configuredButtons">The buttons configured in the game options for an action.</param>
    /// <param name="pressed">The buttons which were pressed.</param>
    private static bool IsConfiguredButtonPressed(InputButton[] configuredButtons, IEnumerable<SButton> pressed)
    {
        foreach (InputButton inputButton in configuredButtons)
        {
            SButton button = inputButton.ToSButton();
            if (pressed.Contains(button))
                return true;
        }

        return false;
    }

    /// <summary>Stop the game from also handling a configured button.</summary>
    /// <param name="configuredButtons">The buttons configured in the game options for an action.</param>
    /// <param name="pressed">The buttons which were pressed.</param>
    private static void SuppressConfiguredButtons(InputButton[] configuredButtons, IEnumerable<SButton> pressed)
    {
        foreach (InputButton inputButton in configuredButtons)
        {
            SButton button = inputButton.ToSButton();
            if (pressed.Contains(button))
                InputHelper?.Suppress(button);
        }
    }

    /// <summary>Get whether the player is typing in a text box.</summary>
    private static bool IsTextInputActive()
        => Game1.game1.HasKeyboardFocus() && Game1.game1.instanceKeyboardDispatcher?.Subscriber != null;

    /// <summary>The state of a field whose entries are being stepped through.</summary>
    private sealed class ExpandedFieldState(ICustomField field, int fieldIndex, IReadOnlyList<SpokenEntry> entries, string? collapseLinkText, bool restoreCollapsedState)
    {
        /// <summary>The field being stepped through.</summary>
        public ICustomField Field { get; } = field;

        /// <summary>The index of the field in the menu.</summary>
        public int FieldIndex { get; } = fieldIndex;

        /// <summary>The entries to step through.</summary>
        public IReadOnlyList<SpokenEntry> Entries { get; } = entries;

        /// <summary>The text of the link which expands the field, if it was collapsed.</summary>
        public string? CollapseLinkText { get; } = collapseLinkText;

        /// <summary>Whether to collapse the field again when leaving it.</summary>
        public bool RestoreCollapsedState { get; } = restoreCollapsedState;

        /// <summary>The index of the selected entry.</summary>
        public int EntryIndex { get; set; }
    }

    /// <summary>Compares objects by reference.</summary>
    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>The shared instance.</summary>
        public static ReferenceEqualityComparer Instance { get; } = new();

        /// <inheritdoc />
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        /// <inheritdoc />
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
