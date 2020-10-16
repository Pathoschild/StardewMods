using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>A central registry for subject lookups.</summary>
    internal interface ISubjectRegistry
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get the subject for an in-game entity, if available.</summary>
        /// <param name="entity">The entity instance.</param>
        ISubject GetByEntity(object entity);
    }
}
