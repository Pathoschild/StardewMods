#nullable disable

using System;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>Encapsulates applying key/value edits to a delimited string.</summary>
    internal class DelimitedStringKeyValueEditor : BaseDataEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The editor for the entry containing the delimited string field.</summary>
        private readonly IKeyValueEditor EntryEditor;

        /// <summary>The delimited string field's key in the <see cref="EntryEditor"/>.</summary>
        private readonly object EntryKey;

        /// <summary>The field delimiter for the data asset's string values.</summary>
        private readonly char FieldDelimiter;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="entryEditor">The editor for the entry containing the delimited string field.</param>
        /// <param name="entryKey">The delimited string field's key in the <see cref="EntryEditor"/>.</param>
        /// <param name="fieldDelimiter">The field delimiter for the data asset's string values.</param>
        public DelimitedStringKeyValueEditor(IKeyValueEditor entryEditor, object entryKey, char fieldDelimiter)
        {
            this.EntryEditor = entryEditor;
            this.EntryKey = entryKey;
            this.FieldDelimiter = fieldDelimiter;
        }

        /// <inheritdoc />
        public override object ParseKey(string key)
        {
            return int.Parse(key);
        }

        /// <inheritdoc />
        public override object GetEntry(object key)
        {
            return this.TryGetField(key, out string value, out _)
                ? value
                : null;
        }

        /// <inheritdoc />
        public override Type GetEntryType(object key)
        {
            return typeof(string);
        }

        /// <inheritdoc />
        public override void RemoveEntry(object key)
        {
            this.SetEntry(key, null);
        }

        /// <inheritdoc />
        public override void SetEntry(object key, JToken value)
        {
            // get index
            int index = (int)key;
            if (index < 0)
                return;

            // get fields
            string[] fields = this.GetFields();
            if (fields is null)
                return;

            // add empty fields if needed
            // Data assets sometimes have optional fields, so this allows editing a later optional
            // field. For example, given asset "a/b", setting index 5 to "c" will result in "a/b///c".
            if (index > fields.Length - 1)
            {
                int firstAdded = fields.Length;

                Array.Resize(ref fields, index + 1);

                for (int i = firstAdded; i < fields.Length; i++)
                    fields[i] = string.Empty;
            }

            // apply change
            fields[index] = value.Value<string>();
            this.EntryEditor.SetEntry(this.EntryKey, string.Join(this.FieldDelimiter, fields));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the fields for the entry.</summary>
        private string[] GetFields()
        {
            if (this.EntryEditor.GetEntry(this.EntryKey) is not string str)
                return null;

            return str.Split(this.FieldDelimiter);
        }

        /// <summary>Get a field from the underlying data.</summary>
        /// <param name="key">The raw field key.</param>
        /// <param name="value">The value found in the delimited string.</param>
        /// <param name="index">The value's index within the string.</param>
        private bool TryGetField(object key, out string value, out int index)
        {
            // get index
            index = (int)key;
            if (index < 0)
            {
                value = null;
                return false;
            }

            // get fields
            string[] fields = this.GetFields();
            if (fields is null || index >= fields.Length)
            {
                value = null;
                return false;
            }

            // get field
            value = fields[index];
            return true;
        }
    }
}
