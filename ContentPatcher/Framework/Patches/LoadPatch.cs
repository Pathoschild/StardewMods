using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for an asset that should be replaced with a content pack file.</summary>
    internal class LoadPatch : Patch
    {
        /*********
        ** Properties
        *********/
        /// <summary>The asset key to load from the content pack instead.</summary>
        public TokenString LocalAsset { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="localAsset">The asset key to load from the content pack instead.</param>
        /// <param name="normaliseAssetName">Normalise an asset name.</param>
        public LoadPatch(string logName, ManagedContentPack contentPack, TokenString assetName, ConditionDictionary conditions, TokenString localAsset, Func<string, string> normaliseAssetName)
            : base(logName, PatchType.Load, contentPack, assetName, conditions, normaliseAssetName)
        {
            this.LocalAsset = localAsset;
        }

        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        public override bool UpdateContext(ConditionContext context)
        {
            bool localAssetChanged = this.LocalAsset.UpdateContext(context);
            return base.UpdateContext(context) || localAssetChanged;
        }

        /// <summary>Load the initial version of the asset.</summary>
        /// <param name="asset">The asset to load.</param>
        public override T Load<T>(IAssetInfo asset)
        {
            T data = this.ContentPack.Load<T>(this.LocalAsset.Value);
            return (data as object) is Texture2D texture
                ? (T)(object)this.CloneTexture(texture)
                : data;
        }

        /// <summary>Get the condition tokens used by this patch in its fields.</summary>
        public override IEnumerable<ConditionKey> GetTokensUsed()
        {
            return base.GetTokensUsed().Union(this.LocalAsset.ConditionTokens);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Clone a texture.</summary>
        /// <param name="texture">The texture to clone.</param>
        /// <returns>Cloning a texture is necessary when loading to avoid having it shared between different content managers, which can lead to undesirable effects like two players having synchronised texture changes.</returns>
        private Texture2D CloneTexture(Texture2D texture)
        {
            Texture2D clone = new Texture2D(Game1.graphics.GraphicsDevice, texture.Width, texture.Height);
            byte[] data = new byte[texture.Width * texture.Height];
            texture.GetData(data);
            clone.SetData(data);
            return clone;
        }
    }
}
