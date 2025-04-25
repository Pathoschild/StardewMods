using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Framework.ContentManagers;
using StardewModdingAPI.Toolkit.Serialization;
using StardewValley;
using TMXTile;
using xTile;

namespace ContentPatcher.Framework.Commands.Commands;

/// <summary>A console command which saves a copy of an asset.</summary>
internal class ExportCommand : BaseCommand
{
    /*********
    ** Fields
    *********/
    /// <summary>The content helper with which to manage loaded assets.</summary>
    private readonly IGameContentHelper ContentHelper;

    /// <summary>The settings to use when writing data to a JSON file.</summary>
    private readonly Lazy<JsonSerializerSettings> JsonSettings = new(JsonHelper.CreateDefaultSettings);


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="contentHelper">The content helper with which to manage loaded assets.</param>
    public ExportCommand(IMonitor monitor, IGameContentHelper contentHelper)
        : base(monitor, "export")
    {
        this.ContentHelper = contentHelper;
    }

    /// <inheritdoc />
    public override string GetDescription()
    {
        return
            """
            patch export
               Usage: patch export "<asset name>"
               Saves a copy of an asset (including any changes from mods like Content Patcher) to the game folder. The asset name should be the target without the locale or extension, like "Characters/Abigail" if you want to export the value of 'Content/Characters/Abigail.xnb'.

               Usage: patch export "<asset name>" "<C# type name>"
               Same as the previous, but manually specify the C# data type for the asset. This must be the C# full type name. Example types:
                  - A string/string dictionary: System.Collections.Generic.Dictionary`2[[System.String],[System.String]]
                  - A number/string dictionary: System.Collections.Generic.Dictionary`2[[System.Int32],[System.String]]
                  - Movie reactions: System.Collections.Generic.List<StardewValley.GameData.Movies.MovieReaction>

              You can also specify 'image' as the type for a Texture2D value, or 'map' for a xTile.Map.
            """;
    }

    /// <inheritdoc />
    public override void Handle(string[] args)
    {
        // validate arguments
        if (args.Length is < 1 or > 2)
        {
            this.Monitor.Log("The 'patch export' command expects one argument containing the target asset name, and an optional second argument for the data type. See 'patch help' for more info.", LogLevel.Error);
            return;
        }

        // get arguments
        string assetName = args[0];
        string? typeName = args.Length > 1 ? args[1] : null;

        // get default type
        List<Type> possibleTypes = [.. this.TryGetTypes(typeName)];
        switch (possibleTypes.Count)
        {
            case 0:
                this.Monitor.Log($"Couldn't find type '{typeName}'. Type `patch help export` for usage.", LogLevel.Error);
                return;

            case 1:
                break;

            default:
                this.Monitor.Log($"Found multiple types matching '{typeName}'. Please enter one of these exact values:\n    - \"{string.Join("\n    - \"", possibleTypes.Select(possibleType => possibleType.AssemblyQualifiedName))}\"", LogLevel.Error);
                return;
        }

        // if needed, pick likely types based on the asset name
        if (possibleTypes[0] == typeof(object))
        {
            var likelyTypes = this.GetLikelyTypes(assetName);
            if (likelyTypes != null)
                possibleTypes.InsertRange(0, likelyTypes);
        }

        // load asset
        object? asset = null;
        Dictionary<Type, Exception>? exceptions = null;
        try
        {
            foreach (Type type in possibleTypes)
            {
                try
                {
                    asset = this.LoadAsset(assetName, type);
                    if (asset != null)
                        break;
                }
                catch (Exception ex) when (ex.InnerException is ContentLoadException or InvalidCastException) // use inner exception since LoadAsset uses reflection
                {
                    exceptions ??= new();
                    exceptions.Add(type, ex.InnerException);
                }
            }
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Can't load asset '{assetName}': {ex.Message}", LogLevel.Error);
        }

        if (asset is null)
        {
            this.Monitor.Log($"Couldn't load asset '{assetName}' using a likely type (tried: {string.Join(", ", possibleTypes.Select(p => p.FullName))}). Try either specifying the type, or waiting until the game loads it before exporting it. See the SMAPI log for details.", LogLevel.Error);
            if (exceptions is not null)
            {
                foreach ((Type type, Exception ex) in exceptions)
                    this.Monitor.Log($"Failed to load using type '{type.FullName}': {ex}");
            }
            return;
        }

        // init export path
        string fullTargetPath = Path.Combine(StardewModdingAPI.Constants.GamePath, "patch export", this.GetSanitizedFileName(assetName));
        Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);

        // export
        if (this.TryExportRaw(asset, ref fullTargetPath, out string? error))
            this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
        else
            this.Monitor.Log($"Failed exporting '{assetName}': {error}.", LogLevel.Error);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to export an arbitrary asset to disk.</summary>
    /// <param name="asset">The asset data to export.</param>
    /// <param name="path">The absolute path to which to write the asset, without the file extension. The file extension will be added if it's exported correctly.</param>
    /// <param name="error">An error phrase indicating why the asset couldn't be exported, if applicable.</param>
    private bool TryExportRaw(object? asset, ref string path, [NotNullWhen(false)] out string? error)
    {
        error = null;

        switch (asset)
        {
            case null:
                error = "the asset could not be loaded";
                return false;

            case Map map:
                path += ".tmx";
                this.ExportMap(map, path);
                return true;

            case Texture2D texture:
                path += ".png";
                this.ExportTexture(texture, path);
                return true;

            case IRawTextureData textureData:
                path += ".png";
                this.ExportRawTexture(textureData, path);
                return true;

            default:
                path += ".json";
                this.ExportData(asset, path);
                return true;
        }
    }

    /// <summary>Export a map asset to disk.</summary>
    /// <param name="rawMap">The asset to export.</param>
    /// <param name="path">The absolute path to which to write the asset.</param>
    private void ExportMap(Map rawMap, string path)
    {
        // derived from code written by Tyler Gibbs, licensed MIT
        // https://github.com/tylergibbs2/StardewValleyMods/blob/bd81d1e/StardewRoguelike/DebugCommands.cs#L369
        TMXFormat format = new(Game1.tileSize / Game1.pixelZoom, Game1.tileSize / Game1.pixelZoom, Game1.pixelZoom, Game1.pixelZoom);
        TMXMap map = format.Store(rawMap);
        foreach (TMXObjectgroup objectGroup in map.Objectgroups)
        {
            for (int i = 0; i < objectGroup.Objects.Count; i++)
            {
                // remove blank tile data
                if (objectGroup.Objects[i].Properties.Length == 0)
                    objectGroup.Objects[i] = null;
            }
        }

        // export matching tilesheets
        string exportFolder = Path.GetDirectoryName(path)!;
        foreach (TMXTileset tilesheet in map.Tilesets)
        {
            string tilesheetLocation = tilesheet.Image.Source;

            // first set to the relative location
            // so if tilesheet export fails
            // people can just copy the relative tilesheet over.
            tilesheet.Image.Source = Path.GetFileName(tilesheetLocation);
            Texture2D? imageAsset;
            {
                string? error = null;
                try
                {
                    imageAsset = this.LoadAssetImpl<Texture2D>(tilesheetLocation);
                }
                catch (ContentLoadException ex)
                {
                    imageAsset = null;
                    error = ex.Message;
                }
                if (imageAsset is null)
                {
                    this.Monitor.Log($"Failed while attempting to export tilesheets for map: Can't load asset '{tilesheetLocation}' with type '{typeof(Texture2D).FullName}'{(error != null ? $": {error}" : ".")}", LogLevel.Error);
                    continue;
                }
            }

            imageAsset = this.UnPremultiplyTransparency(imageAsset);
            string imageFilename = this.GetSanitizedFileName(Path.GetRelativePath(StardewModdingAPI.Constants.GamePath, tilesheetLocation));
            string imagePath = Path.Combine(exportFolder, imageFilename) + ".png";
            this.ExportTexture(imageAsset, imagePath);

            // set tilesheet path to sanitized name so map can be loaded in the map folder
            tilesheet.Image.Source = imageFilename;
            this.Monitor.Log($"Exported asset '{tilesheetLocation}' to '{imagePath}'.", LogLevel.Info);
        }

        // export the map itself
        var parser = new TMXParser();
        parser.Export(map, path);
    }

    /// <summary>Export a texture asset to disk.</summary>
    /// <param name="image">The asset to export.</param>
    /// <param name="path">The absolute path to which to write the asset.</param>
    private void ExportRawTexture(IRawTextureData image, string path)
    {
        using Texture2D exported = new Texture2D(Game1.graphics.GraphicsDevice, image.Width, image.Height);
        exported.SetData(image.Data);
        this.ExportTexture(exported, path);
    }

    /// <summary>Export a raw texture asset to disk.</summary>
    /// <param name="image">The asset to export.</param>
    /// <param name="path">The absolute path to which to write the asset.</param>
    private void ExportTexture(Texture2D image, string path)
    {
        using Texture2D exported = this.UnPremultiplyTransparency(image);
        using Stream stream = File.Create(path);
        exported.SaveAsPng(stream, exported.Width, exported.Height);
    }

    /// <summary>Export a data asset to disk.</summary>
    /// <param name="data">The asset to export.</param>
    /// <param name="path">The absolute path to which to write the asset.</param>
    private void ExportData(object data, string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(data, this.JsonSettings.Value));
    }

    /// <summary>Convert a full asset name like <c>Data/Buildings</c> into a filename-safe value like <c>Data_Buildings</c>.</summary>
    /// <param name="assetName">The raw asset name.</param>
    private string GetSanitizedFileName(string assetName)
    {
        return string.Join('_', assetName.Split(Path.GetInvalidFileNameChars()));
    }

    /// <summary>Get the types matching a name, if any.</summary>
    /// <param name="name">The type name.</param>
    private Type[] TryGetTypes(string? name)
    {
        // none specified, default to object
        if (string.IsNullOrWhiteSpace(name))
            return [typeof(object)];

        // short alias
        if (string.Equals(name, "image", StringComparison.OrdinalIgnoreCase))
            return [typeof(Texture2D)];
        if (string.Equals(name, "map", StringComparison.OrdinalIgnoreCase))
            return [typeof(Map)];

        // by assembly-qualified name
        {
            Type? type = Type.GetType(name);
            if (type != null)
                return [type];
        }

        // by type name
        {
            HashSet<Type> typesByName = [];
            HashSet<Type> typesByFullName = [];
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                foreach (Type type in assembly.GetExportedTypes())
                {
                    try
                    {
                        if (string.Equals(type.FullName, name, StringComparison.OrdinalIgnoreCase))
                            typesByFullName.Add(type);
                        if (string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase))
                            typesByName.Add(type);
                    }
                    catch
                    {
                        // ignore invalid types
                    }
                }
            }

            HashSet<Type> matches = typesByFullName.Any()
                ? typesByFullName
                : typesByName;
            return matches.OrderBy(p => p.FullName, HumanSortComparer.DefaultIgnoreCase).ToArray();

        }
    }

    /// <summary>Try to get likely export types for an asset name.</summary>
    /// <param name="asset">The asset name.</param>
    private List<Type>? GetLikelyTypes(string asset)
    {
        IAssetName assetName = this.ContentHelper.ParseAssetName(asset);

        // based on path
        if (assetName.IsDirectlyUnderPath("Maps"))
            return [typeof(Map), typeof(Texture2D)];

        if (
            assetName.IsDirectlyUnderPath("Animals")
            || assetName.IsDirectlyUnderPath("Buildings")
            || assetName.IsDirectlyUnderPath("Characters")
            || assetName.IsDirectlyUnderPath("Portraits")
            || assetName.IsDirectlyUnderPath("Minigames")
            || assetName.IsDirectlyUnderPath("TerrainFeatures")
            || assetName.IsDirectlyUnderPath("TileSheets")
        )
            return [typeof(Texture2D)];

        if (
            assetName.IsDirectlyUnderPath("Characters/Dialogue")
            || assetName.IsDirectlyUnderPath("Characters/schedules")
            || assetName.IsDirectlyUnderPath("Data/Events")
            || assetName.IsDirectlyUnderPath("Data/Festivals")
        )
            return [typeof(Dictionary<string, string>)];

        // based on DataLoader method
        if (assetName.IsDirectlyUnderPath("Data"))
        {
            string name = assetName.BaseName["Data/".Length..];
            if (name.Contains('_'))
                return null; // no vanilla data asset has `_` in its name, but DataLoader uses it for subfolders like 'Festivals_FestivalDates'

            MethodInfo? method = typeof(DataLoader).GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (method != null)
                return [method.ReturnType];
        }

        return null;
    }

    /// <summary>Reverse premultiplication applied to an image asset by the XNA content pipeline.</summary>
    /// <param name="texture">The texture to adjust.</param>
    private Texture2D UnPremultiplyTransparency(Texture2D texture)
    {
        Color[] data = new Color[texture.Width * texture.Height];
        texture.GetData(data);

        for (int i = 0; i < data.Length; i++)
        {
            Color pixel = data[i];
            if (pixel.A == 0)
                continue;

            data[i] = new Color(
                (byte)((pixel.R * 255) / pixel.A),
                (byte)((pixel.G * 255) / pixel.A),
                (byte)((pixel.B * 255) / pixel.A),
                pixel.A
            ); // don't use named parameters, which are inconsistent between MonoGame (e.g. 'alpha') and XNA (e.g. 'a')
        }

        Texture2D result = new Texture2D(texture.GraphicsDevice ?? Game1.graphics.GraphicsDevice, texture.Width, texture.Height);
        result.SetData(data);
        return result;
    }

    /// <summary>Load an asset from a content manager using the given type.</summary>
    /// <param name="assetName">The asset name to load.</param>
    /// <param name="type">The asset type to load.</param>
    private object? LoadAsset(string assetName, Type type)
    {
        return this
            .GetType()
            .GetMethod(nameof(this.LoadAssetImpl), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(type)
            .Invoke(this, [assetName]);
    }

    /// <summary>Load an asset from a content manager using the given type.</summary>
    /// <typeparam name="TAsset">The asset type to load.</typeparam>
    /// <param name="assetName">The asset name to load.</param>
    private TAsset? LoadAssetImpl<TAsset>(string assetName)
    {
        // get from main content manager if it's already cached
        if (this.IsAssetLoaded(Game1.content, assetName))
            return Game1.content.Load<TAsset>(assetName);

        // If it's not already cached, use a temporary content manager
        // This avoids corrupting the cache with an invalid type if it doesn't match.
        using ContentManager contentManager = Game1.content.CreateTemporary();
        return contentManager.Load<TAsset?>(assetName);
    }

    /// <summary>Get whether the given content manager has already loaded and cached the given asset.</summary>
    /// <param name="contentManager">The content manager to check.</param>
    /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
    [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier", Justification = "Extra validation in error-handling.")]
    private bool IsAssetLoaded(ContentManager contentManager, string assetName)
    {
        IAssetName parsedName = this.ContentHelper.ParseAssetName(assetName);

        return contentManager is IContentManager managed
            ? managed.IsLoaded(parsedName)
            : throw new InvalidOperationException($"Can't access internals for content manager with type {contentManager?.GetType().FullName ?? "null"}, expected implementation of {typeof(IContentManager).FullName}.");
    }
}
