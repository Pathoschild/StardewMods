using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a farm animal.</summary>
    internal class FarmAnimalSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The lookup target.</summary>
        private readonly FarmAnimal Target;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="animal">The lookup target.</param>
        /// <remarks>Reverse engineered from <see cref="FarmAnimal"/>.</remarks>
        public FarmAnimalSubject(FarmAnimal animal)
            : base(animal.name, null, animal.type)
        {
            this.Target = animal;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            FarmAnimal animal = this.Target;

            // calculate maturity
            bool isFullyGrown = animal.age >= animal.ageWhenMature;
            int daysUntilGrown = 0;
            Tuple<string, int> dayOfMaturity = null;
            if (!isFullyGrown)
            {
                daysUntilGrown = animal.ageWhenMature - animal.age;
                dayOfMaturity = GameHelper.GetDayOffset(daysUntilGrown, metadata.Constants.DaysInSeason);
            }

            // yield fields
            yield return new CharacterFriendshipField("Love", DataParser.GetFriendshipForAnimal(Game1.player, animal, metadata));
            yield return new PercentageBarField("Happiness", animal.happiness, byte.MaxValue, Color.Green, Color.Gray, $"{Math.Round(animal.happiness / (metadata.Constants.AnimalMaxHappiness * 1f) * 100)}%");
            yield return new GenericField("Mood today", animal.getMoodMessage());
            yield return new GenericField("Complaints", this.GetMoodReason(animal));
            yield return new ItemIconField("Produce ready", animal.currentProduce > 0 ? new StardewValley.Object(animal.currentProduce, 1) : null);
            if (!isFullyGrown)
                yield return new GenericField("Growth", $"{daysUntilGrown} {GameHelper.Pluralise(daysUntilGrown, "day")} (on {dayOfMaturity.Item1} {dayOfMaturity.Item2})");
            yield return new SaleValueField("Sells for", animal.getSellPrice(), 1);
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            FarmAnimal animal = this.Target;
            animal.Sprite.draw(spriteBatch, position, 1, 0, 0, Color.White, scale: size.X / animal.Sprite.getWidth());
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a short explanation for the animal's current mod.</summary>
        /// <param name="animal">The farm animal.</param>
        private string GetMoodReason(FarmAnimal animal)
        {
            List<string> factors = new List<string>();

            // winter without heat
            if (Game1.IsWinter && Game1.currentLocation.numberOfObjectsWithName(Constant.ItemNames.Heater) <= 0)
                factors.Add("no heater in winter");

            // mood
            switch (animal.moodMessage)
            {
                case FarmAnimal.newHome:
                    factors.Add("moved into new home");
                    break;
                case FarmAnimal.hungry:
                    factors.Add("wasn't fed yesterday");
                    break;
                case FarmAnimal.disturbedByDog:
                    factors.Add($"was disturbed by {Game1.player.getPetName()}");
                    break;
                case FarmAnimal.leftOutAtNight:
                    factors.Add("was left outside last night");
                    break;
            }

            // not pet
            if (!animal.wasPet)
                factors.Add("hasn't been petted today");

            // return factors
            return string.Join(", ", factors);
        }
    }
}