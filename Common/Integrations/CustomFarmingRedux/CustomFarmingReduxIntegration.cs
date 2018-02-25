using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux
{
    /// <summary>Handles the logic for integrating with the Custom Farming Redux mod.</summary>
    internal class CustomFarmingReduxIntegration : BaseIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod's public API.</summary>
        private readonly ICustomFarmingApi ModApi;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public CustomFarmingReduxIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Custom Farming", "Platonymous.CustomFarming", "2.3.6", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;

            // get mod API
            this.ModApi = this.GetValidatedApi<ICustomFarmingApi>();
            this.IsLoaded = this.ModApi != null;
        }

        /// <summary>Get whether an object is a Custom Farming custom object.</summary>
        /// <param name="obj">The custom object.</param>
        public bool IsCustomObject(SObject obj)
        {
            this.AssertLoaded();

            return this.ModApi.isCustom(obj);
        }

        /// <summary>Get the sprite info for a custom object, or <c>null</c> if the object isn't custom.</summary>
        /// <param name="obj">The custom object.</param>
        public CustomSprite GetTexture(SObject obj)
        {
            this.AssertLoaded();

            if (!this.ModApi.isCustom(obj))
                return null;

            Texture2D texture = this.ModApi.getSpritesheet(obj);
            if (texture == null)
                throw new InvalidOperationException($"Custom Farming Redux says object '{obj.Name}' is a custom object or machine, but returned a null texture for it.");
            Rectangle? sourceRectangle = this.ModApi.getSpriteSourceArea(obj);
            if (sourceRectangle == null)
                throw new InvalidOperationException($"Custom Farming Redux says object '{obj.Name}' is a custom object or machine, but returned a null sprite source rectangle for it.");

            return new CustomSprite(texture, sourceRectangle.Value);
        }
    }
}
