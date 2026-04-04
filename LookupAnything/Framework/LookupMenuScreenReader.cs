using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.StardewAccess;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal static class LookupMenuScreenReader
{
    private const string LookupMenuTypeName = "Pathoschild.Stardew.LookupAnything.Components.LookupMenu";
    private static StardewAccessIntegration? StardewAccess;
    private static IInputHelper? InputHelper;

    private static readonly Dictionary<object, string> CollapsedLinkText = new(ReferenceEqualityComparer.Instance);
    private static IClickableMenu? _lastMenu;
    private static int _selectedIndex = -1;
    private static ExpandedFieldState? _expandedFieldState;
    private static FieldInfo? _fieldsField;
    private static FieldInfo? _subjectField;
    private static PropertyInfo? _fieldLabelProperty;
    private static PropertyInfo? _fieldValueProperty;
    private static PropertyInfo? _fieldExpandLinkProperty;
    private static PropertyInfo? _formattedTextProperty;
    private static PropertyInfo? _subjectNameProperty;
    private static PropertyInfo? _subjectTypeProperty;
    private static PropertyInfo? _subjectDescriptionProperty;
    private static readonly Dictionary<string, MethodInfo?> LookupI18nMethods = new(StringComparer.Ordinal);
    private static Type? _lookupI18nType;
    private static MethodInfo? _lookupSkillProgressMethod;
    private static MethodInfo? _lookupSkillProgressLastMethod;

    internal static void Initialize(StardewAccessIntegration stardewAccess, IInputHelper inputHelper)
    {
        StardewAccess = stardewAccess;
        InputHelper = inputHelper;
        StardewAccess.RegisterCustomMenuAsAccessible(LookupMenuTypeName);
    }

    internal static void OnMenuChanged(IClickableMenu? newMenu)
    {
        if (!IsLookupMenu(newMenu))
        {
            Reset();
            return;
        }

        _lastMenu = newMenu;
        _selectedIndex = 0;
        CollapsedLinkText.Clear();
        _expandedFieldState = null;
        SpeakCurrentField(newMenu!, announceSubject: true);
    }

    internal static void HandleButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (e is null || !e.Pressed.Any())
        {
            return;
        }

        if (Game1.activeClickableMenu is not IClickableMenu menu || !IsLookupMenu(menu))
        {
            return;
        }

        if (IsTextInputActive())
        {
            return;
        }

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
            {
                SpeakExpandedFieldEntry(menu, includeCollapseHint: true, previousIndex: null);
            }
            else if (_expandedFieldState == null && TryExpandCurrentField(menu))
            {
                SpeakCurrentField(menu, announceSubject: false);
            }
            return;
        }

        bool leftPressed = IsConfiguredButtonPressed(Game1.options.moveLeftButton, e.Pressed);
        if (leftPressed)
        {
            SuppressConfiguredButtons(Game1.options.moveLeftButton, e.Pressed);
            if (_expandedFieldState != null && TryExitExpandedField())
            {
                SpeakCurrentField(menu, announceSubject: false);
            }
            else if (TryCollapseCurrentField(menu))
            {
                SpeakCurrentField(menu, announceSubject: false);
            }
        }
    }

    private static void Reset()
    {
        _lastMenu = null;
        _selectedIndex = -1;
        _expandedFieldState = null;
        CollapsedLinkText.Clear();
    }

    private static bool IsLookupMenu(IClickableMenu? menu)
        => menu != null && string.Equals(menu.GetType().FullName, LookupMenuTypeName, StringComparison.Ordinal);

    private static bool TryGetFields(IClickableMenu menu, out object[] fields)
    {
        fields = [];
        if (!IsLookupMenu(menu))
        {
            return false;
        }

        _fieldsField ??= menu.GetType().GetField("Fields", BindingFlags.Instance | BindingFlags.NonPublic);
        if (_fieldsField == null)
        {
            return false;
        }

        object? value = _fieldsField.GetValue(menu);
        if (value is not Array array || array.Length == 0)
        {
            return false;
        }

        fields = new object[array.Length];
        array.CopyTo(fields, 0);
        return fields.Length > 0;
    }

    private static bool MoveSelection(IClickableMenu menu, int direction)
    {
        if (!TryGetFields(menu, out object[] fields))
        {
            return false;
        }

        bool hasHeader = HasNavigableHeader(menu);
        if (_selectedIndex < (hasHeader ? -1 : 0) || _selectedIndex >= fields.Length)
        {
            _selectedIndex = 0;
        }

        int itemCount = fields.Length + (hasHeader ? 1 : 0);
        int currentPosition = hasHeader
            ? _selectedIndex + 1
            : _selectedIndex;

        currentPosition += direction;
        if (currentPosition < 0)
        {
            currentPosition = itemCount - 1;
        }
        else if (currentPosition >= itemCount)
        {
            currentPosition = 0;
        }

        _selectedIndex = hasHeader
            ? currentPosition - 1
            : currentPosition;

        SpeakCurrentField(menu, announceSubject: false);
        return true;
    }

    private static bool MoveExpandedSelection(IClickableMenu menu, int direction)
    {
        if (_expandedFieldState == null || _expandedFieldState.Entries.Length == 0)
        {
            return false;
        }

        int previousIndex = _expandedFieldState.EntryIndex;
        _expandedFieldState.EntryIndex += direction;
        if (_expandedFieldState.EntryIndex < 0)
        {
            _expandedFieldState.EntryIndex = _expandedFieldState.Entries.Length - 1;
        }
        else if (_expandedFieldState.EntryIndex >= _expandedFieldState.Entries.Length)
        {
            _expandedFieldState.EntryIndex = 0;
        }

        SpeakExpandedFieldEntry(menu, includeCollapseHint: false, previousIndex);
        return true;
    }

    private static void SpeakCurrentField(IClickableMenu menu, bool announceSubject)
    {
        if (!TryGetFields(menu, out object[] fields))
        {
            return;
        }

        string subjectInfo = GetSubjectInfo(menu);
        if (_selectedIndex == -1 && !string.IsNullOrWhiteSpace(subjectInfo))
        {
            StardewAccess?.SayWithMenuChecker(subjectInfo, true, $"lookup-header:{subjectInfo}");
            return;
        }

        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
        {
            _selectedIndex = 0;
        }

        object field = fields[_selectedIndex];
        string label = GetFieldLabel(field);
        string? value = GetFieldValueText(field);

        string text = string.IsNullOrWhiteSpace(value) ? label : $"{label}, {value}";
        if (TryGetAccessibleListEntries(field, out AccessibleListEntry[] entries) && entries.Length >= 2)
        {
            text = AppendInstruction(text, GetExpandHintText());
        }

        if (announceSubject)
        {
            if (!string.IsNullOrWhiteSpace(subjectInfo))
            {
                text = $"{subjectInfo}. {text}";
            }
        }

        StardewAccess?.SayWithMenuChecker(text, true, $"lookup-top:{_selectedIndex}:{text}");
    }

    private static void SpeakExpandedFieldEntry(IClickableMenu menu, bool includeCollapseHint, int? previousIndex)
    {
        if (_expandedFieldState == null || _expandedFieldState.Entries.Length == 0)
        {
            return;
        }

        _selectedIndex = _expandedFieldState.FieldIndex;
        if (_expandedFieldState.EntryIndex < 0 || _expandedFieldState.EntryIndex >= _expandedFieldState.Entries.Length)
        {
            _expandedFieldState.EntryIndex = 0;
        }

        AccessibleListEntry entry = _expandedFieldState.Entries[_expandedFieldState.EntryIndex];
        string position = GetListPositionText(_expandedFieldState.EntryIndex + 1, _expandedFieldState.Entries.Length);

        string? context = entry.Context;
        if (previousIndex is >= 0 && previousIndex < _expandedFieldState.Entries.Length)
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
        if (includeCollapseHint)
            text = AppendInstruction(text, GetCollapseHintText());

        StardewAccess?.SayWithMenuChecker(text, true, $"lookup-entry:{_expandedFieldState.FieldIndex}:{_expandedFieldState.EntryIndex}:{text}");
    }

    private static string GetSubjectInfo(IClickableMenu menu)
    {
        _subjectField ??= menu.GetType().GetField("Subject", BindingFlags.Instance | BindingFlags.NonPublic);
        object? subject = _subjectField?.GetValue(menu);
        if (subject == null)
        {
            return "";
        }

        _subjectNameProperty ??= subject.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
        _subjectTypeProperty ??= subject.GetType().GetProperty("Type", BindingFlags.Instance | BindingFlags.Public);
        _subjectDescriptionProperty ??= subject.GetType().GetProperty("Description", BindingFlags.Instance | BindingFlags.Public);

        string? name = _subjectNameProperty?.GetValue(subject) as string;
        string? type = _subjectTypeProperty?.GetValue(subject) as string;
        string? description = _subjectDescriptionProperty?.GetValue(subject) as string;

        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(name))
        {
            parts.Add(name);
        }
        if (!string.IsNullOrWhiteSpace(type))
        {
            parts.Add(type);
        }
        if (!string.IsNullOrWhiteSpace(description))
        {
            parts.Add(description);
        }

        return string.Join(", ", parts);
    }

    private static bool HasNavigableHeader(IClickableMenu menu)
        => !string.IsNullOrWhiteSpace(GetSubjectInfo(menu));

    private static bool TryExpandCurrentField(IClickableMenu menu)
    {
        if (!TryGetFields(menu, out object[] fields))
        {
            return false;
        }

        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
        {
            return false;
        }

        object field = fields[_selectedIndex];
        object? expandLink = GetFieldExpandLink(field);
        if (expandLink == null)
        {
            return false;
        }

        string? linkText = GetFormattedText(expandLink, "Value");
        if (!string.IsNullOrWhiteSpace(linkText))
        {
            CollapsedLinkText[field] = linkText;
        }

        InvokeTryGetLinkAt(field);
        return true;
    }

    private static bool TryCollapseCurrentField(IClickableMenu menu)
    {
        if (!TryGetFields(menu, out object[] fields))
        {
            return false;
        }

        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
        {
            return false;
        }

        object field = fields[_selectedIndex];
        if (GetFieldExpandLink(field) != null)
        {
            return false;
        }

        if (!CollapsedLinkText.TryGetValue(field, out string? linkText) || string.IsNullOrWhiteSpace(linkText))
        {
            return false;
        }

        return TryCollapseField(field, linkText);
    }

    private static bool TryEnterExpandedField(IClickableMenu menu)
    {
        if (!TryGetFields(menu, out object[] fields))
        {
            return false;
        }

        if (_selectedIndex < 0 || _selectedIndex >= fields.Length)
        {
            return false;
        }

        object field = fields[_selectedIndex];
        if (!TryGetAccessibleListEntries(field, out AccessibleListEntry[] entries) || entries.Length < 2)
        {
            return false;
        }

        string? collapseLinkText = null;
        bool restoreCollapsedState = false;
        object? expandLink = GetFieldExpandLink(field);
        if (expandLink != null)
        {
            collapseLinkText = GetFormattedText(expandLink, "Value");
            restoreCollapsedState = TryExpandCurrentField(menu);
            if (!restoreCollapsedState)
            {
                return false;
            }
        }

        _expandedFieldState = new ExpandedFieldState(field, _selectedIndex, entries, collapseLinkText, restoreCollapsedState);
        return true;
    }

    private static bool TryExitExpandedField()
    {
        if (_expandedFieldState == null)
        {
            return false;
        }

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

    private static void InvokeTryGetLinkAt(object field)
    {
        MethodInfo? method = field.GetType().GetMethod("TryGetLinkAt", BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            return;
        }

        object?[] parameters = [0, 0, null];
        method.Invoke(field, parameters);
    }

    private static bool TryCollapseField(object field, string linkText)
    {
        MethodInfo? collapseByDefault = field.GetType().GetMethod("CollapseByDefault", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (collapseByDefault == null)
        {
            return false;
        }

        collapseByDefault.Invoke(field, [linkText]);
        return true;
    }

    private static string GetFieldLabel(object field)
    {
        _fieldLabelProperty ??= field.GetType().GetProperty("Label", BindingFlags.Instance | BindingFlags.Public);
        return _fieldLabelProperty?.GetValue(field) as string ?? "Field";
    }

    private static object? GetFieldExpandLink(object field)
    {
        _fieldExpandLinkProperty ??= field.GetType().GetProperty("ExpandLink", BindingFlags.Instance | BindingFlags.Public);
        return _fieldExpandLinkProperty?.GetValue(field);
    }

    private static string? GetFieldValueText(object field)
    {
        object? expandLink = GetFieldExpandLink(field);
        if (expandLink != null)
        {
            return GetFormattedText(expandLink, "Value");
        }

        _fieldValueProperty ??= field.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
        object? value = _fieldValueProperty?.GetValue(field);
        if (value is Array array)
        {
            string? joined = JoinFormattedText(array);
            if (!string.IsNullOrWhiteSpace(joined))
            {
                return joined;
            }
        }

        string fieldTypeName = field.GetType().Name;
        if (string.Equals(fieldTypeName, "SkillBarField", StringComparison.Ordinal))
        {
            return GetSkillBarText(field);
        }

        if (string.Equals(fieldTypeName, "CharacterFriendshipField", StringComparison.Ordinal))
        {
            return GetFriendshipText(field);
        }

        if (string.Equals(fieldTypeName, "ItemRecipesField", StringComparison.Ordinal))
        {
            return GetItemRecipesText(field);
        }

        if (string.Equals(fieldTypeName, "ItemDropListField", StringComparison.Ordinal))
        {
            return GetItemDropListText(field);
        }

        if (string.Equals(fieldTypeName, "FishPondDropsField", StringComparison.Ordinal))
        {
            return GetFishPondDropsText(field);
        }

        if (IsCheckboxListField(field))
        {
            return GetCheckboxListText(field);
        }

        if (IsPercentageBarField(field))
        {
            string? percentageText = GetPercentageBarText(field);
            if (!string.IsNullOrWhiteSpace(percentageText))
            {
                return percentageText;
            }
        }

        if (string.Equals(fieldTypeName, "ItemIconListField", StringComparison.Ordinal))
        {
            return GetItemIconListText(field);
        }

        if (string.Equals(fieldTypeName, "ColorField", StringComparison.Ordinal))
        {
            return GetColorFieldText(field);
        }

        return null;
    }

    private static bool TryGetAccessibleListEntries(object field, out AccessibleListEntry[] entries)
    {
        string fieldTypeName = field.GetType().Name;
        entries =
            string.Equals(fieldTypeName, "ItemIconListField", StringComparison.Ordinal) ? GetItemIconListEntries(field)
            : IsCheckboxListField(field) ? GetCheckboxListEntries(field)
            : string.Equals(fieldTypeName, "ItemRecipesField", StringComparison.Ordinal) ? GetItemRecipeEntries(field)
            : string.Equals(fieldTypeName, "ItemDropListField", StringComparison.Ordinal) ? GetItemDropListEntries(field)
            : string.Equals(fieldTypeName, "FishPondDropsField", StringComparison.Ordinal) ? GetFishPondDropEntries(field)
            : string.Equals(fieldTypeName, "CharacterGiftTastesField", StringComparison.Ordinal) ? GetFormattedValueListEntries(field)
            : string.Equals(fieldTypeName, "ItemGiftTastesField", StringComparison.Ordinal) ? GetFormattedValueListEntries(field)
            : string.Equals(fieldTypeName, "MovieTastesField", StringComparison.Ordinal) ? GetMovieTasteEntries(field)
            : [];

        return entries.Length > 0;
    }

    private static AccessibleListEntry[] GetItemIconListEntries(object field)
    {
        if (GetMemberValue(field, "Items") is not Array items || items.Length == 0)
        {
            return [];
        }

        string? introText = GetMemberValue(field, "IntroText") as string;
        bool showStackSize = GetBoolMember(field, "ShowStackSize");
        Delegate? formatItemName = GetMemberValue(field, "FormatItemName") as Delegate;

        List<AccessibleListEntry> entries = [];
        foreach (object itemEntry in items)
        {
            if (GetMemberValue(itemEntry, "Item1") is not Item item)
            {
                continue;
            }

            string? name = formatItemName?.DynamicInvoke(item) as string;
            name ??= item.DisplayName;
            if (showStackSize && item.Stack > 1)
            {
                name += $" x{item.Stack}";
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                entries.Add(new AccessibleListEntry(name, introText));
            }
        }

        return entries.ToArray();
    }

    private static AccessibleListEntry[] GetCheckboxListEntries(object field)
    {
        FieldInfo? listsField = GetFieldInHierarchy(field.GetType(), "CheckboxLists");
        if (listsField?.GetValue(field) is not Array lists || lists.Length == 0)
        {
            return [];
        }

        List<AccessibleListEntry> entries = [];
        int hiddenCount = 0;
        foreach (object list in lists)
        {
            if (GetBoolMember(list, "IsHidden"))
            {
                hiddenCount++;
                continue;
            }

            string? introText = GetNestedMemberText(list, "Intro", "Text");
            if (GetMemberValue(list, "Checkboxes") is not Array checkboxes)
            {
                continue;
            }

            foreach (object checkbox in checkboxes)
            {
                string? text = JoinFormattedText(GetMemberValue(checkbox, "Text") as Array);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                string stateText = GetCheckboxStateText(GetBoolMember(checkbox, "IsChecked"));
                entries.Add(new AccessibleListEntry($"{stateText}: {text}", introText));
            }
        }

        if (hiddenCount > 0)
        {
            string hiddenText =
                string.Equals(field.GetType().Name, "FishSpawnRulesField", StringComparison.Ordinal)
                    ? (TryInvokeLookupI18nMethod("Item_UncaughtFish", hiddenCount) ?? $"uncaught fish: {hiddenCount}")
                    : $"hidden entries: {hiddenCount}";
            entries.Add(new AccessibleListEntry(hiddenText));
        }

        return entries.ToArray();
    }

    private static AccessibleListEntry[] GetItemRecipeEntries(object field)
    {
        FieldInfo? recipesField = GetFieldInHierarchy(field.GetType(), "RecipesByType");
        if (recipesField?.GetValue(field) is not Array groups || groups.Length == 0)
        {
            return [];
        }

        bool showUnknown = GetBoolMember(field, "ShowUnknownRecipes", defaultValue: true);
        bool showInvalid = GetBoolMember(field, "ShowInvalidRecipes", defaultValue: true);
        bool showOutputLabels = GetBoolMember(field, "ShowOutputLabels", defaultValue: true);

        List<AccessibleListEntry> entries = [];
        foreach (object group in groups)
        {
            string type = GetMemberValue(group, "Type") as string ?? "Recipes";
            if (GetMemberValue(group, "Recipes") is not Array recipes)
            {
                continue;
            }

            int unknownCount = 0;
            foreach (object recipe in recipes)
            {
                bool isKnown = GetBoolMember(recipe, "IsKnown", defaultValue: true);
                bool isValid = GetBoolMember(recipe, "IsValid", defaultValue: true);
                if (!showInvalid && !isValid)
                {
                    continue;
                }
                if (!showUnknown && !isKnown)
                {
                    unknownCount++;
                    continue;
                }

                string output = GetRecipeItemText(GetMemberValue(recipe, "Output"));
                string[] inputs = GetRecipeItemInputs(GetMemberValue(recipe, "Inputs"));
                string description = inputs.Length > 0
                    ? (showOutputLabels ? $"{output} <- {string.Join(" + ", inputs)}" : string.Join(" + ", inputs))
                    : output;

                string? conditions = GetMemberValue(recipe, "Conditions") as string;
                if (!string.IsNullOrWhiteSpace(conditions))
                {
                    description += $" ({conditions})";
                }

                if (showUnknown && !isKnown)
                {
                    description += " (unknown)";
                }

                entries.Add(new AccessibleListEntry(description, type));
            }

            if (!showUnknown && unknownCount > 0)
            {
                string unknownText = TryInvokeLookupI18nMethod("Item_UnknownRecipes", unknownCount)
                                     ?? $"unknown recipes: {unknownCount}";
                entries.Add(new AccessibleListEntry(unknownText, type));
            }
        }

        return entries.ToArray();
    }

    private static AccessibleListEntry[] GetItemDropListEntries(object field)
    {
        if (GetMemberValue(field, "Drops") is not Array drops || drops.Length == 0)
        {
            return [];
        }

        string? preface = GetMemberValue(field, "Preface") as string;
        List<AccessibleListEntry> entries = [];
        foreach (object entry in drops)
        {
            object? dropData = GetMemberValue(entry, "Item1");
            Item? item = GetMemberValue(entry, "Item2") as Item;
            if (dropData == null || item == null)
            {
                continue;
            }

            float probability = GetFloatMember(dropData, "Probability");
            int minDrop = GetIntMember(dropData, "MinDrop");
            int maxDrop = GetIntMember(dropData, "MaxDrop");
            string? conditions = GetMemberValue(dropData, "Conditions") as string;

            string text = probability > 0f && probability < 1f
                ? $"{Math.Round(probability * 100f)}% {item.DisplayName}"
                : item.DisplayName;
            if (minDrop != maxDrop)
            {
                text += $" ({minDrop}-{maxDrop})";
            }
            else if (minDrop > 1)
            {
                text += $" ({minDrop})";
            }
            if (!string.IsNullOrWhiteSpace(conditions))
            {
                text += $" ({conditions})";
            }

            entries.Add(new AccessibleListEntry(text, preface));
        }

        return entries.ToArray();
    }

    private static AccessibleListEntry[] GetFishPondDropEntries(object field)
    {
        if (GetMemberValue(field, "Drops") is not Array drops || drops.Length == 0)
        {
            return [];
        }

        string? preface = GetMemberValue(field, "Preface") as string;
        List<AccessibleListEntry> entries = [];
        foreach (object drop in drops)
        {
            string? entryText = FormatFishPondDropEntry(drop);
            if (string.IsNullOrWhiteSpace(entryText))
            {
                continue;
            }

            int minPopulation = GetIntMember(drop, "MinPopulation");
            string minFishText = TryInvokeLookupI18nMethod("Building_FishPond_Drops_MinFish", minPopulation)
                                 ?? $"min fish {minPopulation}";
            string context = string.IsNullOrWhiteSpace(preface)
                ? minFishText
                : $"{preface}, {minFishText}";
            entries.Add(new AccessibleListEntry(entryText, context));
        }

        return entries.ToArray();
    }

    private static AccessibleListEntry[] GetFormattedValueListEntries(object field)
    {
        if (GetMemberValue(field, "Value") is not Array valueEntries || valueEntries.Length == 0)
        {
            return [];
        }

        string listSeparator = GetListSeparator();
        List<AccessibleListEntry> entries = [];
        foreach (object entry in valueEntries)
        {
            if (GetMemberValue(entry, "Text") is not string text || string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            text = TrimTrailingSeparator(text.Trim(), listSeparator);
            if (!string.IsNullOrWhiteSpace(text))
            {
                entries.Add(new AccessibleListEntry(text));
            }
        }

        return entries.ToArray();
    }

    private static AccessibleListEntry[] GetMovieTasteEntries(object field)
    {
        if (GetMemberValue(field, "Value") is not Array valueEntries || valueEntries.Length == 0)
        {
            return [];
        }

        string? joined = JoinFormattedText(valueEntries);
        if (string.IsNullOrWhiteSpace(joined))
        {
            return [];
        }

        string separator = GetListSeparator();
        return joined
            .Split([separator], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => new AccessibleListEntry(part))
            .ToArray();
    }

    private static string GetCheckboxStateText(bool isChecked)
        => isChecked
            ? (TryInvokeLookupI18nMethod("Generic_Yes") ?? "yes")
            : (TryInvokeLookupI18nMethod("Generic_No") ?? "no");

    private static string GetListSeparator()
        => TryInvokeLookupI18nMethod("Generic_ListSeparator") ?? ", ";

    private static string TrimTrailingSeparator(string text, string separator)
    {
        return text.EndsWith(separator, StringComparison.Ordinal)
            ? text[..^separator.Length].TrimEnd()
            : text;
    }

    private static string AppendInstruction(string text, string instruction)
    {
        return string.IsNullOrWhiteSpace(instruction)
            ? text
            : $"{text}, {instruction}";
    }

    private static string GetExpandHintText()
        => TryInvokeLookupI18nMethod("ScreenReader_ListExpandHint") ?? "right to expand";

    private static string GetCollapseHintText()
        => TryInvokeLookupI18nMethod("ScreenReader_ListCollapseHint") ?? "left to collapse";

    private static string GetListPositionText(int current, int total)
        => TryInvokeLookupI18nMethod("ScreenReader_ListPosition", current, total) ?? $"{current} of {total}";

    private static string? GetFormattedText(object formattedTextOwner, string propertyName)
    {
        _formattedTextProperty ??= formattedTextOwner.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        object? value = _formattedTextProperty?.GetValue(formattedTextOwner);
        if (value is Array array)
        {
            return JoinFormattedText(array);
        }

        return null;
    }

    private static string? JoinFormattedText(Array? formattedTextArray)
    {
        if (formattedTextArray == null)
        {
            return null;
        }

        List<string> parts = [];
        foreach (object entry in formattedTextArray)
        {
            PropertyInfo? textProperty = entry.GetType().GetProperty("Text", BindingFlags.Instance | BindingFlags.Public);
            if (textProperty?.GetValue(entry) is string text && !string.IsNullOrWhiteSpace(text))
            {
                parts.Add(text.Trim());
            }
        }

        return parts.Count > 0 ? string.Join("", parts) : null;
    }

    private static string? GetItemIconListText(object field)
    {
        FieldInfo? itemsField = field.GetType().GetField("Items", BindingFlags.Instance | BindingFlags.NonPublic);
        if (itemsField == null)
        {
            return null;
        }

        if (itemsField.GetValue(field) is not Array items)
        {
            return null;
        }

        List<string> names = [];
        foreach (object entry in items)
        {
            PropertyInfo? itemProperty = entry.GetType().GetProperty("Item1", BindingFlags.Instance | BindingFlags.Public);
            if (itemProperty?.GetValue(entry) is not Item item)
            {
                continue;
            }

            string name = item.DisplayName;
            if (item.Stack > 1)
            {
                name = $"{name} x{item.Stack}";
            }
            names.Add(name);
        }

        return names.Count > 0 ? string.Join(", ", names) : null;
    }

    private static bool IsCheckboxListField(object field)
    {
        Type? type = field.GetType();
        while (type != null)
        {
            if (string.Equals(type.Name, "CheckboxListField", StringComparison.Ordinal))
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static string? GetCheckboxListText(object field)
    {
        FieldInfo? listsField = GetFieldInHierarchy(field.GetType(), "CheckboxLists");
        if (listsField?.GetValue(field) is not Array lists || lists.Length == 0)
        {
            return null;
        }

        List<string> listTexts = [];
        int hiddenCount = 0;
        foreach (object list in lists)
        {
            bool isHidden = GetBoolMember(list, "IsHidden");
            if (isHidden)
            {
                hiddenCount++;
                continue;
            }

            string? introText = GetNestedMemberText(list, "Intro", "Text");
            List<string> checkboxTexts = [];
            if (GetMemberValue(list, "Checkboxes") is Array checkboxes)
            {
                foreach (object checkbox in checkboxes)
                {
                    bool isChecked = GetBoolMember(checkbox, "IsChecked");
                    string? text = JoinFormattedText(GetMemberValue(checkbox, "Text") as Array);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    checkboxTexts.Add($"{(isChecked ? "yes" : "no")}: {text}");
                }
            }

            if (checkboxTexts.Count == 0)
            {
                continue;
            }

            string combined = string.IsNullOrWhiteSpace(introText)
                ? string.Join("; ", checkboxTexts)
                : $"{introText}: {string.Join("; ", checkboxTexts)}";
            listTexts.Add(combined);
        }

        if (hiddenCount > 0)
        {
            string? hiddenText = null;
            if (string.Equals(field.GetType().Name, "FishSpawnRulesField", StringComparison.Ordinal))
            {
                hiddenText = TryInvokeLookupI18nMethod("Item_UncaughtFish", hiddenCount)
                             ?? $"uncaught fish: {hiddenCount}";
            }

            hiddenText ??= $"hidden entries: {hiddenCount}";
            listTexts.Add(hiddenText);
        }

        return listTexts.Count > 0 ? string.Join(" | ", listTexts) : null;
    }

    private static bool IsPercentageBarField(object field)
    {
        Type? type = field.GetType();
        while (type != null)
        {
            if (string.Equals(type.Name, "PercentageBarField", StringComparison.Ordinal))
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static string? GetPercentageBarText(object field)
    {
        FieldInfo? textField = GetFieldInHierarchy(field.GetType(), "Text");
        if (textField?.GetValue(field) is string text && !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        FieldInfo? currentValueField = GetFieldInHierarchy(field.GetType(), "CurrentValue");
        FieldInfo? maxValueField = GetFieldInHierarchy(field.GetType(), "MaxValue");
        if (currentValueField?.GetValue(field) is int currentValue &&
            maxValueField?.GetValue(field) is int maxValue &&
            maxValue > 0)
        {
            return $"{currentValue}/{maxValue}";
        }

        return null;
    }

    private static string? GetSkillBarText(object field)
    {
        FieldInfo? skillPointsField = GetFieldInHierarchy(field.GetType(), "SkillPointsPerLevel");
        FieldInfo? currentValueField = GetFieldInHierarchy(field.GetType(), "CurrentValue");
        if (skillPointsField?.GetValue(field) is not int[] skillPointsPerLevel ||
            currentValueField?.GetValue(field) is not int currentValue ||
            skillPointsPerLevel.Length == 0)
        {
            return null;
        }

        int nextThreshold = skillPointsPerLevel.FirstOrDefault(points => points - currentValue > 0);
        if (nextThreshold > 0)
        {
            int expNeeded = nextThreshold - currentValue;
            int level = Array.IndexOf(skillPointsPerLevel, nextThreshold);
            return TryInvokeLookupI18n("Player_Skill_Progress", level, expNeeded)
                   ?? $"level {level} ({expNeeded} XP to next)";
        }

        int maxLevel = skillPointsPerLevel.Length;
        return TryInvokeLookupI18n("Player_Skill_ProgressLast", maxLevel)
               ?? $"level {maxLevel}";
    }

    private static string? GetFriendshipText(object field)
    {
        object? friendship = GetMemberValue(field, "Friendship");
        if (friendship == null)
        {
            return null;
        }

        int filledHearts = GetIntMember(friendship, "FilledHearts");
        int totalHearts = GetIntMember(friendship, "TotalHearts");
        int lockedHearts = GetIntMember(friendship, "LockedHearts");
        int points = GetIntMember(friendship, "Points");
        int maxPoints = GetIntMember(friendship, "MaxPoints");
        int pointsToNext = InvokeGetPointsToNext(friendship);

        List<string> parts = [];
        if (totalHearts > 0)
        {
            parts.Add($"{filledHearts} of {totalHearts} hearts");
        }
        if (lockedHearts > 0)
        {
            parts.Add($"{lockedHearts} locked hearts");
        }
        if (maxPoints > 0)
        {
            parts.Add($"{points}/{maxPoints} points");
        }
        if (pointsToNext > 0)
        {
            parts.Add($"{pointsToNext} points to next");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    private static string? GetItemRecipesText(object field)
    {
        FieldInfo? recipesField = GetFieldInHierarchy(field.GetType(), "RecipesByType");
        if (recipesField?.GetValue(field) is not Array groups || groups.Length == 0)
        {
            return null;
        }

        bool showUnknown = GetBoolMember(field, "ShowUnknownRecipes", defaultValue: true);
        bool showInvalid = GetBoolMember(field, "ShowInvalidRecipes", defaultValue: true);
        bool showOutputLabels = GetBoolMember(field, "ShowOutputLabels", defaultValue: true);

        List<string> groupTexts = [];
        foreach (object group in groups)
        {
            string type = GetMemberValue(group, "Type") as string ?? "Recipes";
            if (GetMemberValue(group, "Recipes") is not Array recipes)
            {
                continue;
            }

            List<string> recipeTexts = [];
            int unknownCount = 0;
            foreach (object recipe in recipes)
            {
                bool isKnown = GetBoolMember(recipe, "IsKnown", defaultValue: true);
                bool isValid = GetBoolMember(recipe, "IsValid", defaultValue: true);
                if (!showInvalid && !isValid)
                {
                    continue;
                }
                if (!showUnknown && !isKnown)
                {
                    unknownCount++;
                    continue;
                }

                string output = GetRecipeItemText(GetMemberValue(recipe, "Output"));
                string[] inputs = GetRecipeItemInputs(GetMemberValue(recipe, "Inputs"));
                string description;
                if (inputs.Length > 0)
                {
                    description = showOutputLabels
                        ? $"{output} <- {string.Join(" + ", inputs)}"
                        : string.Join(" + ", inputs);
                }
                else
                {
                    description = output;
                }

                string? conditions = GetMemberValue(recipe, "Conditions") as string;
                if (!string.IsNullOrWhiteSpace(conditions))
                {
                    description += $" ({conditions})";
                }

                if (showUnknown && !isKnown)
                {
                    description += " (unknown)";
                }

                recipeTexts.Add(description);
            }

            if (!showUnknown && unknownCount > 0)
            {
                string unknownText = TryInvokeLookupI18nMethod("Item_UnknownRecipes", unknownCount)
                                     ?? $"unknown recipes: {unknownCount}";
                recipeTexts.Add(unknownText);
            }

            if (recipeTexts.Count == 0)
            {
                continue;
            }

            groupTexts.Add($"{type}: {string.Join("; ", recipeTexts)}");
        }

        return groupTexts.Count > 0 ? string.Join(" | ", groupTexts) : null;
    }

    private static string? GetItemDropListText(object field)
    {
        object? dropsValue = GetMemberValue(field, "Drops");
        if (dropsValue is not Array drops)
        {
            string? defaultText = GetMemberValue(field, "DefaultText") as string;
            return string.IsNullOrWhiteSpace(defaultText) ? null : defaultText;
        }

        string? preface = GetMemberValue(field, "Preface") as string;
        List<string> dropTexts = [];
        foreach (object entry in drops)
        {
            object? dropData = GetMemberValue(entry, "Item1");
            Item? item = GetMemberValue(entry, "Item2") as Item;
            if (dropData == null || item == null)
            {
                continue;
            }

            float probability = GetFloatMember(dropData, "Probability");
            int minDrop = GetIntMember(dropData, "MinDrop");
            int maxDrop = GetIntMember(dropData, "MaxDrop");
            string? conditions = GetMemberValue(dropData, "Conditions") as string;

            string text = item.DisplayName;
            if (probability > 0f && probability < 1f)
            {
                text = $"{Math.Round(probability * 100f)}% {text}";
            }
            if (minDrop != maxDrop)
            {
                text += $" ({minDrop}-{maxDrop})";
            }
            else if (minDrop > 1)
            {
                text += $" ({minDrop})";
            }
            if (!string.IsNullOrWhiteSpace(conditions))
            {
                text += $" ({conditions})";
            }

            dropTexts.Add(text);
        }

        if (dropTexts.Count == 0)
        {
            return null;
        }

        string combined = string.Join(", ", dropTexts);
        if (!string.IsNullOrWhiteSpace(preface))
        {
            combined = $"{preface}: {combined}";
        }

        return combined;
    }

    private static string? GetFishPondDropsText(object field)
    {
        if (GetMemberValue(field, "Drops") is not Array drops || drops.Length == 0)
        {
            return null;
        }

        string? preface = GetMemberValue(field, "Preface") as string;
        List<string> groupTexts = [];
        int? currentMin = null;
        List<string> currentEntries = [];

        foreach (object drop in drops)
        {
            int minPopulation = GetIntMember(drop, "MinPopulation");
            if (currentMin == null || minPopulation != currentMin.Value)
            {
                if (currentMin != null && currentEntries.Count > 0)
                {
                    groupTexts.Add(FormatFishPondDropGroup(currentMin.Value, currentEntries));
                }
                currentMin = minPopulation;
                currentEntries = [];
            }

            string? entryText = FormatFishPondDropEntry(drop);
            if (!string.IsNullOrWhiteSpace(entryText))
            {
                currentEntries.Add(entryText);
            }
        }

        if (currentMin != null && currentEntries.Count > 0)
        {
            groupTexts.Add(FormatFishPondDropGroup(currentMin.Value, currentEntries));
        }

        if (groupTexts.Count == 0)
        {
            return null;
        }

        string combined = string.Join(" | ", groupTexts);
        if (!string.IsNullOrWhiteSpace(preface))
        {
            combined = $"{preface}: {combined}";
        }

        return combined;
    }

    private static string FormatFishPondDropGroup(int minPopulation, IReadOnlyList<string> entries)
    {
        string label = TryInvokeLookupI18nMethod("Building_FishPond_Drops_MinFish", minPopulation)
                       ?? $"min fish {minPopulation}";
        return $"{label}: {string.Join(", ", entries)}";
    }

    private static string? FormatFishPondDropEntry(object drop)
    {
        if (GetMemberValue(drop, "SampleItem") is not Item item)
        {
            return null;
        }

        float probability = GetFloatMember(drop, "Probability");
        int minDrop = GetIntMember(drop, "MinDrop");
        int maxDrop = GetIntMember(drop, "MaxDrop");
        string? conditions = GetMemberValue(drop, "Conditions") as string;
        bool isUnlocked = GetBoolMember(drop, "IsUnlocked", defaultValue: true);

        string text = item.DisplayName;
        if (probability > 0f)
        {
            text = $"{Math.Round(probability * 100f)}% {text}";
        }

        if (minDrop != maxDrop)
        {
            text += $" ({minDrop}-{maxDrop})";
        }
        else if (minDrop > 1)
        {
            text += $" ({minDrop})";
        }

        if (!string.IsNullOrWhiteSpace(conditions))
        {
            text += $" ({conditions})";
        }

        if (!isUnlocked)
        {
            text = $"locked {text}";
        }

        return text;
    }

    private static string? GetColorFieldText(object field)
    {
        bool isPrismatic = GetBoolMember(field, "IsPrismatic");
        if (isPrismatic)
        {
            return "prismatic";
        }

        if (GetMemberValue(field, "Color") is not Microsoft.Xna.Framework.Color color)
        {
            return null;
        }

        string colorName = GetApproximateColorName(color);
        int strength = GetIntMember(field, "Strength");
        string strengthText = strength switch
        {
            1 => "weak",
            2 => "medium",
            3 => "strong",
            _ => ""
        };

        if (!string.IsNullOrWhiteSpace(strengthText))
        {
            return $"{colorName}, {strengthText}";
        }

        return colorName;
    }

    private static string GetRecipeItemText(object? entry)
    {
        if (entry == null)
        {
            return "Unknown";
        }

        string? text = GetMemberValue(entry, "DisplayText") as string;
        if (string.IsNullOrWhiteSpace(text))
        {
            text =
                GetNestedMemberText(entry, "Entity", "DisplayName")
                ?? GetNestedMemberText(entry, "Entity", "Name");

            if (string.IsNullOrWhiteSpace(text))
            {
                return "Unknown";
            }
        }

        text = text.Trim().TrimEnd(':');

        bool isGoldPrice = GetBoolMember(entry, "IsGoldPrice");
        if (isGoldPrice)
        {
            text = $"{text} gold";
        }

        int quality = GetIntMember(entry, "Quality");
        if (quality > 0)
        {
            string qualityText = GetQualityText(quality);
            if (!string.IsNullOrWhiteSpace(qualityText))
            {
                text = $"{text} ({qualityText})";
            }
        }

        return text;
    }

    private static string[] GetRecipeItemInputs(object? inputsValue)
    {
        if (inputsValue is not Array inputs || inputs.Length == 0)
        {
            return [];
        }

        List<string> items = [];
        foreach (object input in inputs)
        {
            string text = GetRecipeItemText(input);
            if (!string.IsNullOrWhiteSpace(text))
            {
                items.Add(text);
            }
        }

        return items.ToArray();
    }

    private static FieldInfo? GetFieldInHierarchy(Type type, string name)
    {
        while (type != null)
        {
            FieldInfo? field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static object? GetMemberValue(object instance, string name)
    {
        Type type = instance.GetType();
        PropertyInfo? prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            return prop.GetValue(instance);
        }

        FieldInfo? field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field?.GetValue(instance);
    }

    private static string? GetNestedMemberText(object instance, string parentName, string childName)
    {
        object? parent = GetMemberValue(instance, parentName);
        if (parent == null)
        {
            return null;
        }

        return GetMemberValue(parent, childName) as string;
    }

    private static bool GetBoolMember(object instance, string name, bool defaultValue = false)
    {
        object? value = GetMemberValue(instance, name);
        return value is bool boolValue ? boolValue : defaultValue;
    }

    private static int GetIntMember(object instance, string name)
    {
        object? value = GetMemberValue(instance, name);
        return value is int intValue ? intValue : 0;
    }

    private static float GetFloatMember(object instance, string name)
    {
        object? value = GetMemberValue(instance, name);
        return value is float floatValue ? floatValue : 0f;
    }

    private static int InvokeGetPointsToNext(object friendship)
    {
        MethodInfo? method = friendship.GetType().GetMethod("GetPointsToNext", BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            return 0;
        }

        object? result = method.Invoke(friendship, null);
        return result is int intValue ? intValue : 0;
    }

    private static string? TryInvokeLookupI18n(string methodName, params object?[] args)
    {
        _lookupI18nType ??= Type.GetType("Pathoschild.Stardew.LookupAnything.Framework.I18n, LookupAnything");
        if (_lookupI18nType == null)
        {
            return null;
        }

        MethodInfo? method = methodName == "Player_Skill_Progress"
            ? _lookupSkillProgressMethod ??= _lookupI18nType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
            : _lookupSkillProgressLastMethod ??= _lookupI18nType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

        if (method == null)
        {
            return null;
        }

        return method.Invoke(null, args) as string;
    }

    private static string? TryInvokeLookupI18nMethod(string methodName, params object?[] args)
    {
        _lookupI18nType ??= Type.GetType("Pathoschild.Stardew.LookupAnything.Framework.I18n, LookupAnything");
        if (_lookupI18nType == null)
        {
            return null;
        }

        if (!LookupI18nMethods.TryGetValue(methodName, out MethodInfo? method))
        {
            method = _lookupI18nType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            LookupI18nMethods[methodName] = method;
        }

        if (method == null)
        {
            return null;
        }

        return method.Invoke(null, args) as string;
    }

    private static bool IsConfiguredButtonPressed(InputButton[] configuredButtons, IEnumerable<SButton> pressed)
    {
        foreach (InputButton inputButton in configuredButtons)
        {
            SButton button = inputButton.ToSButton();
            if (pressed.Contains(button))
            {
                return true;
            }
        }

        return false;
    }

    private static void SuppressConfiguredButtons(InputButton[] configuredButtons, IEnumerable<SButton> pressed)
    {
        foreach (InputButton inputButton in configuredButtons)
        {
            SButton button = inputButton.ToSButton();
            if (pressed.Contains(button))
            {
                InputHelper?.Suppress(button);
            }
        }
    }

    private static bool IsTextInputActive()
        => Game1.game1.HasKeyboardFocus() && Game1.game1.instanceKeyboardDispatcher?.Subscriber != null;

    private static string GetQualityText(int quality)
        => Enum.IsDefined(typeof(ItemQuality), quality)
            ? ((ItemQuality)quality).GetName()
            : "";

    private static string GetApproximateColorName(Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));
        int delta = max - min;

        if (max < 35)
        {
            return "black";
        }

        if (delta < 18)
        {
            if (max > 215)
            {
                return "white";
            }

            if (max > 150)
            {
                return "light gray";
            }

            if (max > 90)
            {
                return "gray";
            }

            return "dark gray";
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
            {
                hue = 60f * (((green - blue) / channelDelta) % 6f);
            }
            else if (Math.Abs(maxChannel - green) < 0.0001f)
            {
                hue = 60f * (((blue - red) / channelDelta) + 2f);
            }
            else
            {
                hue = 60f * (((red - green) / channelDelta) + 4f);
            }

            if (hue < 0f)
            {
                hue += 360f;
            }
        }

        if (saturation < 0.22f)
        {
            return brightness > 0.75f ? "pale gray" : "gray";
        }

        return hue switch
        {
            < 15 or >= 345 => brightness < 0.35f ? "dark red" : "red",
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

    private sealed class ExpandedFieldState(object field, int fieldIndex, AccessibleListEntry[] entries, string? collapseLinkText, bool restoreCollapsedState)
    {
        public object Field { get; } = field;
        public int FieldIndex { get; } = fieldIndex;
        public AccessibleListEntry[] Entries { get; } = entries;
        public string? CollapseLinkText { get; } = collapseLinkText;
        public bool RestoreCollapsedState { get; } = restoreCollapsedState;
        public int EntryIndex { get; set; }
    }

    private sealed record AccessibleListEntry(string Text, string? Context = null);

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
