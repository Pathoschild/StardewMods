using StardewValley;

namespace Pathoschild.NotesAnywhere.Framework.Subjects
{
    /// <summary>Describes a growing crop.</summary>
    public class CropSubject : ObjectSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying crop.</summary>
        private readonly Crop Crop;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="crop">The underlying crop.</param>
        /// <param name="obj">The underlying object.</param>
        public CropSubject(Crop crop, Object obj)
            : base(obj)
        {
            this.Crop = crop;
        }
    }
}