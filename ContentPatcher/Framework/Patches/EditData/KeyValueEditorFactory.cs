using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace ContentPatcher.Framework.Patches.EditData;

/// <summary>Constructs key/value editors for arbitrary data.</summary>
internal class KeyValueEditorFactory
{
    /*********
    ** Fields
    *********/
    /// <summary>A cache of editor constructors by data type.</summary>
    private static readonly Dictionary<Type, Func<object, IKeyValueEditor>?> CachedConstructors = [];


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

        // get factory
        if (!KeyValueEditorFactory.CachedConstructors.TryGetValue(type, out Func<object, IKeyValueEditor>? getEditor))
        {
            if (!this.TryGetConstructorWithoutCache(type, out getEditor))
                getEditor = null;
            KeyValueEditorFactory.CachedConstructors[type] = getEditor;
        }

        // build editor
        if (getEditor != null)
        {
            editor = getEditor(data);
            return true;
        }

        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get an editor constructor for the given data structure (if it can be edited), ignoring the factory cache.</summary>
    /// <param name="type">The data type to edit.</param>
    /// <param name="getEditor">A callback which creates an editor for data of the given type, else <c>null</c>.</param>
    private bool TryGetConstructorWithoutCache(Type type, [NotNullWhen(true)] out Func<object, IKeyValueEditor>? getEditor)
    {
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

            // get constructor builder
            MethodInfo? method = this.GetType().GetMethod(nameof(this.GetConstructorForDictionary), BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException($"Can't fetch the internal {nameof(this.GetConstructorForDictionary)} method.");

            // create callback
            getEditor = (Func<object, IKeyValueEditor>)method.MakeGenericMethod(keyType, valueType).Invoke(this, null)!;
            return true;
        }

        // handle list types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            // get list's value type
            Type? keyType = type.GetGenericArguments().FirstOrDefault();
            if (keyType == null)
                throw new InvalidOperationException("Can't parse the asset's list value type.");

            // get constructor builder
            MethodInfo? method = this.GetType().GetMethod(nameof(this.GetConstructorForList), BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException($"Can't fetch the internal {nameof(this.GetConstructorForList)} method.");

            // create callback
            getEditor = (Func<object, IKeyValueEditor>)method.MakeGenericMethod(keyType).Invoke(this, null)!;
            return true;
        }

        // data model
        if (type is { IsValueType: false, IsGenericType: false } && type != typeof(string))
        {
            getEditor = data => new ModelKeyValueEditor(data);
            return true;
        }

        // unknown type
        getEditor = null;
        return false;
    }


    /// <summary>Get an editor for a dictionary.</summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    private Func<object, IKeyValueEditor> GetConstructorForDictionary<TKey, TValue>()
    {
        return data => new DictionaryKeyValueEditor<TKey, TValue>((IDictionary<TKey, TValue>)data);
    }

    /// <summary>Get an editor for a list.</summary>
    /// <typeparam name="TValue">The list value type.</typeparam>
    private Func<object, IKeyValueEditor> GetConstructorForList<TValue>()
    {
        return data => new ListKeyValueEditor<TValue>((IList<TValue>)data);
    }
}
