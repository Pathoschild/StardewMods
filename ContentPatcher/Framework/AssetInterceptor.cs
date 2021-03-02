using ContentPatcher.Framework.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Handles asset interception to apply Content Patcher's changes.</summary>
    internal class AssetInterceptor : IAssetLoader, IAssetEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages state for each screen.</summary>
        private readonly PerScreen<ScreenManager> ScreenManager;

        /// <summary>Manages loaded patches.</summary>
        private PatchManager PatchManager => this.ScreenManager.Value.PatchManager;

        /// <summary>Handles loading custom location data and adding it to the game.</summary>
        private CustomLocationManager CustomLocationManager => this.ScreenManager.Value.CustomLocationManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="screenManager">Manages state for each screen.</param>
        public AssetInterceptor(PerScreen<ScreenManager> screenManager)
        {
            this.ScreenManager = screenManager;
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return
                this.PatchManager.CanLoad<T>(asset)
                || this.CustomLocationManager.CanLoad<T>(asset);
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return this.PatchManager.CanEdit<T>(asset);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return this.CustomLocationManager.CanLoad<T>(asset)
                ? this.CustomLocationManager.Load<T>(asset)
                : this.PatchManager.Load<T>(asset);
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            this.PatchManager.Edit<T>(asset);
        }
    }
}
