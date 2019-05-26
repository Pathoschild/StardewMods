using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using xTile;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for a data to edit into a data file.</summary>
    internal class EditDataPatch : Patch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The data records to edit.</summary>
        private readonly EditDataPatchRecord[] Records;

        /// <summary>The data fields to edit.</summary>
        private readonly EditDataPatchField[] Fields;

        /// <summary>The records to reorder, if the target is a list asset.</summary>
        private readonly EditDataPatchMoveRecord[] MoveRecords;

        /// <summary>A list of warning messages which have been previously logged.</summary>
        private readonly HashSet<string> LoggedWarnings = new HashSet<string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="records">The data records to edit.</param>
        /// <param name="fields">The data fields to edit.</param>
        /// <param name="moveRecords">The records to reorder, if the target is a list asset.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="normaliseAssetName">Normalise an asset name.</param>
        public EditDataPatch(string logName, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, IEnumerable<EditDataPatchRecord> records, IEnumerable<EditDataPatchField> fields, IEnumerable<EditDataPatchMoveRecord> moveRecords, IMonitor monitor, Func<string, string> normaliseAssetName)
            : base(logName, PatchType.EditData, contentPack, assetName, conditions, normaliseAssetName)
        {
            // set fields
            this.Records = records.ToArray();
            this.Fields = fields.ToArray();
            this.MoveRecords = moveRecords.ToArray();
            this.Monitor = monitor;

            // track contextuals
            this.Contextuals
                .Add(this.Records)
                .Add(this.Fields)
                .Add(this.MoveRecords)
                .Add(this.Conditions);
        }

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        /// <exception cref="NotSupportedException">The asset data can't be parsed or edited.</exception>
        public override void Edit<T>(IAssetData asset)
        {
            // throw on invalid type
            if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Map))
            {
                this.Monitor.Log($"Can't apply data patch \"{this.LogName}\" to {this.TargetAsset}: this file isn't a data file (found {(typeof(T) == typeof(Texture2D) ? "image" : typeof(T).Name)}).", LogLevel.Warn);
                return;
            }

            // handle dictionary types
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // get dictionary's key/value types
                Type[] genericArgs = typeof(T).GetGenericArguments();
                if (genericArgs.Length != 2)
                    throw new InvalidOperationException("Can't parse the asset's dictionary key/value types.");
                Type keyType = typeof(T).GetGenericArguments().FirstOrDefault();
                Type valueType = typeof(T).GetGenericArguments().LastOrDefault();
                if (keyType == null)
                    throw new InvalidOperationException("Can't parse the asset's dictionary key type.");
                if (valueType == null)
                    throw new InvalidOperationException("Can't parse the asset's dictionary value type.");

                // get underlying apply method
                MethodInfo method = this.GetType().GetMethod(nameof(this.ApplyDictionary), BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                    throw new InvalidOperationException($"Can't fetch the internal {nameof(this.ApplyDictionary)} method.");

                // invoke method
                method
                    .MakeGenericMethod(keyType, valueType)
                    .Invoke(this, new object[] { asset });
            }

            // handle list types
            else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                // get list's value type
                Type keyType = typeof(T).GetGenericArguments().FirstOrDefault();
                if (keyType == null)
                    throw new InvalidOperationException("Can't parse the asset's list value type.");

                // get underlying apply method
                MethodInfo method = this.GetType().GetMethod(nameof(this.ApplyList), BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                    throw new InvalidOperationException($"Can't fetch the internal {nameof(this.ApplyList)} method.");

                // invoke method
                method
                    .MakeGenericMethod(keyType)
                    .Invoke(this, new object[] { asset });
            }

            // unknown type
            else
                throw new NotSupportedException($"Unknown data asset type {typeof(T).FullName}, expected dictionary or list.");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Apply the patch to a dictionary asset.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        private void ApplyDictionary<TKey, TValue>(IAssetData asset)
        {
            // get data
            IDictionary<TKey, TValue> data = asset.AsDictionary<TKey, TValue>().Data;

            // apply field/record edits
            this.ApplyCollection<TKey, TValue>(
                hasEntry: key => data.ContainsKey(key),
                getEntry: key => data[key],
                setEntry: (key, value) => data[key] = value,
                removeEntry: key => data.Remove(key)
            );

            // apply moves
            if (this.MoveRecords.Any())
                this.LogOnce($"Can't move records for \"{this.LogName}\" > {nameof(PatchConfig.MoveEntries)}: target asset '{this.TargetAsset}' isn't an ordered list).", LogLevel.Warn);
        }

        /// <summary>Apply the patch to a list asset.</summary>
        /// <typeparam name="TValue">The list value type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        private void ApplyList<TValue>(IAssetData asset)
        {
            // get data
            IList<TValue> data = asset.GetData<List<TValue>>();
            TValue GetByKey(string key) => data.FirstOrDefault(p => this.GetKey(p) == key);

            // apply field/record edits
            this.ApplyCollection<string, TValue>(
                hasEntry: key => GetByKey(key) != null,
                getEntry: key => GetByKey(key),
                setEntry: (key, value) =>
                {
                    TValue match = GetByKey(key);
                    if (match != null)
                    {
                        int index = data.IndexOf(match);
                        data.RemoveAt(index);
                        data.Insert(index, value);
                    }
                    else
                        data.Add(value);
                },
                removeEntry: key =>
                {
                    TValue match = GetByKey(key);
                    if (match != null)
                    {
                        int index = data.IndexOf(match);
                        data.RemoveAt(index);
                    }
                }
            );

            // apply moves
            foreach (EditDataPatchMoveRecord moveRecord in this.MoveRecords)
            {
                if (!moveRecord.IsReady)
                    continue;
                string errorLabel = $"record \"{this.LogName}\" > {nameof(PatchConfig.MoveEntries)} > \"{moveRecord.ID.Value}\"";

                // get entry
                TValue entry = GetByKey(moveRecord.ID.Value);
                if (entry == null)
                {
                    this.LogOnce($"Can't move {errorLabel}: no entry with that ID exists.", LogLevel.Warn);
                    continue;
                }
                int fromIndex = data.IndexOf(entry);

                // move to position
                if (moveRecord.ToPosition == MoveEntryPosition.Top)
                {
                    data.RemoveAt(fromIndex);
                    data.Insert(0, entry);
                }
                else if (moveRecord.ToPosition == MoveEntryPosition.Bottom)
                {
                    data.RemoveAt(fromIndex);
                    data.Add(entry);
                }
                else if (moveRecord.AfterID.IsMeaningful() || moveRecord.BeforeID.IsMeaningful())
                {
                    // get config
                    bool isAfterID = moveRecord.AfterID.IsMeaningful();
                    string anchorID = isAfterID ? moveRecord.AfterID.Value : moveRecord.BeforeID.Value;
                    errorLabel += $" {(isAfterID ? nameof(PatchMoveEntryConfig.AfterID) : nameof(PatchMoveEntryConfig.BeforeID))} \"{anchorID}\"";

                    // get anchor entry
                    TValue anchorEntry = GetByKey(anchorID);
                    if (anchorEntry == null)
                    {
                        this.LogOnce($"Can't move {errorLabel}: no entry with the relative ID exists.", LogLevel.Warn);
                        continue;
                    }
                    if (object.ReferenceEquals(entry, anchorEntry))
                    {
                        this.LogOnce($"Can't move {errorLabel}: can't move entry relative to itself.", LogLevel.Warn);
                        continue;
                    }

                    // move record
                    data.RemoveAt(fromIndex);
                    int newIndex = data.IndexOf(anchorEntry);
                    data.Insert(isAfterID ? newIndex + 1 : newIndex, entry);
                }
            }
        }

        /// <summary>Apply the patch to a dictionary asset.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="hasEntry">Get whether the collection has the given entry.</param>
        /// <param name="getEntry">Get an entry from the collection.</param>
        /// <param name="removeEntry">Remove an entry from the collection.</param>
        /// <param name="setEntry">Add or replace an entry in the collection.</param>
        private void ApplyCollection<TKey, TValue>(Func<TKey, bool> hasEntry, Func<TKey, TValue> getEntry, Action<TKey> removeEntry, Action<TKey, TValue> setEntry)
        {
            // apply records
            if (this.Records != null)
            {
                int i = 0;
                foreach (EditDataPatchRecord record in this.Records)
                {
                    i++;

                    // get key
                    TKey key = (TKey)Convert.ChangeType(record.Key.Value, typeof(TKey));

                    // apply string
                    if (typeof(TValue) == typeof(string))
                    {
                        if (record.Value?.Value == null)
                            removeEntry(key);
                        else if (record.Value.Value is JValue field)
                            setEntry(key, field.Value<TValue>());
                        else
                            this.Monitor.Log($"Can't apply data patch \"{this.LogName} > entry #{i}\" to {this.TargetAsset}: this asset has string values (but {record.Value.Value.Type} values were provided).", LogLevel.Warn);
                    }

                    // apply object
                    else
                    {
                        if (record.Value?.Value == null)
                            removeEntry(key);
                        else if (record.Value.Value is JObject field)
                            setEntry(key, field.ToObject<TValue>());
                        else
                            this.Monitor.Log($"Can't apply data patch \"{this.LogName} > entry #{i}\" to {this.TargetAsset}: this asset has {typeof(TValue)} values (but {record.Value.Value.Type} values were provided).", LogLevel.Warn);
                    }
                }
            }

            // apply fields
            if (this.Fields != null)
            {
                foreach (IGrouping<string, EditDataPatchField> recordGroup in this.Fields.GroupByIgnoreCase(p => p.EntryKey.Value))
                {
                    // get key
                    TKey key = (TKey)Convert.ChangeType(recordGroup.Key, typeof(TKey));
                    if (!hasEntry(key))
                    {
                        this.Monitor.Log($"Can't apply data patch \"{this.LogName}\" to {this.TargetAsset}: there's no record matching key '{key}' under {nameof(PatchConfig.Fields)}.", LogLevel.Warn);
                        continue;
                    }

                    // apply string
                    if (typeof(TValue) == typeof(string))
                    {
                        string[] actualFields = ((string)(object)getEntry(key)).Split('/');
                        foreach (EditDataPatchField field in recordGroup)
                        {
                            if (!int.TryParse(field.FieldKey.Value, out int index))
                            {
                                this.Monitor.Log($"Can't apply data field \"{this.LogName}\" to {this.TargetAsset}: record '{key}' under {nameof(PatchConfig.Fields)} is a string, so it requires a field index between 0 and {actualFields.Length - 1} (received \"{field.FieldKey}\"instead)).", LogLevel.Warn);
                                continue;
                            }
                            if (index < 0 || index > actualFields.Length - 1)
                            {
                                this.Monitor.Log($"Can't apply data field \"{this.LogName}\" to {this.TargetAsset}: record '{key}' under {nameof(PatchConfig.Fields)} has no field with index {field.FieldKey} (must be 0 to {actualFields.Length - 1}).", LogLevel.Warn);
                                continue;
                            }

                            actualFields[index] = field.Value.Value.Value<string>();
                        }

                        setEntry(key, (TValue)(object)string.Join("/", actualFields));
                    }

                    // apply object
                    else
                    {
                        JObject obj = new JObject();
                        foreach (EditDataPatchField field in recordGroup)
                            obj[field.FieldKey.Value] = field.Value.Value;

                        JsonSerializer serializer = new JsonSerializer();
                        using (JsonReader reader = obj.CreateReader())
                            serializer.Populate(reader, getEntry(key));
                    }
                }
            }
        }

        /// <summary>Get the key for a list asset entry.</summary>
        /// <typeparam name="TValue">The list value type.</typeparam>
        /// <param name="entity">The entity whose ID to fetch.</param>
        private string GetKey<TValue>(TValue entity)
        {
            return InternalConstants.GetListAssetKey(entity);
        }

        /// <summary>Log a message the first time it occurs.</summary>
        /// <param name="message">The log message.</param>
        /// <param name="level">The log level.</param>
        private void LogOnce(string message, LogLevel level)
        {
            if (this.LoggedWarnings.Add(message))
                this.Monitor.Log(message, level);
        }
    }
}
