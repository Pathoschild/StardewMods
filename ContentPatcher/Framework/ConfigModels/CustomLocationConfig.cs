namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A custom location to add to the game.</summary>
    internal class CustomLocationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique location name.</summary>
        public string Name { get; set; }

        /// <summary>The initial map file to load.</summary>
        public string FromMapFile { get; set; }
    }
}
