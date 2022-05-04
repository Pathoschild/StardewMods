using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace ContentPatcher.Framework.Patches.EditData
{
    /// <summary>Constructs key/value editors for arbitrary data.</summary>
    internal class KeyValueEditorFactory
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get an editor for the given data structure, if it can be edited.</summary>
        /// <param name="data">The data structure to edit.</param>
        /// <param name="editor">The editor if the data type is supported, else <c>null</c>.</param>
        public bool TryGetEditorFor([NotNullWhen(true)] object? data, [NotNullWhen(true)] out IKeyValueEditor? editor)
        {
            // get type
            Type? type = data?.GetType();
            editor = null;
            if (data == null || type == null)
                return false;

            // handle dictionary
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // get dictionary's key/value types
                Type[] genericArgs = type.GetGenericArguments();
                if (genericArgs.Length != 2)
                    throw new InvalidOperationException("Can't parse the asset's dictionary key/value types.");
                Type? keyType = type.GetGenericArguments().FirstOrDefault();
                Type? valueType = type.GetGenericArguments().LastOrDefault();
                if (keyType == null)
                    throw new InvalidOperationException("Can't parse the asset's dictionary key type.");
                if (valueType == null)
                    throw new InvalidOperationException("Can't parse the asset's dictionary value type.");

                // get underlying apply method
                MethodInfo? method = this.GetType().GetMethod(nameof(this.GetDictionaryEditor), BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                    throw new InvalidOperationException($"Can't fetch the internal {nameof(this.GetDictionaryEditor)} method.");

                // invoke method
                editor = (IKeyValueEditor)method
                    .MakeGenericMethod(keyType, valueType)
                    .Invoke(this, new[] { data })!;
                return true;
            }

            // handle list types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // get list's value type
                Type? keyType = type.GetGenericArguments().FirstOrDefault();
                if (keyType == null)
                    throw new InvalidOperationException("Can't parse the asset's list value type.");

                // get underlying apply method
                MethodInfo? method = this.GetType().GetMethod(nameof(this.GetListEditor), BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                    throw new InvalidOperationException($"Can't fetch the internal {nameof(this.GetListEditor)} method.");

                // invoke method
                editor = (IKeyValueEditor)method
                    .MakeGenericMethod(keyType)
                    .Invoke(this, new[] { data })!;
                return true;
            }

            // data model
            if (!type.IsValueType && !type.IsGenericType && type != typeof(string))
            {
                editor = new ModelKeyValueEditor(data);
                return true;
            }

            // unknown type
            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get an editor for a dictionary.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="data">The data to edit.</param>
        private IKeyValueEditor GetDictionaryEditor<TKey, TValue>(IDictionary<TKey, TValue> data)
        {
            return new DictionaryKeyValueEditor<TKey, TValue>(data);
        }

        /// <summary>Get an editor for a list.</summary>
        /// <typeparam name="TValue">The list value type.</typeparam>
        /// <param name="data">The data to edit.</param>
        private IKeyValueEditor GetListEditor<TValue>(IList<TValue> data)
        {
            return new ListKeyValueEditor<TValue>(data);
        }
    }
}
