namespace Pathoschild.LookupAnything
{
    /// <summary>A utility class for writing text output.</summary>
    internal class GrammarHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Select the correct plural form for a word.</summary>
        /// <param name="count">The number.</param>
        /// <param name="single">The singular form.</param>
        /// <param name="plural">The plural form.</param>
        public static string Pluralise(int count, string single, string plural = null)
        {
            return count == 1 ? single : (plural ?? single + "s");
        }
    }
}
