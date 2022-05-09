using System;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides extension methods for objects.</summary>
    internal static class ObjectExtensions
    {
        /// <summary>Throw an exception if the given value is null.</summary>
        /// <param name="value">The value to test.</param>
        /// <param name="paramName">The name of the parameter being tested.</param>
        /// <returns>Returns the value if non-null.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        public static T AssertNotNull<T>(this T? value, string? paramName = null)
            where T : class
        {
            if (value is null)
                throw new ArgumentNullException(paramName);

            return value;
        }
    }
}
