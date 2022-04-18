#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.TestMod
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique assets for which <see cref="IAssetLoader.CanLoad{T}"/> was called.</summary>
        private readonly HashSet<IAssetName> LoadedAssets = new();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            this.LoadedAssets.Add(e.Name);

            //if (typeof(Texture2D).IsAssignableFrom(e.DataType))
            //{
            //    e.Edit(editor =>
            //    {
            //        Texture2D texture = editor.AsImage().Data;
            //        texture = this.GrayscaleColors(texture);
            //        editor.ReplaceWith(texture);
            //    });
            //}
        }

        /// <inheritdoc cref="IInputEvents.ButtonPressed"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // print list of loaded assets
            if (e.Button == SButton.F12)
            {
                string[] assets = this.LoadedAssets.Select(p => p.Name).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
                this.Monitor.Log($"CanLoad<T> was called for these assets:\n   {string.Join("\n   ", assets)}", LogLevel.Info);
            }
        }

        /// <summary>Create a copy of a texture with inverted colors.</summary>
        /// <param name="source">The original texture to copy.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "This is test code.")]
        private Texture2D InvertColors(Texture2D source)
        {
            // get source pixels
            Color[] pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);

            // invert pixel colors
            for (int i = 0; i < pixels.Length; i++)
            {
                Color color = pixels[i];
                if (color.A == 0)
                    continue; // transparent

                pixels[i] = new Color(byte.MaxValue - color.R, byte.MaxValue - color.G, byte.MaxValue - color.B, color.A);
            }

            // create new texture
            Texture2D target = new Texture2D(source.GraphicsDevice, source.Width, source.Height);
            target.SetData(pixels);
            return target;
        }

        /// <summary>Create a copy of a texture with grayscale colors.</summary>
        /// <param name="source">The original texture to copy.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "This is test code.")]
        private Texture2D GrayscaleColors(Texture2D source)
        {
            // get source pixels
            Color[] pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);

            // grayscale pixel colors
            for (int i = 0; i < pixels.Length; i++)
            {
                Color color = pixels[i];
                if (color.A == 0)
                    continue; // not transparent

                int grayscale = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                pixels[i] = new Color(grayscale, grayscale, grayscale, color.A);
            }

            // create new texture
            Texture2D target = new Texture2D(source.GraphicsDevice, source.Width, source.Height);
            target.SetData(pixels);
            return target;
        }
    }
}
