namespace Pathoschild.LookupAnything.Framework.Data
{
    /// <summary>Indicates the sprite sheet used to draw an object. A given sprite ID can be duplicated between two sprite sheets.</summary>
    internal enum ObjectSpriteSheet
    {
        /// <summary>The object sprite sheet, used to draw most game objects.</summary>
        Object,

        /// <summary>The 'big craftables' sprite sheet, used for some crafting objects like the egg incubator.</summary>
        BigCraftable
    }
}