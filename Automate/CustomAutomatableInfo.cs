namespace Pathoschild.Stardew.Automate
{
    /// <summary>
    /// Provides information about an automatable type (typically an ad-hoc "machine", such as
    /// <see cref="Framework.Machines.Tiles.TrashCanMachine"/>) that does not correspond to an addressable game object.
    /// </summary>
    /// <remarks>
    /// Can be used as the <see cref="IAutomatable.Instance"/> when no other type would be applicable.
    /// </remarks>
    public interface ICustomAutomatableInfo
    {
        /// <summary>
        /// A unique (per save) identifier for this instance.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Describes the kind of automation performed. Can be the name of the <see cref="System.Type"/> or any other descriptive string.
        /// </summary>
        string Kind { get; set; }
    }

    /// <summary>
    /// Holds the data for an <see cref="ICustomAutomatableInfo"/> used in the API.
    /// </summary>
    internal record CustomAutomatableInfo(string Id, string Kind);
}
