using System.Linq;

namespace ChestsAnywhere.Framework
{
    /// <summary>The input mapping configuration.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    internal class InputMapConfiguration<T>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The control which toggles the chest UI.</summary>
        public T Toggle { get; set; }

        /// <summary>The control which navigates to the previous chest.</summary>
        public T PrevChest { get; set; }

        /// <summary>The control which navigates to the next chest.</summary>
        public T NextChest { get; set; }
        
        /// <summary>The control which navigates to the previous category.</summary>
        public T PrevCategory { get; set; }

        /// <summary>The control which navigates to the next category.</summary>
        public T NextCategory { get; set; }

        /// <summary>The control which sorts items in the chest.</summary>
        public T SortItems { get; set; }


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
            return new[] { this.Toggle, this.PrevChest, this.NextChest, this.PrevCategory, this.NextCategory, this.SortItems }.Any(this.IsValidKey);
        }
    }
}