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

        /// <summary>The settings to use when writing data to a JSON file.</summary>
        private readonly Lazy<JsonSerializerSettings> JsonSettings = new Lazy<JsonSerializerSettings>(JsonHelper.CreateDefaultSettings);


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

                  You can also specify 'image' as the type for a Texture2D value.
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

            // init export path
            string fullTargetPath = Path.Combine(StardewModdingAPI.Constants.GamePath, "patch export", string.Join('_', assetName.Split(Path.GetInvalidFileNameChars())));
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

                case Map:
                    error = "can't export map assets";
                    return false;

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
