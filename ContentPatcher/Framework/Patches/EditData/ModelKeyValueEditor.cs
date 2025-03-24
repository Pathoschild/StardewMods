using System;
using System.Collections.Generic;
using System.Reflection;
using Sickhead.Engine.Util;

namespace ContentPatcher.Framework.Patches.EditData;

/// <summary>Encapsulates applying key/value edits to a data model.</summary>
internal class ModelKeyValueEditor : BaseDataEditor
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying data.</summary>
    private readonly object Data;

    /// <summary>Maps entry names to the associated field or property.</summary>
    private readonly Lazy<Dictionary<string, MemberInfo>> FieldMap;


    /*********
    ** Accessors
    *********/
    /// <summary>The field names defined for this model.</summary>
    public IEnumerable<string> FieldNames => this.FieldMap.Value.Keys.OrderByHuman();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="data">The underlying data.</param>
    public ModelKeyValueEditor(object data)
    {
        this.CanAddEntries = false;

        this.Data = data;
        this.FieldMap = new(this.GetFieldMap);
    }

    /// <inheritdoc />
    public override object ParseKey(string key)
    {
        return key;
    }

    /// <inheritdoc />
    public override bool HasEntry(object key)
    {
        string name = (string)key;

        return this.FieldMap.Value.ContainsKey(name);
    }

    /// <inheritdoc />
    public override object? GetEntry(object key)
    {
        string name = (string)key;

        return this
            .GetMember(name)
            ?.GetValue(this.Data);
    }

    /// <inheritdoc />
    public override Type? GetEntryType(object key)
    {
        string name = (string)key;

        return this
            .GetMember(name)
            ?.GetDataType();
    }

    /// <inheritdoc />
    public override void RemoveEntry(object key)
    {
        this.SetEntry(key, null);
    }

    /// <inheritdoc />
    public override void SetEntry(object key, object? value)
    {
        string name = (string)key;

        this
            .GetMember(name)
            ?.SetValue(this.Data, value);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a map of field/property names to class members.</summary>
    private Dictionary<string, MemberInfo> GetFieldMap()
    {
        Type type = this.Data.GetType();
        var map = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (FieldInfo field in type.GetFields())
        {
            if (field.IsPrivate || field.IsFamily)
                continue;

            map[field.Name] = field;
        }

        foreach (PropertyInfo property in type.GetProperties())
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            map[property.Name] = property;
        }

        return map;
    }

    /// <summary>Get the member for the given field or property name, if any.</summary>
    /// <param name="name">The field or property name.</param>
    private MemberInfo? GetMember(string name)
    {
        return this.FieldMap.Value.GetValueOrDefault(name);
    }
}
