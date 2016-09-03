namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>The input mapping configuration.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    internal class InputMapConfiguration<T>
    {
        /// <summary>The control which toggles the lookup UI.</summary>
        public T Toggle { get; set; }
    }
}