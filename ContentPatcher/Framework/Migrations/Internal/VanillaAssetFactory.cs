using System;
using System.Diagnostics.CodeAnalysis;
using Force.DeepCloner;
using StardewValley;
using StardewValley.ContentManagement;

namespace ContentPatcher.Framework.Migrations.Internal;

/// <summary>A factory which produces a copy of an asset's original data without mod edits applied.</summary>
/// <typeparam name="T">The asset type.</typeparam>
internal class VanillaAssetFactory<T>
    where T : class
{
    /*********
    ** Fields
    *********/
    /// <summary>The loaded asset data.</summary>
    private readonly Lazy<T> Data;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="load">The asset name to load.</param>
    public VanillaAssetFactory(Func<IContentManager, T> load)
    {
        this.Data = new Lazy<T>(() => VanillaAssetFactory<T>.LoadVanillaData(load));
    }

    /// <summary>Get a fresh copy of the asset with no edits applied.</summary>
    [return: NotNull]
    public T GetFreshCopy()
    {
        return this.Data.Value.DeepClone();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Load the vanilla data for an asset without mod edits applied.</summary>
    /// <param name="load">Load the asset from a content manager.</param>
    public static T LoadVanillaData(Func<IContentManager, T> load)
    {
        using var content = new LocalizedContentManager(Game1.game1.Content.ServiceProvider, Game1.content.GetContentRoot());
        return load(content);
    }
}
