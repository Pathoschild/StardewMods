using System;
using System.Collections.Generic;

namespace Common.Utilities
{
    /// <summary>A comparer which sorts values by comparing alphabetical and numeric sequences within the strings, resulting in a more intuitive order like "1, 2, 10" instead of "1, 10, 2".</summary>
    internal class HumanSortComparer : IComparer<string>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The comparer to use for alphabetical sequences.</summary>
        private readonly IComparer<string> AlphaComparer;


        /*********
        ** Accessors
        *********/
        /// <summary>A default instance which ignores letter case.</summary>
        public static readonly HumanSortComparer DefaultIgnoreCase = new HumanSortComparer(ignoreCase: true);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="ignoreCase">Whether to ignore letter case when sorting.</param>
        public HumanSortComparer(bool ignoreCase = false)
        {
            this.AlphaComparer = ignoreCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
        }

        /// <inheritdoc />
        public int Compare(string a, string b)
        {
            // no special sorting needed
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return this.AlphaComparer.Compare(a, b);

            // sort by sequence
            int indexA = 0;
            int indexB = 0;
            while (true)
            {
                // get next parts
                this.GetNextPart(a, ref indexA, out string rawA, out long? numericA);
                this.GetNextPart(b, ref indexB, out string rawB, out long? numericB);
                bool isNumeric = numericA.HasValue && numericB.HasValue;

                // null is less than any other value
                if (rawA == null && rawB == null)
                    return 0;
                if (rawA == null)
                    return -1;
                if (rawB == null)
                    return 1;

                // numeric with preceding zeros sort first (e.g. 01 < 1)
                if (isNumeric)
                {
                    for (int i = 0; i < rawA.Length && i < rawB.Length; i++)
                    {
                        bool zeroA = rawA[i] == '0';
                        bool zeroB = rawB[i] == '0';

                        if (zeroA && zeroB)
                            continue;
                        if (zeroA)
                            return -1;
                        if (zeroB)
                            return 1;

                        break;
                    }
                }

                // else compare alphanumerically
                int result = isNumeric
                    ? numericA.Value.CompareTo(numericB.Value)
                    : this.AlphaComparer.Compare(rawA, rawB);

                // normalize return value
                if (result < 0)
                    return -1;
                if (result > 0)
                    return 1;
            }
        }


        /*********
        ** Private
        *********/
        /// <summary>Get the next sequence of characters in a string consisting entirely of numeric or non-numeric characters.</summary>
        /// <param name="str">The string to scan.</param>
        /// <param name="position">The next position in the string.</param>
        /// <param name="raw">The raw sequence value.</param>
        /// <param name="numeric">The numeric sequence, if applicable.</param>
        private void GetNextPart(string str, ref int position, out string raw, out long? numeric)
        {
            // sequence over
            if (position >= str.Length)
            {
                raw = null;
                numeric = null;
                return;
            }

            // scan ahead
            int start = position;
            bool isNumeric = char.IsNumber(str[start]);
            for (position += 1; position < str.Length && char.IsNumber(str[position]) == isNumeric; position++)
                ;

            // read sequence
            raw = str.Substring(start, position - start);
            numeric = isNumeric && long.TryParse(raw, out long parsedNumeric)
                ? parsedNumeric
                : null;
        }
    }
}
