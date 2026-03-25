using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

/// <inheritdoc />
internal abstract class BaseLookupProvider : ILookupProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Simplifies access to private game code.</summary>
    protected readonly IReflectionHelper Reflection;

    /// <summary>Provides utility methods for interacting with the game code.</summary>
    protected readonly GameHelper GameHelper;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public virtual IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
    {
        yield break;
    }

    /// <inheritdoc />
    public virtual ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
    {
        return null;
    }

    /// <inheritdoc />
    public virtual ISubject? GetSubjectFor(object entity, GameLocation? location)
    {
        return null;
    }

    /// <inheritdoc />
    public virtual IEnumerable<ISubject> GetSearchSubjects()
    {
        yield break;
    }


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="reflection">Simplifies access to private game code.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    protected BaseLookupProvider(IReflectionHelper reflection, GameHelper gameHelper)
    {
        this.Reflection = reflection;
        this.GameHelper = gameHelper;
    }

    /// <summary>Try to get a property or field's value, based on one or more property or field names</summary>
    /// <typeparam name="T">Type of property/field</typeparam>
    /// <param name="reflection">SMAPI reflection helper helper</param>
    /// <param name="thisObject">The object to reflect on</param>
    /// <param name="value">Found property or field value</param>
    /// <param name="propertyOrFieldNames">One or more property/field names</param>
    /// <returns>true if value found</returns>
    public bool TryPropertyOrField<T>(object thisObject, [NotNullWhen(true)] out T? value, params string[] propertyOrFieldNames)
    {
        // properties
        foreach (string name in propertyOrFieldNames)
        {
            var prop = this.Reflection.GetProperty<T>(thisObject, name, required: false);
            if (prop != null && (value = prop.GetValue()) != null)
                return true;
        }
        // fields
        foreach (string name in propertyOrFieldNames)
        {
            var field = this.Reflection.GetField<T>(thisObject, name, required: false);
            if (field != null && (value = field.GetValue()) != null)
                return true;
        }
        value = default;
        return false;
    }
}
