using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Patches.EditData;
using ContentPatcher.Framework.TextOperations;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Toolkit;
using xTile;

namespace ContentPatcher.Framework.Patches;

/// <summary>Metadata for data to edit into a data file.</summary>
internal class EditDataPatch : Patch
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>Constructs key/value editors for arbitrary data.</summary>
    private readonly KeyValueEditorFactory EditorFactory = new();

    /// <summary>The field within the data asset to which edits should be applied, or empty to apply to the root asset.</summary>
    private readonly IManagedTokenString[] TargetField;

    /// <summary>The data records to edit.</summary>
    private EditDataPatchRecord[] Records;

    /// <summary>The data fields to edit.</summary>
    private EditDataPatchField[] Fields;

    /// <summary>The records to reorder, if the target is a list asset.</summary>
    private EditDataPatchMoveRecord[] MoveRecords;

    /// <summary>The text operations to apply to existing values.</summary>
    private readonly ITextOperation[] TextOperations;

    /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
    private readonly TryParseFieldsDelegate TryParseFields;

    /// <summary>The cached JSON serializer used to apply JSON structures to a model.</summary>
    private static readonly Lazy<JsonSerializer> JsonSerializer = new(EditDataPatch.CreateJsonSerializer);

    /// <summary>Whether the file specified by <see cref="Patch.FromAsset"/> has been loaded at least once for this patch.</summary>
    private bool HasEverLoadedFromFile;


    /*********
    ** Accessors
    *********/
    /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
    /// <param name="context">The tokens available for this content pack.</param>
    /// <param name="entry">The change to load.</param>
    /// <param name="entries">The parsed data entry changes.</param>
    /// <param name="fields">The parsed data field changes.</param>
    /// <param name="moveEntries">The parsed move entry records.</param>
    /// <param name="targetField">The field within the data asset to which edits should be applied, or empty to apply to the root asset.</param>
    /// <param name="error">The error message indicating why parsing failed, if applicable.</param>
    /// <returns>Returns whether parsing succeeded.</returns>
    public delegate bool TryParseFieldsDelegate(IContext context, PatchConfig entry, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out List<IManagedTokenString> targetField, [NotNullWhen(false)] out string? error);


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
    /// <param name="path">The path to the patch from the root content file.</param>
    /// <param name="assetName">The normalized asset name to intercept.</param>
    /// <param name="assetLocale">The locale code in the target asset's name to match. See <see cref="IPatch.TargetAsset"/> for more info.</param>
    /// <param name="priority">The priority for this patch when multiple patches apply.</param>
    /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
    /// <param name="fromFile">The normalized asset key from which to load entries (if applicable), including tokens.</param>
    /// <param name="records">The data records to edit.</param>
    /// <param name="fields">The data fields to edit.</param>
    /// <param name="moveRecords">The records to reorder, if the target is a list asset.</param>
    /// <param name="textOperations">The text operations to apply to existing values.</param>
    /// <param name="targetField">The field within the data asset to which edits should be applied, or empty to apply to the root asset.</param>
    /// <param name="updateRate">When the patch should be updated.</param>
    /// <param name="inheritedLocalTokens">The local token values inherited from the parent patch if applicable, to use in addition to the pre-existing tokens.</param>
    /// <param name="localTokens">The local token values defined directly on this patch, to use in addition to the pre-existing tokens.</param>
    /// <param name="contentPack">The content pack which requested the patch.</param>
    /// <param name="migrator">The aggregate migration which applies for this patch.</param>
    /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="parseAssetName">Parse an asset name.</param>
    /// <param name="tryParseFields">Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</param>
    public EditDataPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString? assetLocale, AssetEditPriority priority, IEnumerable<Condition> conditions, IManagedTokenString? fromFile, IEnumerable<EditDataPatchRecord>? records, IEnumerable<EditDataPatchField>? fields, IEnumerable<EditDataPatchMoveRecord>? moveRecords, IEnumerable<ITextOperation>? textOperations, IEnumerable<IManagedTokenString>? targetField, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, IMonitor monitor, Func<string, IAssetName> parseAssetName, TryParseFieldsDelegate tryParseFields)
        : base(
            indexPath: indexPath,
            path: path,
            type: PatchType.EditData,
            assetName: assetName,
            assetLocale: assetLocale,
            priority: (int)priority,
            updateRate: updateRate,
            inheritedLocalTokens: inheritedLocalTokens,
            localTokens: localTokens,
            conditions: conditions,
            contentPack: contentPack,
            migrator: migrator,
            parentPatch: parentPatch,
            parseAssetName: parseAssetName,
            fromAsset: fromFile
        )
    {
        // set fields
        this.Records = records?.ToArray() ?? [];
        this.Fields = fields?.ToArray() ?? [];
        this.MoveRecords = moveRecords?.ToArray() ?? [];
        this.TextOperations = textOperations?.ToArray() ?? [];
        this.TargetField = targetField?.ToArray() ?? [];
        this.Monitor = monitor;
        this.TryParseFields = tryParseFields;

        // track contextuals
        this.Contextuals
            .Add(this.Records)
            .Add(this.Fields)
            .Add(this.MoveRecords)
            .Add(this.TextOperations)
            .Add(this.TargetField)
            .Add(this.Conditions);
    }

    /// <inheritdoc />
    public override bool UpdateContext(IContext context)
    {
        // need to reload data for legacy FromFile
        if (this.HasFromAsset && (this.ManagedRawFromAsset.UpdateContext(context) || !this.HasEverLoadedFromFile))
        {
            this.HasEverLoadedFromFile = true;

            // reload non-data changes
            this.Contextuals
                .Remove(this.Records)
                .Remove(this.Fields)
                .Remove(this.MoveRecords);
            base.UpdateContext(context);

            // reload data
            this.Records = [];
            this.Fields = [];
            this.MoveRecords = [];
            if (this.IsReady)
            {
                if (this.TryLoadFile(this.RawFromAsset, context, out List<EditDataPatchRecord>? records, out List<EditDataPatchField>? fields, out List<EditDataPatchMoveRecord>? moveEntries, out string? error))
                {
                    this.Records = records.ToArray();
                    this.Fields = fields.ToArray();
                    this.MoveRecords = moveEntries.ToArray();
                }
                else
                    this.Monitor.Log($"Can't load \"{this.Path}\" fields from file '{this.RawFromAsset}': {error}.", LogLevel.Warn);
            }

            // update context
            this.Contextuals
                .Add(this.Records)
                .Add(this.Fields)
                .Add(this.MoveRecords)
                .UpdateContext(context);
            this.IsReady = this.IsReady && this.Contextuals.IsReady;
            this.TokensUsedCache = null;

            return this.MarkUpdated();
        }

        // no custom logic needed
        return base.UpdateContext(context);
    }

    /// <inheritdoc />
    public override void Edit<T>(IAssetData asset)
    {
        this.Edit<T>(asset, onWarning: null);
    }

    /// <inheritdoc cref="Edit{T}(IAssetData)" />
    /// <param name="asset" />
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    public void Edit<T>(IAssetData asset, Action<string, IMonitor>? onWarning)
    {
        // get editor
        if (!this.EditorFactory.TryGetEditorFor(asset.Data, out IKeyValueEditor? editor))
        {
            this.WarnForPatch(this.GetEditorNotCompatibleError("the target asset", asset.Data, entryExists: true), onWarning);
            return;
        }

        // apply target field
        if (this.TargetField.Any())
        {
            var path = new List<string>(this.TargetField.Length);

            foreach (IManagedTokenString fieldName in this.TargetField)
            {
                path.Add(fieldName.Value!);
                IKeyValueEditor parentEditor = editor;

                object key = parentEditor.ParseKey(fieldName.Value!);
                object? data = parentEditor.GetEntry(key);

                if (data is null)
                {
                    // if we're targeting a null list or dictionary, create it
                    Type? type = parentEditor.GetEntryType(key);
                    if (type?.IsGenericType is true)
                    {
                        Type genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(List<>) || genericType == typeof(Dictionary<,>))
                        {
                            object newData = Activator.CreateInstance(type!)!;
                            parentEditor.SetEntry(key, newData);
                            data = parentEditor.GetEntry(key);
                        }
                    }
                }

                if (!this.EditorFactory.TryGetEditorFor(data, out editor))
                {
                    this.WarnForPatch(this.GetEditorNotCompatibleError($"the field '{string.Join("' > '", path)}'", data, entryExists: parentEditor.HasEntry(key)), onWarning);
                    return;
                }
            }
        }

        // apply edits
        char fieldDelimiter = this.GetStringFieldDelimiter(asset);
        this.ApplyEdits(editor, fieldDelimiter, onWarning);
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetChangeLabels()
    {
        if (this.Records.Any(p => p.Value?.Value == null))
            yield return "deleted entries";

        if (this.Fields.Any() || this.Records.Any(p => p.Value?.Value != null))
            yield return "changed entries";

        if (this.MoveRecords.Any())
            yield return "reordered entries";

        if (this.TextOperations.Any())
            yield return "applied text operations";
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Create a JSON serializer which matches SMAPI's internal serializer settings.</summary>
    /// <exception cref="InvalidOperationException">SMAPI's internal toolkit instance could not be accessed.</exception>
    private static JsonSerializer CreateJsonSerializer()
    {
        // HACK: get SMAPI's internal JSON helper
        ModToolkit toolkit = typeof(SCore).GetField("Toolkit", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(SCore.Instance) as ModToolkit ?? throw new InvalidOperationException("Can't access SMAPI's toolkit instance.");
        return toolkit.JsonHelper.GetSerializer();
    }

    /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
    /// <param name="fromFile">The normalized asset key from which to load entries (if applicable), including tokens.</param>
    /// <param name="context">The tokens available for this content pack.</param>
    /// <param name="entries">The parsed data entry changes.</param>
    /// <param name="fields">The parsed data field changes.</param>
    /// <param name="moveEntries">The parsed move entry records.</param>
    /// <param name="error">The error message indicating why parsing failed, if applicable.</param>
    /// <returns>Returns whether parsing succeeded.</returns>
    private bool TryLoadFile(ITokenString fromFile, IContext context, [NotNullWhen(true)] out List<EditDataPatchRecord>? entries, [NotNullWhen(true)] out List<EditDataPatchField>? fields, [NotNullWhen(true)] out List<EditDataPatchMoveRecord>? moveEntries, [NotNullWhen(false)] out string? error)
    {
        if (fromFile is { IsMutable: true, IsReady: false })
        {
            error = $"the {nameof(fromFile)} contains tokens which aren't available yet"; // this shouldn't happen, since the patch should check before calling this method
            entries = null;
            fields = null;
            moveEntries = null;
            return false;
        }

        // validate path
        if (!this.ContentPack.HasFile(fromFile.Value!))
        {
            error = "that file doesn't exist in the content pack";
            entries = null;
            fields = null;
            moveEntries = null;
            return false;
        }

        // load JSON file
        PatchConfig? model;
        try
        {
            model = this.ContentPack.ReadJsonFile<PatchConfig>(fromFile.Value!);
            if (model == null)
            {
                error = "could not parse that file: file contains a null patch.";
                entries = null;
                fields = null;
                moveEntries = null;
                return false;
            }
        }
        catch (JsonException ex)
        {
            error = $"could not parse that file: {ex}";
            entries = null;
            fields = null;
            moveEntries = null;
            return false;
        }

        // parse fields
        // note: this is only used for the legacy FromFile field, so it shouldn't allow
        // features added in Content Patcher 1.18.0+ (like TargetField).
        return this.TryParseFields(context, model, out entries, out fields, out moveEntries, out _, out error);
    }

    /// <summary>Apply the patch to a data asset.</summary>
    /// <param name="editor">The asset editor to apply.</param>
    /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void ApplyEdits(IKeyValueEditor editor, char fieldDelimiter, Action<string, IMonitor>? onWarning)
    {
        this.ApplyRecords(editor, onWarning);
        this.ApplyFields(editor, fieldDelimiter, onWarning);
        this.ApplyTextOperations(editor, fieldDelimiter, onWarning);
        this.ApplyMoveEntries(editor, onWarning);
    }

    /// <summary>Apply entry overwrites to the data asset.</summary>
    /// <param name="editor">The asset editor to apply.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void ApplyRecords(IKeyValueEditor editor, Action<string, IMonitor>? onWarning)
    {
        int i = 0;
        foreach (EditDataPatchRecord record in this.Records)
        {
            i++;

            // get info
            object key = editor.ParseKey(record.Key.Value!);
            Type? targetType = editor.GetEntryType(key);
            JToken? fromValue = record.Value?.Value;
            JTokenType fromType = fromValue?.Type ?? JTokenType.Null;

            // validate
            if (!editor.CanAddEntries && !editor.HasEntry(key))
            {
                if (editor is ModelKeyValueEditor modelEditor)
                    this.WarnForRecord(i, $"this asset is a data model, which doesn't allow adding new entries. The entry '{record.Key.Value}' isn't defined in the model, must be one of: {string.Join(", ", modelEditor.FieldNames)}.", onWarning);
                else
                    this.WarnForRecord(i, $"this asset doesn't allow adding new entries, and the entry '{record.Key.Value}' isn't defined in the model.", onWarning);
                continue;
            }
            if (targetType is null)
            {
                // This shouldn't happen in practice per the remarks on IKeyValueEditor.GetEntryType.
                // If you're here because it did happen, sorry!
                this.WarnForRecord(i, $"this asset doesn't have a type for entry '{record.Key.Value}', so new entries can't be added.", onWarning);
                continue;
            }

            // remove entry if null
            if (fromValue is null)
            {
                editor.RemoveEntry(key);
                continue;
            }

            // convert value to expected type
            object fromObj;
            try
            {
                fromObj = fromValue.ToObject(targetType, EditDataPatch.JsonSerializer.Value)!;
            }
            catch (Exception ex)
            {
                this.WarnForRecord(i, $"failed converting {(fromType == JTokenType.Object ? "entry" : $"{fromType} value")} to the expected type '{targetType.FullName}': {ex.Message}.", onWarning);
                continue;
            }

            // set value
            try
            {
                editor.SetEntry(key, fromObj);
            }
            catch (Exception ex)
            {
                this.WarnForRecord(i, $"failed setting '{key}' entry: {ex.Message}", onWarning);
                continue;
            }
        }
    }

    /// <summary>Apply field overwrites to the data asset.</summary>
    /// <param name="editor">The asset editor to apply.</param>
    /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void ApplyFields(IKeyValueEditor editor, char fieldDelimiter, Action<string, IMonitor>? onWarning)
    {
        foreach (IGrouping<string, EditDataPatchField> recordGroup in this.Fields.GroupByIgnoreCase(p => p.EntryKey.Value!))
        {
            // get entry info
            object key = editor.ParseKey(recordGroup.Key);
            Type? valueType = editor.GetEntryType(key);

            // skip if doesn't exist
            if (!editor.HasEntry(key))
            {
                this.WarnForField($"there's no record matching key '{key}' under {nameof(PatchConfig.Fields)}.", onWarning);
                continue;
            }

            // apply string
            if (valueType == typeof(string))
            {
                string[] actualFields = ((string)editor.GetEntry(key)!).Split(fieldDelimiter);
                foreach (EditDataPatchField field in recordGroup)
                {
                    if (!int.TryParse(field.FieldKey.Value, out int index))
                    {
                        this.WarnForField($"record '{key}' under {nameof(PatchConfig.Fields)} is a string, so it requires a field index between 0 and {actualFields.Length - 1} (received \"{field.FieldKey}\" instead)).", onWarning);
                        continue;
                    }
                    if (index < 0 || index > actualFields.Length - 1)
                    {
                        this.WarnForField($"record '{key}' under {nameof(PatchConfig.Fields)} has no field with index {index} (must be 0 to {actualFields.Length - 1}).", onWarning);
                        continue;
                    }

                    actualFields[index] = field.Value?.Value.Value<string?>() ?? string.Empty;
                }

                editor.SetEntry(key, string.Join(fieldDelimiter.ToString(), actualFields));
            }

            // apply object
            else
            {
                JObject obj = new();
                foreach (EditDataPatchField field in recordGroup)
                    obj[field.FieldKey.Value!] = field.Value?.Value;
                using JsonReader reader = obj.CreateReader();
                EditDataPatch.JsonSerializer.Value.Populate(reader, editor.GetEntry(key)!);
            }
        }
    }

    /// <summary>Apply text operations to the data asset.</summary>
    /// <param name="editor">The asset editor to apply.</param>
    /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void ApplyTextOperations(IKeyValueEditor editor, char fieldDelimiter, Action<string, IMonitor>? onWarning)
    {
        for (int i = 0; i < this.TextOperations.Length; i++)
        {
            if (!this.TryApplyTextOperation(this.TextOperations[i], editor, fieldDelimiter, out string? error))
            {
                string warning = $"Can't apply data patch \"{this.Path} > text operation #{i}\" to {this.TargetAsset}: {error}";
                onWarning?.Invoke(warning, this.Monitor);
                this.Monitor.Log(warning, LogLevel.Warn);
            }
        }
    }

    /// <summary>Apply entry moves to the data asset.</summary>
    /// <param name="editor">The asset editor to apply.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void ApplyMoveEntries(IKeyValueEditor editor, Action<string, IMonitor>? onWarning)
    {
        if (!this.MoveRecords.Any())
            return;

        if (!editor.CanMoveEntries)
        {
            string warning = $"Can't move records for \"{this.Path}\" > {nameof(PatchConfig.MoveEntries)}: target asset '{this.TargetAsset}' isn't an ordered list.";
            onWarning?.Invoke(warning, this.Monitor);
            this.Monitor.LogOnce(warning, LogLevel.Warn);
            return;
        }

        foreach (EditDataPatchMoveRecord moveRecord in this.MoveRecords)
        {
            if (!moveRecord.IsReady)
                continue;

            object key = editor.ParseKey(moveRecord.ID.Value!);
            string errorLabel = $"record \"{this.Path}\" > {nameof(PatchConfig.MoveEntries)} > \"{moveRecord.ID}\"";

            // move record
            MoveResult result = MoveResult.Success;
            object? anchorKey = null;
            if (moveRecord.ToPosition is MoveEntryPosition.Top or MoveEntryPosition.Bottom)
            {
                anchorKey = moveRecord.ToPosition; // should never be used, but provides a more useful error if it ever happens
                result = editor.MoveEntry(key, moveRecord.ToPosition);
            }
            else if (moveRecord.AfterID.IsMeaningful() || moveRecord.BeforeID.IsMeaningful())
            {
                // get config
                bool isAfter = moveRecord.AfterID.IsMeaningful();
                string rawAnchorKey = (isAfter ? moveRecord.AfterID!.Value : moveRecord.BeforeID!.Value)!;
                anchorKey = editor.ParseKey(rawAnchorKey);

                // move entry
                errorLabel += $" {(isAfter ? nameof(PatchMoveEntryConfig.AfterID) : nameof(PatchMoveEntryConfig.BeforeID))} \"{rawAnchorKey}\"";
                result = editor.MoveEntry(key, anchorKey, isAfter);
            }

            // log error
            if (result != MoveResult.Success)
            {
                string logMessage;
                bool isWarning = true;

                switch (result)
                {
                    case MoveResult.TargetNotFound:
                        logMessage = $"Can't move {errorLabel}: no entry with ID '{key}' exists.";
                        isWarning = false;
                        break;

                    case MoveResult.AnchorNotFound:
                        logMessage = $"Can't move {errorLabel}: no entry with ID '{anchorKey}' exists.";
                        isWarning = false;
                        break;

                    case MoveResult.AnchorIsMain:
                        logMessage = $"Can't move {errorLabel}: can't move entry relative to itself.";
                        break;

                    default:
                        logMessage = $"Can't move {errorLabel}: an unknown error occurred.";
                        break;
                }

                onWarning?.Invoke(logMessage, this.Monitor);
                this.Monitor.LogOnce(logMessage, isWarning ? LogLevel.Warn : LogLevel.Trace);
            }
        }
    }

    /// <summary>Try to apply a text operation.</summary>
    /// <param name="operation">The text operation to apply.</param>
    /// <param name="editor">The asset editor to apply.</param>
    /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
    /// <param name="error">An error indicating why applying the operation failed, if applicable.</param>
    /// <returns>Returns whether applying the operation succeeded.</returns>
    private bool TryApplyTextOperation(ITextOperation operation, IKeyValueEditor editor, char fieldDelimiter, [NotNullWhen(false)] out string? error)
    {
        var targetRoot = operation.GetTargetRoot();
        switch (targetRoot)
        {
            case TextOperationTargetRoot.Entries:
                {
                    // validate format
                    if (operation.Target.Length != 2)
                        return this.Fail($"an '{TextOperationTargetRoot.Entries}' path must have exactly one other segment: the entry key.", out error);

                    // get entry
                    object key = editor.ParseKey(operation.Target[1].Value!);
                    Type? entryType = editor.GetEntryType(key);
                    if (entryType != typeof(string))
                        return this.Fail($"can't apply text operation to the '{operation.Target[1].Value}' entry because it's not a string value.", out error);
                    string? value = (string?)editor.GetEntry(key);

                    // set value
                    editor.SetEntry(key, operation.Apply(value));
                }
                break;

            case TextOperationTargetRoot.Fields:
                {
                    // validate format
                    if (operation.Target.Length != 3)
                        return this.Fail($"a '{TextOperationTargetRoot.Fields}' path must have exactly two other segments: one for the entry key, and one for the field key or index.", out error);

                    // get entry editor
                    string rawEntryKey = operation.Target[1].Value!;
                    string rawFieldKey = operation.Target[2].Value!;
                    IKeyValueEditor? entryEditor;
                    {
                        object key = editor.ParseKey(rawEntryKey);
                        Type? entryType = editor.GetEntryType(key);
                        object? entry = editor.GetEntry(key);
                        if (entry is null)
                            return this.Fail($"record '{rawEntryKey}' has no value, so field '{rawFieldKey}' can't be modified using text operations.", out error);

                        // get entry editor
                        if (!this.EditorFactory.TryGetEditorFor(entry, out entryEditor))
                        {
                            if (entryType == typeof(string))
                                entryEditor = new DelimitedStringKeyValueEditor(editor, key, fieldDelimiter);
                            else
                                return this.Fail($"record '{rawEntryKey}' > field '{rawFieldKey}' can't be modified using text operations because its type ({entryType?.FullName ?? "<unknown>"}) isn't supported.", out error);
                        }
                    }

                    // get field
                    object fieldKey = entryEditor.ParseKey(rawFieldKey);
                    Type? fieldType = entryEditor.GetEntryType(fieldKey);
                    object? fieldValue = entryEditor.GetEntry(fieldKey);

                    // validate type
                    if (fieldType != typeof(string))
                        return this.Fail($"field '{rawEntryKey}' > '{rawFieldKey}' has type '{fieldType}', but you can only apply text operations to a text field.", out error);

                    // edit value
                    entryEditor.SetEntry(
                        fieldKey,
                        operation.Apply(fieldValue as string ?? "")
                    );
                }
                break;

            default:
                return this.Fail(
                    targetRoot == null
                        ? $"unknown path root '{operation.Target[0]}'."
                        : $"path root '{targetRoot}' isn't valid for an {nameof(PatchType.EditMap)} patch",
                    out error
                );
        }

        error = null;
        return true;
    }

    /// <summary>Get the delimiter used in string entries for an asset.</summary>
    /// <param name="asset">The asset being edited.</param>
    private char GetStringFieldDelimiter(IAssetInfo asset)
    {
        return asset.Name.IsEquivalentTo("Data/Achievements", useBaseName: true)
            ? '^'
            : '/';
    }

    /// <summary>Log a warning for an issue when applying the patch.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void WarnForPatch(string message, Action<string, IMonitor>? onWarning)
    {
        string warning = $"Can't apply data patch \"{this.Path}\" to {this.TargetAsset}: {message}";
        onWarning?.Invoke(warning, this.Monitor);
        this.Monitor.Log(warning, LogLevel.Warn);
    }

    /// <summary>Log a warning for an issue when applying a field edit.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void WarnForField(string message, Action<string, IMonitor>? onWarning)
    {
        string warning = $"Can't apply data patch \"{this.Path}\" to {this.TargetAsset}: {message}";
        onWarning?.Invoke(warning, this.Monitor);
        this.Monitor.Log(warning, LogLevel.Warn);
    }

    /// <summary>Log a warning for an issue when applying a record.</summary>
    /// <param name="index">The index of the record in the <see cref="Records"/> list.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
    private void WarnForRecord(int index, string message, Action<string, IMonitor>? onWarning)
    {
        string warning = $"Can't apply data patch \"{this.Path} > entry #{index}\" to {this.TargetAsset}: {message}";
        onWarning?.Invoke(warning, this.Monitor);
        this.Monitor.Log(warning, LogLevel.Warn);
    }


    /// <summary>If an editor can't be constructed for a given data structure, get a human-readable error indicating why.</summary>
    /// <param name="nounPhrase">A noun phase which describes the field being accessed.</param>
    /// <param name="data">The data for which an editor couldn't be constructed.</param>
    /// <param name="entryExists">Whether the entry exists in the asset.</param>
    private string GetEditorNotCompatibleError(string nounPhrase, object? data, bool entryExists)
    {
        if (!entryExists)
            return $"{nounPhrase} doesn't match an existing target";

        if (data is null)
            return $"{nounPhrase} is null and can't be targeted for edits";

        Type type = data.GetType();

        if (typeof(Texture2D).IsAssignableFrom(type))
            return $"{nounPhrase} is an image, not data";
        if (typeof(Map).IsAssignableFrom(type))
            return $"{nounPhrase} is a map, not data";
        return $"{nounPhrase} has type '{type.FullName}', which isn't recognized by Content Patcher";
    }
}
