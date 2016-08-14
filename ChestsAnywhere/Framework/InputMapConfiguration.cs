namespace ChestsAnywhere.Framework
{
    /// <summary>The input mapping configuration.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    internal class InputMapConfiguration<T>
    {
        /// <summary>The control which toggles the chest UI.</summary>
        public T Toggle { get; set; }

        /// <summary>The control which navigates to the previous chest.</summary>
        public T PrevChest { get; set; }

        /// <summary>The control which navigates to the next chest.</summary>
        public T NextChest { get; set; }

        /// <summary>The control which sorts items in the chest.</summary>
        public T SortItems { get; set; }
    }
}