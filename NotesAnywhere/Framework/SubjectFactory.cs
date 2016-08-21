using Pathoschild.NotesAnywhere.Framework.Subjects;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.NotesAnywhere.Framework
{
    /// <summary>A factory which extracts metadata from arbitrary objects.</summary>
    public class SubjectFactory
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="obj">The underlying object.</param>
        public ISubject GetSubject(Object obj)
        {
            return new ObjectSubject(obj);
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="terrainFeature">The underlying object.</param>
        public ISubject GetSubject(TerrainFeature terrainFeature)
        {
            // crop
            if (terrainFeature is HoeDirt)
            {
                Crop crop = ((HoeDirt)terrainFeature).crop;
                if (crop == null)
                    return null;

                Object obj = new Object(crop.indexOfHarvest, 1);
                return new CropSubject(crop, obj);
            }

            return null;
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="character">The underlying object.</param>
        public ISubject GetSubject(NPC character)
        {
            return new CharacterSubject(character);
        }
    }
}