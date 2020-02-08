using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Items.ItemData;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Provides subject entries for target values.</summary>
    internal class SubjectFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        private readonly Metadata Metadata;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Provides translations stored in the mod folder.</summary>
        private readonly ITranslationHelper Translations;

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;

        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>Provides methods for searching and constructing items.</summary>
        private readonly ItemRepository ItemRepository = new ItemRepository();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="config">The mod configuration.</param>
        public SubjectFactory(Metadata metadata, ITranslationHelper translations, IReflectionHelper reflection, GameHelper gameHelper, ModConfig config)
        {
            this.Metadata = metadata;
            this.Translations = translations;
            this.Reflection = reflection;
            this.GameHelper = gameHelper;
            this.GameHelper = gameHelper;
            this.Config = config;
        }

        /****
        ** Get subjects
        ****/
        /// <summary>Get an NPC subject.</summary>
        /// <param name="target">The target instance.</param>
        public ISubject GetCharacter(NPC target)
        {
            SubjectType type = this.GetSubjectType(target);
            return new CharacterSubject(this, this.GameHelper, target, type, this.Metadata, this.Translations, this.Reflection, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes);
        }

        /// <summary>Get a player subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="isLoadMenu">Whether this is being displayed on the load menu, before the save data is fully initialized.</param>
        public ISubject GetPlayer(Farmer target, bool isLoadMenu = false)
        {
            return new FarmerSubject(this, this.GameHelper, target, this.Translations, isLoadMenu);
        }

        /// <summary>Get a farm animal subject.</summary>
        /// <param name="target">The target instance.</param>
        public ISubject GetFarmAnimal(FarmAnimal target)
        {
            return new FarmAnimalSubject(this, this.GameHelper, target, this.Translations);
        }

        /// <summary>Get a crop subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="context">The context of the object being looked up.</param>
        public ISubject GetCrop(Crop target, ObjectContext context)
        {
            return new ItemSubject(this, this.GameHelper, this.Translations, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes, this.GameHelper.GetObjectBySpriteIndex(target.indexOfHarvest.Value), context, knownQuality: false, fromCrop: target);
        }

        /// <summary>Get a fruit tree subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="tile">The tree's tile position.</param>
        public ISubject GetFruitTree(FruitTree target, Vector2 tile)
        {
            return new FruitTreeSubject(this, this.GameHelper, target, tile, this.Translations);
        }

        /// <summary>Get a wild tree subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="tile">The tree's tile position.</param>
        public ISubject GetWildTree(Tree target, Vector2 tile)
        {
            return new TreeSubject(this, this.GameHelper, target, tile, this.Translations);
        }

        /// <summary>Get an item subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        public ISubject GetItem(Item target, ObjectContext context, bool knownQuality = true)
        {
            return new ItemSubject(this, this.GameHelper, this.Translations, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes, target, context, knownQuality);
        }

        /// <summary>Get a movie concession subject.</summary>
        /// <param name="target">The target instance.</param>
        public ISubject GetMovieSnack(MovieConcession target)
        {
            return new MovieSnackSubject(this, this.GameHelper, this.Translations, target);
        }

        /// <summary>Get a building subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="sourceRectangle">The building's source rectangle in its spritesheet.</param>
        public ISubject GetBuilding(Building target, Rectangle sourceRectangle)
        {
            return new BuildingSubject(this, this.GameHelper, target, sourceRectangle, this.Translations);
        }

        /// <summary>Get a bush subject.</summary>
        /// <param name="target">The target instance.</param>
        public ISubject GetBush(Bush target)
        {
            return new BushSubject(this, this.GameHelper, target, this.Translations, this.Reflection);
        }

        /// <summary>Get a bush subject.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        public ISubject GetTile(GameLocation location, Vector2 position)
        {
            return new TileSubject(this, this.GameHelper, location, position, this.Translations);
        }

        /****
        ** Get metadata
        ****/
        /// <summary>Get the subject type for an NPC.</summary>
        /// <param name="npc">The NPC instance.</param>
        public SubjectType GetSubjectType(NPC npc)
        {
            if (npc.isVillager())
                return SubjectType.Villager;

            return npc switch
            {
                Child _ => SubjectType.Villager,
                Horse _ => SubjectType.Horse,
                Junimo _ => SubjectType.Junimo,
                Pet _ => SubjectType.Pet,
                Monster _ => SubjectType.Monster,
                _ => SubjectType.Unknown
            };
        }

        /****
        ** Search
        ****/
        /// <summary>Get the subjects available for searching.</summary>
        /// <remarks>Related to <see cref="TargetFactory.GetNearbyTargets"/>.</remarks>
        public IEnumerable<ISubject> GetSearchSubjects()
        {
            // NPCs
            foreach (NPC npc in Utility.getAllCharacters())
                yield return this.GetCharacter(npc);

            // animals
            foreach (var location in CommonHelper.GetLocations())
            {
                IEnumerable<FarmAnimal> animals =
                    (location as Farm)?.animals.Values
                    ?? (location as AnimalHouse)?.animals.Values;

                if (animals != null)
                {
                    foreach (var animal in animals)
                        yield return this.GetFarmAnimal(animal);
                }
            }

            // items
            foreach (SearchableItem item in this.ItemRepository.GetAll())
                yield return this.GetItem(item.Item, ObjectContext.World, knownQuality: false);

            // players
            foreach (Farmer player in Game1.getAllFarmers())
                yield return this.GetPlayer(player);
        }
    }
}
