namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
{
    /// <summary>Metadata about an input ingredient for a Producer Framework Mod machine.</summary>
    internal class ProducerFrameworkIngredient
    {
        /// <summary>The ID for the input ingredient, or <c>null</c> for a context tag ingredient.</summary>
        public int? InputId { get; set; }

        /// <summary>The number of the ingredient needed.</summary>
        public int Count { get; set; }
    }
}
