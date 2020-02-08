using StardewModdingAPI;

namespace Pathoschild.Stardew.RotateToolbar.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which rotates the toolbar up (i.e. show the previous inventory row).</summary>
        public SButton[] ShiftToPrevious { get; }

        /// <summary>The key which rotates the toolbar up (i.e. show the next inventory row).</summary>
        public SButton[] ShiftToNext { get; }



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="shiftToPrevious">The key which rotates the toolbar up (i.e. show the previous inventory row).</param>
        /// <param name="shiftToNext">The key which rotates the toolbar up (i.e. show the next inventory row).</param>
        public ModConfigKeys(SButton[] shiftToPrevious, SButton[] shiftToNext)
        {
            this.ShiftToPrevious = shiftToPrevious;
            this.ShiftToNext = shiftToNext;
        }
    }
}
