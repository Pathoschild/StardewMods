using System;
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
    /// <summary>Encapsulates monitoring and logging.</summary>
    protected readonly IMonitor Monitor;

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
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="reflection">Simplifies access to private game code.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    protected BaseLookupProvider(IMonitor monitor, IReflectionHelper reflection, GameHelper gameHelper)
    {
        this.Monitor = monitor;
        this.Reflection = reflection;
        this.GameHelper = gameHelper;
    }

    /// <summary>Try to get the value of a property or field with one of the given names.</summary>
    /// <typeparam name="T">The expected member type.</typeparam>
    /// <param name="obj">The object whose members to search.</param>
    /// <param name="value">The value that was found, if applicable.</param>
    /// <param name="propertyOrFieldNames">The member names to match.</param>
    /// <returns>Returns whether a matching member was found with a non-null value.</returns>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = "Reflection API can return null when `required: false` is set.")]
    public bool TryGetPropertyOrField<T>(object obj, [NotNullWhen(true)] out T? value, params string[] propertyOrFieldNames)
    {
        // property
        foreach (string name in propertyOrFieldNames)
        {
            IReflectedProperty<T>? property = this.Reflection.GetProperty<T>(obj, name, required: false);
            if (property != null)
            {
                try
                {
                    value = property.GetValue();
                    if (value is not null)
                        return true;
                }
                catch (Exception ex)
                {
                    this.Monitor.LogOnce($"Couldn't get subject from the {obj.GetType().FullName} menu's {name} property: {ex.Message}");
                }
            }
        }

        // field
        foreach (string name in propertyOrFieldNames)
        {
            IReflectedField<T>? field = this.Reflection.GetField<T>(obj, name, required: false);
            if (field != null)
            {
                try
                {
                    value = field.GetValue();
                    if (value is not null)
                        return true;
                }
                catch (Exception ex)
                {
                    this.Monitor.LogOnce($"Couldn't get subject from the {obj.GetType().FullName} menu's {name} field: {ex.Message}");
                }
            }
        }

        // none found
        value = default;
        return false;
    }
}
