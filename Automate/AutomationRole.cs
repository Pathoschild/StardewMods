namespace Pathoschild.Stardew.Automate
{
    /// <summary>
    /// Describes the role of an <see cref="IAutomatable"/>.
    /// </summary>
    public enum AutomationRole
    {
        /// <summary>
        /// No role is specified. This may be the case for automation objects added by mods based on an older version of Automate that don't yet support this API.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// A container, such as a chest, from which items can be taken for processing.
        /// </summary>
        Container,

        /// <summary>
        /// A connector that passively links other <see cref="IAutomatable"/> objects together.
        /// </summary>
        Connector,

        /// <summary>
        /// A machine that processes items in a <see cref="Container"/>.
        /// </summary>
        Machine,
    }
}
