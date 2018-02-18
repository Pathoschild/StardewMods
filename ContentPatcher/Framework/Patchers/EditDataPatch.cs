using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patchers
{
    /// <summary>Metadata for a data to edit into a data file.</summary>
    internal class EditDataPatch : IPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The data records to edit.</summary>
        private readonly IDictionary<string, string> Records;

        /// <summary>The data fields to edit.</summary>
        private readonly IDictionary<string, IDictionary<int, string>> Fields;


        /*********
        ** Accessors
        *********/
        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The language code to patch (or <c>null</c> for any language).</summary>
        /// <remarks>This is handled by the main logic.</remarks>
        public string Locale { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="locale">The language code to patch (or <c>null</c> for any language).</param>
        /// <param name="records">The data records to edit.</param>
        /// <param name="fields">The data fields to edit.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public EditDataPatch(IContentPack contentPack, string assetName, string locale, IDictionary<string, string> records, IDictionary<string, IDictionary<int, string>> fields, IMonitor monitor)
        {
            // init
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.Locale = locale;
            this.Records = records;
            this.Fields = fields;
            this.Monitor = monitor;
        }

        /// <summary>Apply the patch to an asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        public void Apply<T>(IAssetData asset)
        {
            // validate
            if (!typeof(T).IsGenericType || typeof(T).GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                this.Monitor.Log($"Can't apply edit-data patch by {this.ContentPack.Manifest.Name} to {this.AssetName}: this file isn't a data file (found {(typeof(T) == typeof(Texture2D) ? "image" : typeof(T).Name)}).", LogLevel.Warn);
                return;
            }

            // get dictionary's key type
            Type keyType = typeof(T).GetGenericArguments().FirstOrDefault();
            if (keyType == null)
                throw new InvalidOperationException("Can't parse the asset's dictionary key type.");

            // get underlying apply method
            MethodInfo method = this.GetType().GetMethod(nameof(this.ApplyImpl), BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException("Can't fetch the internal apply method.");

            // invoke method
            method
                .MakeGenericMethod(keyType)
                .Invoke(this, new object[] { asset });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Apply the patch to an asset.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        private void ApplyImpl<TKey>(IAssetData asset)
        {
            IDictionary<TKey, string> data = asset.AsDictionary<TKey, string>().Data;

            // apply records
            if (this.Records != null)
            {
                foreach (KeyValuePair<string, string> record in this.Records)
                {
                    TKey key = (TKey)Convert.ChangeType(record.Key, typeof(TKey));
                    data[key] = record.Value;
                }
            }

            // apply fields
            if (this.Fields != null)
            {
                foreach (KeyValuePair<string, IDictionary<int, string>> record in this.Fields)
                {
                    TKey key = (TKey)Convert.ChangeType(record.Key, typeof(TKey));
                    if (!data.ContainsKey(key))
                    {
                        this.Monitor.Log($"Can't apply data field patch by {this.ContentPack.Manifest.Name} to {this.AssetName}: there's no record matching key '{key}' under {nameof(PatchConfig.Fields)}.", LogLevel.Warn);
                        continue;
                    }

                    string[] actualFields = data[key].Split('/');
                    foreach (KeyValuePair<int, string> field in record.Value)
                    {
                        if (field.Key < 0 || field.Key > actualFields.Length - 1)
                        {
                            this.Monitor.Log($"Can't apply data field patch by {this.ContentPack.Manifest.Name} to {this.AssetName}: record '{key}' under {nameof(PatchConfig.Fields)} has no field with index {field.Key} (must be 0 to {actualFields.Length - 1}).", LogLevel.Warn);
                            continue;
                        }

                        actualFields[field.Key] = field.Value;
                    }

                    data[key] = string.Join("/", actualFields);
                }
            }
        }
    }
}
