using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Framework.ContentManagers;
using StardewValley;

using xTile;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which saves a copy of an asset.</summary>
    internal class ExportCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>The content helper with which to manage loaded assets.</summary>
        private readonly IGameContentHelper ContentHelper;



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
            return @"
                patch export
                   Usage: patch export ""<asset name>""
                   Saves a copy of an asset (including any changes from mods like Content Patcher) to the game folder. The asset name should be the target without the locale or extension, like ""Characters/Abigail"" if you want to export the value of 'Content/Characters/Abigail.xnb'.

                   Usage: patch export ""<asset name>"" ""<C# type name>""
                   Same as the previous, but manually specify the C# data type for the asset. This must be the C# full type name. Example types:
                      - A string/string dictionary: System.Collections.Generic.Dictionary`2[[System.String],[System.String]]
                      - A number/string dictionary: System.Collections.Generic.Dictionary`2[[System.Int32],[System.String]]
                      - Movie reactions: System.Collections.Generic.List<StardewValley.GameData.Movies.MovieReaction>

                  You can also specify 'image' as the type for a Texture2D value, or 'map' for a XTile.Map.
            ";
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

            // load type
            Type? type = null;
            {
                Type[] possibleTypes = this.TryGetTypes(typeName);
                switch (possibleTypes.Length)
                {
                    case 0:
                        this.Monitor.Log($"Couldn't find type '{typeName}'. Type `patch help export` for usage.", LogLevel.Error);
                        break;

                    case 1:
                        type = possibleTypes[0];
                        break;

                    default:
                        this.Monitor.Log($"Found multiple types matching '{typeName}'. Please enter one of these exact values:\n    - \"{string.Join("\n    - \"", possibleTypes.Select(possibleType => possibleType.AssemblyQualifiedName))}\"", LogLevel.Error);
                        break;
                }
            }
            if (type is null)
                return;

            // load asset
            object? asset;
            try
            {
                asset = this.LoadAsset(assetName, type);
            }
            catch (ContentLoadException ex)
            {
                this.Monitor.Log($"Can't load asset '{assetName}' with type '{type.FullName}': {ex.Message}", LogLevel.Error);
                return;
            }

            static string GetSanitizedName(string assetName) =>
                string.Join('_', assetName.Split(Path.GetInvalidFileNameChars()));

            string filepath = GetSanitizedName(assetName);
            string fullTargetPath = Path.Combine(StardewModdingAPI.Constants.GamePath, "patch export", filepath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);

            // export
            if (asset is Texture2D texture)
            {
                fullTargetPath += ".png";

                texture = this.UnPremultiplyTransparency(texture);
                using (Stream stream = File.Create(fullTargetPath))
                    texture.SaveAsPng(stream, texture.Width, texture.Height);

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else if (asset is Map map)
            {

                // derived from code written by Tyler Gibbs here: https://github.com/tylergibbs2/StardewValleyMods/blob/bd81d1eac26faf153bb7577215f12c9d3a98b342/StardewRoguelike/DebugCommands.cs#L369
                // which is licensed MIT

                TMXTile.TMXFormat Format = new(Game1.tileSize / Game1.pixelZoom, Game1.tileSize / Game1.pixelZoom, Game1.pixelZoom, Game1.pixelZoom);
                TMXTile.TMXMap tmxMap = Format.Store(map);

                fullTargetPath += ".tmx";

                foreach (TMXTile.TMXObjectgroup? objectGroup in tmxMap.Objectgroups)
                {
                    for (int i = 0; i < objectGroup.Objects.Count; i++)
                    {
                        var tmxObject = objectGroup.Objects[i];
                        tmxObject.Properties = tmxObject.Properties.Skip(2).ToArray();
                        if (tmxObject.Properties.Length == 0)
                            objectGroup.Objects[i] = null;
                    }
                }

                // export the matching tilesheets as well.
                string contentPath = StardewModdingAPI.Constants.GamePath;
                foreach (var tileset in tmxMap.Tilesets)
                {
                    string tilesheetLocation = tileset.Image.Source;

                    // first set to the relative location
                    // so if tilesheet export fails
                    // people can just copy the relative tilesheet over.
                    tileset.Image.Source = Path.GetFileName(tilesheetLocation);
                    object? imageAsset;
                    try
                    {
                        imageAsset = this.LoadAsset(tilesheetLocation, typeof(Texture2D));
                    }
                    catch (ContentLoadException ex)
                    {
                        this.Monitor.Log($"Failed while attempting to export tilesheets for map: Can't load asset '{tilesheetLocation}' with type '{typeof(Texture2D).FullName}': {ex.Message}", LogLevel.Error);
                        continue;
                    }

                    if (imageAsset is not Texture2D tex)
                    {
                        this.Monitor.Log($"Attempted export failed for tilesheet {tilesheetLocation}.");
                        continue;
                    }

                    tex = this.UnPremultiplyTransparency(tex);
                    string relativeImageLocation = GetSanitizedName(Path.GetRelativePath(contentPath, tileset.Image.Source));
                    string fullImagePath = Path.Combine(Path.GetDirectoryName(fullTargetPath)!, relativeImageLocation) + ".png";
                    using (Stream stream = File.Create(fullImagePath))
                        tex.SaveAsPng(stream, tex.Width, tex.Height);

                    // set the tilesheet path to the sanitized name here.
                    tileset.Image.Source = relativeImageLocation;

                    this.Monitor.Log($"Exported asset '{tilesheetLocation}' to '{fullImagePath}'.", LogLevel.Info);
                }


                // export the map itself.
                var parser = new TMXTile.TMXParser();
                parser.Export(tmxMap, fullTargetPath, TMXTile.DataEncodingType.XML);

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else if (this.IsDataAsset(asset))
            {
                fullTargetPath += ".json";

                File.WriteAllText(fullTargetPath, JsonConvert.SerializeObject(asset, Formatting.Indented));

                this.Monitor.Log($"Exported asset '{assetName}' to '{fullTargetPath}'.", LogLevel.Info);
            }
            else
                this.Monitor.Log($"Can't export asset '{assetName}' of type {asset?.GetType().FullName ?? "null"}, expected image, data, or map.", LogLevel.Error);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the types matching a name, if any.</summary>
        /// <param name="name">The type name.</param>
        private Type[] TryGetTypes(string? name)
        {
            // none specified, default to object
            if (string.IsNullOrWhiteSpace(name))
                return new[] { typeof(object) };

            // short alias
            if (string.Equals(name, "image", StringComparison.OrdinalIgnoreCase))
                return new[] { typeof(Texture2D) };

            if (string.Equals(name, "map", StringComparison.OrdinalIgnoreCase))
                return new[] { typeof(Map) };

            // by assembly-qualified name
            {
                Type? type = Type.GetType(name);
                if (type != null)
                    return new[] { type };
            }

            // by type name
            {
                HashSet<Type> typesByName = new();
                HashSet<Type> typesByFullName = new();
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

        /// <summary>Get whether an asset can be saved to JSON.</summary>
        /// <param name="asset">The asset to check.</param>
        private bool IsDataAsset(object? asset)
        {
            if (asset is null)
                return false;

            Type type = asset.GetType();
            type = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            return type == typeof(Dictionary<,>) || type == typeof(List<>) || type == typeof(JArray);
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
                .Invoke(this, new object[] { assetName });
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
}
