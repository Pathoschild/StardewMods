using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Patches.EditData;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using xTile;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for data to edit into a data file.</summary>
    internal class EditDataPatch : Patch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Constructs key/value editors for arbitrary data.</summary>
        private readonly KeyValueEditorFactory EditorFactory;

        /// <summary>The field within the data asset to which edits should be applied, or empty to apply to the root asset.</summary>
        private readonly IManagedTokenString[] TargetField;

        /// <summary>The data records to edit.</summary>
        private EditDataPatchRecord[] Records;

        /// <summary>The data fields to edit.</summary>
        private EditDataPatchField[] Fields;

        /// <summary>The records to reorder, if the target is a list asset.</summary>
        private EditDataPatchMoveRecord[] MoveRecords;

        /// <summary>The text operations to apply to existing values.</summary>
        private readonly TextOperation[] TextOperations;

        /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
        private readonly TryParseFieldsDelegate TryParseFields;

        /// <summary>Whether the patch already tried loading the <see cref="Patch.FromAsset"/> asset for the current context. This doesn't necessarily means it succeeded (e.g. the file may not have existed).</summary>
        private bool AttemptedDataLoad;

        /// <summary>The cached JSON serializer used to apply JSON structures to a model.</summary>
        private readonly Lazy<JsonSerializer> Serializer = new(() => new()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        });


        /*********
        ** Accessors
        *********/
        /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
        /// <param name="context">The tokens available for this content pack.</param>
        /// <param name="entry">The change to load.</param>
        /// <param name="entries">The parsed data entry changes.</param>
        /// <param name="fields">The parsed data field changes.</param>
        /// <param name="moveEntries">The parsed move entry records.</param>
        /// <param name="targetFields">The field within the data asset to which edits should be applied, or empty to apply to the root asset.</param>
        /// <param name="error">The error message indicating why parsing failed, if applicable.</param>
        /// <returns>Returns whether parsing succeeded.</returns>
        public delegate bool TryParseFieldsDelegate(IContext context, PatchConfig entry, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out List<IManagedTokenString> targetFields, out string error);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromFile">The normalized asset key from which to load entries (if applicable), including tokens.</param>
        /// <param name="records">The data records to edit.</param>
        /// <param name="fields">The data fields to edit.</param>
        /// <param name="moveRecords">The records to reorder, if the target is a list asset.</param>
        /// <param name="textOperations">The text operations to apply to existing values.</param>
        /// <param name="targetField">The field within the data asset to which edits should be applied, or empty to apply to the root asset.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies dynamic access to game code.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        /// <param name="tryParseFields">Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</param>
        public EditDataPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromFile, IEnumerable<EditDataPatchRecord> records, IEnumerable<EditDataPatchField> fields, IEnumerable<EditDataPatchMoveRecord> moveRecords, IEnumerable<TextOperation> textOperations, IEnumerable<IManagedTokenString> targetField, UpdateRate updateRate, IContentPack contentPack, IPatch parentPatch, IMonitor monitor, IReflectionHelper reflection, Func<string, string> normalizeAssetName, TryParseFieldsDelegate tryParseFields)
            : base(
                indexPath: indexPath,
                path: path,
                type: PatchType.EditData,
                assetName: assetName,
                conditions: conditions,
                updateRate: updateRate,
                contentPack: contentPack,
                parentPatch: parentPatch,
                normalizeAssetName: normalizeAssetName,
                fromAsset: fromFile
            )
        {
            // set fields
            this.Records = records?.ToArray();
            this.Fields = fields?.ToArray();
            this.MoveRecords = moveRecords?.ToArray();
            this.TextOperations = textOperations?.ToArray() ?? Array.Empty<TextOperation>();
            this.TargetField = targetField?.ToArray() ?? Array.Empty<IManagedTokenString>();
            this.Monitor = monitor;
            this.EditorFactory = new(reflection);
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
            // skip: don't need to handle a data file
            if (this.RawFromAsset == null)
                return base.UpdateContext(context);

            // skip: file already loaded and target didn't change
            if (!this.ManagedRawTargetAsset.UpdateContext(context) && this.AttemptedDataLoad)
                return base.UpdateContext(context);

            // reload non-data changes
            this.Contextuals
                .Remove(this.Records)
                .Remove(this.Fields)
                .Remove(this.MoveRecords);
            base.UpdateContext(context);

            // reload data
            this.Records = Array.Empty<EditDataPatchRecord>();
            this.Fields = Array.Empty<EditDataPatchField>();
            this.MoveRecords = Array.Empty<EditDataPatchMoveRecord>();
            if (this.IsReady)
            {
                if (this.TryLoadFile(this.RawFromAsset, context, out List<EditDataPatchRecord> records, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out string error))
                {
                    this.Records = records.ToArray();
                    this.Fields = fields.ToArray();
                    this.MoveRecords = moveEntries.ToArray();
                }
                else
                    this.Monitor.Log($"Can't load \"{this.Path}\" fields from file '{this.RawFromAsset}': {error}.", LogLevel.Warn);

                this.AttemptedDataLoad = true;
            }

            // update context
            this.Contextuals
                .Add(this.Records)
                .Add(this.Fields)
                .Add(this.MoveRecords)
                .UpdateContext(context);
            this.IsReady = this.IsReady && this.Contextuals.IsReady;

            return this.MarkUpdated();
        }

        /// <inheritdoc />
        public override void Edit<T>(IAssetData asset)
        {
            string errorPrefix = $"Can't apply data patch \"{this.Path}\" to {this.TargetAsset}";

            // get editor
            IKeyValueEditor editor = this.EditorFactory.GetEditorFor(asset.Data);
            if (editor is null)
            {
                this.Monitor.Log($"{errorPrefix}: {this.GetEditorNotCompatibleError("the target asset", asset.Data, entryExists: true)}", LogLevel.Warn);
                return;
            }

            // apply target field
            if (this.TargetField.Any())
            {
                var path = new List<string>(this.TargetField.Length);

                foreach (IManagedTokenString fieldName in this.TargetField)
                {
                    path.Add(fieldName.Value);
                    IKeyValueEditor parentEditor = editor;

                    object key = parentEditor.ParseKey(fieldName.Value);
                    object data = parentEditor.GetEntry(key);

                    editor = this.EditorFactory.GetEditorFor(data);
                    if (editor is null)
                    {
                        this.Monitor.Log($"{errorPrefix}: {this.GetEditorNotCompatibleError($"the field '{string.Join("' > '", path)}'", data, entryExists: parentEditor.HasEntry(key))}", LogLevel.Warn);
                        return;
                    }
                }
            }

            // apply edits
            char fieldDelimiter = this.GetStringFieldDelimiter(asset);
            this.ApplyEdits(editor, fieldDelimiter);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            if (this.Records?.Any(p => p.Value?.Value == null) == true)
                yield return "deleted entries";

            if (this.Fields?.Any() == true || this.Records?.Any(p => p.Value?.Value != null) == true)
                yield return "changed entries";

            if (this.MoveRecords?.Any() == true)
                yield return "reordered entries";

            if (this.TextOperations.Any())
                yield return "applied text operations";
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
        /// <param name="fromFile">The normalized asset key from which to load entries (if applicable), including tokens.</param>
        /// <param name="context">The tokens available for this content pack.</param>
        /// <param name="entries">The parsed data entry changes.</param>
        /// <param name="fields">The parsed data field changes.</param>
        /// <param name="moveEntries">The parsed move entry records.</param>
        /// <param name="error">The error message indicating why parsing failed, if applicable.</param>
        /// <returns>Returns whether parsing succeeded.</returns>
        private bool TryLoadFile(ITokenString fromFile, IContext context, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out string error)
        {
            if (fromFile.IsMutable && !fromFile.IsReady)
            {
                error = $"the {nameof(fromFile)} contains tokens which aren't available yet"; // this shouldn't happen, since the patch should check before calling this method
                entries = null;
                fields = null;
                moveEntries = null;
                return false;
            }

            // validate path
            if (!this.ContentPack.HasFile(fromFile.Value))
            {
                error = "that file doesn't exist in the content pack";
                entries = null;
                fields = null;
                moveEntries = null;
                return false;
            }

            // load JSON file
            PatchConfig model;
            try
            {
                model = this.ContentPack.ReadJsonFile<PatchConfig>(fromFile.Value);
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
        private void ApplyEdits(IKeyValueEditor editor, char fieldDelimiter)
        {
            this.ApplyRecords(editor);
            this.ApplyFields(editor, fieldDelimiter);
            this.ApplyTextOperations(editor, fieldDelimiter);
            this.ApplyMoveEntries(editor);
        }

        /// <summary>Apply entry overwrites to the data asset.</summary>
        /// <param name="editor">The asset editor to apply.</param>
        private void ApplyRecords(IKeyValueEditor editor)
        {
            if (this.Records == null)
                return;

            int i = 0;
            foreach (EditDataPatchRecord record in this.Records)
            {
                string errorPrefix = $"Can't apply data patch \"{this.Path} > entry #{i}\" to {this.TargetAsset}";
                i++;

                // get entry info
                object key = editor.ParseKey(record.Key.Value);
                Type valueType = editor.GetEntryType(key);

                // validate
                if (!editor.CanAddEntries && !editor.HasEntry(key))
                    this.Monitor.Log($"{errorPrefix}: this asset is a data model, which doesn't allow adding new entries. The entry '{record.Key.Value}' isn't defined in the model.", LogLevel.Warn);

                // apply string
                else if (valueType == typeof(string))
                {
                    if (record.Value?.Value == null)
                        editor.RemoveEntry(key);
                    else if (record.Value.Value is JValue field)
                        editor.SetEntry(key, field);
                    else
                        this.Monitor.Log($"{errorPrefix}: this asset has string values (but {record.Value.Value.Type} values were provided).", LogLevel.Warn);
                }

                // apply object
                else
                {
                    if (record.Value?.Value == null)
                        editor.RemoveEntry(key);
                    else if (record.Value.Value is JObject field)
                        editor.SetEntry(key, field);
                    else
                        this.Monitor.Log($"{errorPrefix}: this asset has {valueType} values (but {record.Value.Value.Type} values were provided).", LogLevel.Warn);
                }
            }
        }

        /// <summary>Apply field overwrites to the data asset.</summary>
        /// <param name="editor">The asset editor to apply.</param>
        /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
        private void ApplyFields(IKeyValueEditor editor, char fieldDelimiter)
        {
            if (this.Fields == null)
                return;

            foreach (IGrouping<string, EditDataPatchField> recordGroup in this.Fields.GroupByIgnoreCase(p => p.EntryKey.Value))
            {
                string errorPrefix = $"Can't apply data patch \"{this.Path}\" to {this.TargetAsset}";

                // get entry info
                object key = editor.ParseKey(recordGroup.Key);
                Type valueType = editor.GetEntryType(key);

                // skip if doesn't exist
                if (!editor.HasEntry(key))
                {
                    this.Monitor.Log($"{errorPrefix}: there's no record matching key '{key}' under {nameof(PatchConfig.Fields)}.", LogLevel.Warn);
                    continue;
                }

                // apply string
                if (valueType == typeof(string))
                {
                    string[] actualFields = ((string)editor.GetEntry(key)).Split(fieldDelimiter);
                    foreach (EditDataPatchField field in recordGroup)
                    {
                        if (!int.TryParse(field.FieldKey.Value, out int index))
                        {
                            this.Monitor.Log($"{errorPrefix}: record '{key}' under {nameof(PatchConfig.Fields)} is a string, so it requires a field index between 0 and {actualFields.Length - 1} (received \"{field.FieldKey}\" instead)).", LogLevel.Warn);
                            continue;
                        }
                        if (index < 0 || index > actualFields.Length - 1)
                        {
                            this.Monitor.Log($"{errorPrefix}: record '{key}' under {nameof(PatchConfig.Fields)} has no field with index {index} (must be 0 to {actualFields.Length - 1}).", LogLevel.Warn);
                            continue;
                        }

                        actualFields[index] = field.Value.Value.Value<string>();
                    }

                    editor.SetEntry(key, string.Join(fieldDelimiter.ToString(), actualFields));
                }

                // apply object
                else
                {
                    JObject obj = new();
                    foreach (EditDataPatchField field in recordGroup)
                        obj[field.FieldKey.Value] = field.Value.Value;
                    using JsonReader reader = obj.CreateReader();
                    this.Serializer.Value.Populate(reader, editor.GetEntry(key));
                }
            }
        }

        /// <summary>Apply text operations to the data asset.</summary>
        /// <param name="editor">The asset editor to apply.</param>
        /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
        private void ApplyTextOperations(IKeyValueEditor editor, char fieldDelimiter)
        {
            for (int i = 0; i < this.TextOperations.Length; i++)
            {
                if (!this.TryApplyTextOperation(this.TextOperations[i], editor, fieldDelimiter, out string error))
                    this.Monitor.Log($"Can't data patch \"{this.Path} > text operation #{i}\" to {this.TargetAsset}: {error}", LogLevel.Warn);
            }
        }

        /// <summary>Apply entry moves to the data asset.</summary>
        /// <param name="editor">The asset editor to apply.</param>
        private void ApplyMoveEntries(IKeyValueEditor editor)
        {
            if (!this.MoveRecords.Any())
                return;

            if (!editor.CanMoveEntries)
            {
                this.Monitor.LogOnce($"Can't move records for \"{this.Path}\" > {nameof(PatchConfig.MoveEntries)}: target asset '{this.TargetAsset}' isn't an ordered list.", LogLevel.Warn);
                return;
            }

            foreach (EditDataPatchMoveRecord moveRecord in this.MoveRecords)
            {
                if (!moveRecord.IsReady)
                    continue;

                object key = editor.ParseKey(moveRecord.ID.Value);
                string errorLabel = $"record \"{this.Path}\" > {nameof(PatchConfig.MoveEntries)} > \"{moveRecord.ID}\"";

                // move record
                MoveResult result = MoveResult.Success;
                if (moveRecord.ToPosition is MoveEntryPosition.Top or MoveEntryPosition.Bottom)
                    result = editor.MoveEntry(key, moveRecord.ToPosition);
                else if (moveRecord.AfterID.IsMeaningful() || moveRecord.BeforeID.IsMeaningful())
                {
                    // get config
                    bool isAfter = moveRecord.AfterID.IsMeaningful();
                    string rawAnchorKey = isAfter ? moveRecord.AfterID.Value : moveRecord.BeforeID.Value;
                    object anchorKey = editor.ParseKey(rawAnchorKey);

                    // move entry
                    errorLabel += $" {(isAfter ? nameof(PatchMoveEntryConfig.AfterID) : nameof(PatchMoveEntryConfig.BeforeID))} \"{rawAnchorKey}\"";
                    result = editor.MoveEntry(key, anchorKey, isAfter);
                }

                // log error
                if (result != MoveResult.Success)
                {
                    switch (result)
                    {
                        case MoveResult.TargetNotFound:
                            this.Monitor.LogOnce($"Can't move {errorLabel}: no entry with that ID exists.");
                            break;

                        case MoveResult.AnchorNotFound:
                            this.Monitor.LogOnce($"Can't move {errorLabel}: no entry with the relative ID exists.");
                            break;

                        case MoveResult.AnchorIsMain:
                            this.Monitor.LogOnce($"Can't move {errorLabel}: can't move entry relative to itself.");
                            break;

                        default:
                            this.Monitor.LogOnce($"Can't move {errorLabel}: an unknown error occurred.");
                            break;
                    }
                }
            }
        }

        /// <summary>Try to apply a text operation.</summary>
        /// <param name="operation">The text operation to apply.</param>
        /// <param name="editor">The asset editor to apply.</param>
        /// <param name="fieldDelimiter">The field delimiter for the data asset's string values, if applicable.</param>
        /// <param name="error">An error indicating why applying the operation failed, if applicable.</param>
        /// <returns>Returns whether applying the operation succeeded.</returns>
        private bool TryApplyTextOperation(TextOperation operation, IKeyValueEditor editor, char fieldDelimiter, out string error)
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
                        object key = editor.ParseKey(operation.Target[1].Value);
                        Type entryType = editor.GetEntryType(key);
                        if (entryType != typeof(string))
                            return this.Fail($"can't apply text operation to the '{operation.Target[1].Value}' entry because it's not a string value.", out error);
                        string value = (string)editor.GetEntry(key);

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
                        string rawEntryKey = operation.Target[1].Value;
                        string rawFieldKey = operation.Target[2].Value;
                        IKeyValueEditor entryEditor;
                        {
                            object key = editor.ParseKey(rawEntryKey);
                            Type entryType = editor.GetEntryType(key);
                            object entry = editor.GetEntry(key);
                            if (entry is null)
                                return this.Fail($"record '{rawEntryKey}' has no value, so field '{rawFieldKey}' can't be modified using text operations.", out error);

                            // get entry editor
                            entryEditor = this.EditorFactory.GetEditorFor(entry);
                            if (entryEditor is null)
                                return this.Fail($"record '{rawEntryKey}' > field '{rawFieldKey}' can't be modified using text operations because its type ({entryType.FullName}) isn't supported.", out error);
                        }

                        // get field
                        object fieldKey = entryEditor.ParseKey(rawFieldKey);
                        Type fieldType = entryEditor.GetEntryType(fieldKey);
                        object fieldValue = entryEditor.GetEntry(fieldKey);

                        // edit field value
                        if (fieldType == typeof(string))
                        {
                            // read fields
                            string[] actualFields = ((string)fieldValue).Split(fieldDelimiter);

                            // validate key
                            if (!int.TryParse(rawFieldKey, out int fieldIndex))
                                return this.Fail($"record '{rawEntryKey}' needs a field index between 0 and {actualFields.Length - 1} (received \"{rawFieldKey}\" instead)).", out error);
                            if (fieldIndex < 0 || fieldIndex > actualFields.Length - 1)
                                return this.Fail($"record '{rawEntryKey}' has no field with index {fieldIndex} (must be 0 to {actualFields.Length - 1}).", out error);

                            // apply change
                            actualFields[fieldIndex] = operation.Apply(actualFields[fieldIndex]);
                            entryEditor.SetEntry(fieldKey, string.Join(fieldDelimiter.ToString(), actualFields));
                        }
                        else
                        {
                            // apply change
                            if (fieldValue is null)
                                entryEditor.SetEntry(fieldKey, operation.Apply(""));
                            else if (fieldValue is string fieldStr)
                                entryEditor.SetEntry(fieldKey, operation.Apply(fieldStr));
                            else
                                return this.Fail($"field '{rawEntryKey}' > '{rawFieldKey}' has type '{fieldType}', but you can only apply text operations to a text field.", out error);
                        }
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
            return asset.AssetNameEquals("Data/Achievements")
                ? '^'
                : '/';
        }

        /// <summary>If an editor can't be constructed for a given data structure, get a human-readable error indicating why.</summary>
        /// <param name="nounPhrase">A noun phase </param>
        /// <param name="data">The data for which an editor couldn't be constructed.</param>
        /// <param name="entryExists">Whether the entry exists in the asset.</param>
        private string GetEditorNotCompatibleError(string nounPhrase, object data, bool entryExists)
        {
            if (!entryExists || data is null)
                return $"{nounPhrase} is null and can't be targeted for edits";

            Type type = data.GetType();

            if (typeof(Texture2D).IsAssignableFrom(type))
                return $"{nounPhrase} is an image, not data";
            if (typeof(Map).IsAssignableFrom(type))
                return $"{nounPhrase} is a map, not data";
            return $"{nounPhrase} has type '{type.FullName}', which isn't recognized by Content Patcher";
        }
    }
}
