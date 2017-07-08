using System.Linq;

namespace Pathoschild.Stardew.RotateToolbar.Framework
{
    /// <summary>The input mapping configuration.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    internal class InputMapConfiguration<T>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The control which rotates the toolbar up (i.e. show the previous inventory row).</summary>
        public T ShiftToPrevious { get; set; }

        /// <summary>The control which rotates the toolbar up (i.e. show the next inventory row).</summary>
        public T ShiftToNext { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the specified key is valid.</summary>
        /// <param name="key">The key to check.</param>
        public bool IsValidKey(T key)
        {
            return key != null && !key.Equals(default(T));
        }

        /// <summary>Get whether any keys are configured.</summary>
        public bool HasAny()
        {
            return new[] { this.ShiftToPrevious, this.ShiftToNext }.Any(this.IsValidKey);
        }
    }
}
