using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataMaps.Framework.Integrations
{
    /// <summary>Handles the logic for integrating with the Pelican Fiber mod.</summary>
    internal class PelicanFiberIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>An API for accessing private code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the Pelican Fiber is installed.</summary>
        public bool IsLoaded { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="reflection">An API for accessing private code.</param>
        public PelicanFiberIntegration(IModRegistry modRegistry, IReflectionHelper reflection)
        {
            this.IsLoaded = modRegistry.IsLoaded("jwdred.PelicanFiber");
            this.Reflection = reflection;
        }

        /// <summary>Get whether the Pelican Fiber build menu is open.</summary>
        public bool IsBuildMenuOpen()
        {
            return this.IsLoaded && Game1.activeClickableMenu?.GetType().FullName == "PelicanFiber.Framework.ConstructionMenu";
        }

        /// <summary>Get the selected blueprint from the Pelican Fiber build menu, if it's open.</summary>
        public BluePrint GetBuildMenuBlueprint()
        {
            if (!this.IsBuildMenuOpen())
                return null;

            return this.Reflection.GetPrivateProperty<BluePrint>(Game1.activeClickableMenu, "CurrentBlueprint").GetValue();
        }
    }
}
