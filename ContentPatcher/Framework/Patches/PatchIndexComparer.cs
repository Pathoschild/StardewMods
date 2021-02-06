using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A comparer which sorts patches by their global index within the content pack's <c>content.json</c>.</summary>
    internal class PatchIndexComparer : IComparer<IPatch>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A singleton patch index comparer.</summary>
        public static readonly PatchIndexComparer Instance = new();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public int Compare(IPatch leftPatch, IPatch rightPatch)
        {
            int[] left = leftPatch?.IndexPath;
            int[] right = rightPatch?.IndexPath;

            // handle null just in case
            if (left is null || right is null)
                return (left is null).CompareTo(right is null);

            // sort by the first different value in the index paths
            int maxLen = Math.Min(left.Length, right.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int result = left[i].CompareTo(right[i]);
                if (result != 0)
                    return result;
            }

            // if all matched elements are the same, longer array sorts last
            return left.Length.CompareTo(right.Length);
        }
    }
}
