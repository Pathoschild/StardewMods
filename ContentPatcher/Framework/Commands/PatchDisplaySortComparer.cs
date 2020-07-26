using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>A comparer which sorts patches for a content pack by their display order.</summary>
    internal class PatchDisplaySortComparer : IComparer<PatchInfo>
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public int Compare(PatchInfo left, PatchInfo right)
        {
            // equivalent
            if (object.ReferenceEquals(left, right) || left?.Path == null || right?.Path == null || string.Equals(left.Path.ToString(), right.Path.ToString()))
                return 0;

            // sort by path segments
            string[] leftParts = this.GetComparablePathSegments(left);
            string[] rightParts = this.GetComparablePathSegments(right);
            for (int i = 0; i < leftParts.Length && i < rightParts.Length; i++)
            {
                int sort = string.Compare(leftParts[i], rightParts[i], StringComparison.OrdinalIgnoreCase);
                if (sort != 0)
                    return sort;
            }

            // one is the ancestor of the other, so sort the higher-level one first
            return leftParts.Length.CompareTo(rightParts.Length);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the patch's path segments adjusted to allow sorting <see cref="PatchType.Include"/> patches before their included patches.</summary>
        /// <param name="patch">The patch for which to get path segments.</param>
        private string[] GetComparablePathSegments(PatchInfo patch)
        {
            LogPathBuilder path = patch.Path;

            // for an 'Include' patch, replace the patch name with the 'from' asset path to sort included patches under their include patch
            if (patch.ParsedType == PatchType.Include)
            {
                string includeFrom = patch.ParsedFromAsset?.Value ?? patch.RawFromAsset;
                if (includeFrom != null)
                    path = new LogPathBuilder(path.Segments.Take(path.Segments.Length - 1)).With(PathUtilities.NormalizePathSeparators(includeFrom));
            }

            return path.Segments;
        }
    }
}
