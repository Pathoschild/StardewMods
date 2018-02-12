using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations;
using StardewModdingAPI;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Integrations.CustomFarmingRedux
{
    /// <summary>Handles the logic for integrating with the Custom Farming Redux mod.</summary>
    internal class CustomFarmingReduxIntegration : BaseIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>The <see cref="Type.FullName"/> values for Custom Farming's custom objects.</summary>
        private readonly HashSet<string> CustomTypeNames = new HashSet<string> { "CustomFarmingRedux.CustomObject", "CustomFarmingRedux.CustomMachine" };

        /// <summary>The name of the spritesheet field on a custom object.</summary>
        private readonly string SpritesheetFieldName = "texture";

        /// <summary>The name of the source rectangle field on a custom object.</summary>
        private readonly string SourceRectangleFieldName = "sourceRectangle";

        /// <summary>Provides an API for accessing inaccessible code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Provides an API for accessing inaccessible code.</param>
        public CustomFarmingReduxIntegration(IModRegistry modRegistry, IMonitor monitor, IReflectionHelper reflection)
            : base("Custom Farming", "Platonymous.CustomFarming", "2.3.5", modRegistry, monitor)
        {
            this.Reflection = reflection;

            // validate custom object type & texture
            if (this.IsLoaded)
            {
                Type type = Type.GetType("CustomFarmingRedux.CustomObject, CustomFarmingRedux");
                if (type == null)
                {
                    this.IsLoaded = false;
                    monitor.Log($"Detected {this.Label}, but couldn't access the custom object type. Disabled integration with this mod.", LogLevel.Warn);
                    return;
                }

                // validate spritesheet field
                {
                    FieldInfo field = type.GetField(this.SpritesheetFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null)
                    {
                        this.IsLoaded = false;
                        monitor.Log($"Detected {this.Label}, but couldn't access the custom object type's {this.SpritesheetFieldName} field. Disabled integration with this mod.", LogLevel.Warn);
                        return;
                    }
                }

                // validate source rectangle field
                {
                    FieldInfo field = type.GetField(this.SourceRectangleFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null)
                    {
                        this.IsLoaded = false;
                        monitor.Log($"Detected {this.Label}, but couldn't access the custom object type's {this.SourceRectangleFieldName} field. Disabled integration with this mod.", LogLevel.Warn);
                        return;
                    }
                }
            }
        }

        /// <summary>Get whether an object is a Custom Farming custom object.</summary>
        /// <param name="obj">The custom object.</param>
        public bool IsCustomObject(SObject obj)
        {
            if (obj == null)
                return false;

            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                if (this.CustomTypeNames.Contains(type.FullName))
                    return true;
            }

            return false;
        }

        /// <summary>Get the sprite info for a custom object, or <c>null</c> if the object isn't custom.</summary>
        /// <param name="obj">The custom object.</param>
        public CustomSprite GetTexture(SObject obj)
        {
            if (!this.IsCustomObject(obj))
                return null;

            Texture2D texture = this.Reflection.GetField<Texture2D>(obj, this.SpritesheetFieldName).GetValue();
            Rectangle sourceRectangle =
                this.Reflection.GetField<Rectangle>(obj, this.SourceRectangleFieldName, required: false)?.GetValue() // custom object has field
                ?? this.Reflection.GetProperty<Rectangle>(obj, this.SourceRectangleFieldName).GetValue(); // custom machine has property

            return new CustomSprite(texture, sourceRectangle);
        }
    }
}
