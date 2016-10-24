using System.Text;

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

        /// <summary>Get a human-readable list of values.</summary>
        /// <param name="values">The values to print.</param>
        public static string OrList(string[] values)
        {
            values = values ?? new string[0];
            switch (values.Length)
            {
                case 0:
                    return string.Empty;

                case 1:
                    return values[0];

                case 2:
                    return $"{values[0]} or {values[1]}";

                default:
                    StringBuilder list = new StringBuilder();
                    list.Append(values[0]);
                    for (int i = 1; i < values.Length - 1; i++)
                        list.Append($", {values[i]}");
                    if (values.Length != 1)
                        list.Append($", or {values[values.Length - 1]}");

                    return list.ToString();
            }
        }
    }
}
