using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

/// <summary>Describes a farm animal.</summary>
internal class FarmAnimalSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The lookup target.</summary>
    private readonly FarmAnimal Target;

    /// <summary>Provides subject entries.</summary>
    private readonly ISubjectRegistry Codex;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="codex">Provides subject entries.</param>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="animal">The lookup target.</param>
    /// <remarks>Reverse engineered from <see cref="FarmAnimal"/>.</remarks>
    public FarmAnimalSubject(ISubjectRegistry codex, GameHelper gameHelper, FarmAnimal animal)
        : base(gameHelper, animal.displayName, null, animal.displayType)
    {
        this.Codex = codex;
        this.Target = animal;
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        FarmAnimal animal = this.Target;

        // calculate maturity
        bool isFullyGrown = animal.isAdult();
        int daysUntilGrown = 0;
        SDate? dayOfMaturity = null;
        if (!isFullyGrown)
        {
            daysUntilGrown = animal.GetAnimalData().DaysToMature - animal.age.Value;
            dayOfMaturity = SDate.Now().AddDays(daysUntilGrown);
        }

        // added by mod
        {
            IModInfo? fromMod = this.GameHelper.TryGetModFromStringId(animal.type.Value);
            if (fromMod != null)
                yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(modName: fromMod.Manifest.Name));
        }

        // yield fields
        yield return new CharacterFriendshipField(I18n.Animal_Love(), this.GameHelper.GetFriendshipForAnimal(Game1.player, animal));
        yield return new PercentageBarField(I18n.Animal_Happiness(), animal.happiness.Value, byte.MaxValue, Color.Green, Color.Gray, I18n.Generic_Percent(percent: (int)Math.Round(animal.happiness.Value / (this.Constants.AnimalMaxHappiness * 1f) * 100)));
        yield return new GenericField(I18n.Animal_Mood(), animal.getMoodMessage());
        yield return new GenericField(I18n.Animal_Complaints(), this.GetMoodReason(animal));
        yield return new ItemIconField(this.GameHelper, I18n.Animal_ProduceReady(), CommonHelper.IsItemId(animal.currentProduce.Value, allowZero: false) ? ItemRegistry.Create(animal.currentProduce.Value) : null, this.Codex);
        if (!isFullyGrown)
            yield return new GenericField(I18n.Animal_Growth(), $"{I18n.Generic_Days(count: daysUntilGrown)} ({this.Stringify(dayOfMaturity)})");
        yield return new GenericField(I18n.Animal_SellsFor(), GenericField.GetSaleValueString(animal.getSellPrice(), 1));

        // internal type
        yield return new GenericField(I18n.InternalId(), animal.type.Value);
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        FarmAnimal target = this.Target;

        // pinned fields
        yield return new GenericDebugField("age", $"{target.age} days", pinned: true);
        yield return new GenericDebugField("friendship", $"{target.friendshipTowardFarmer} (max {this.Constants.AnimalMaxHappiness})", pinned: true);
        yield return new GenericDebugField("fullness", this.Stringify(target.fullness.Value), pinned: true);
        yield return new GenericDebugField("happiness", this.Stringify(target.happiness.Value), pinned: true);

        // raw fields
        foreach (IDebugField field in this.GetDebugFieldsFrom(target))
            yield return field;
    }

    /// <inheritdoc />
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
        List<string> factors = [];

        // winter without heat
        if (Game1.IsWinter && Game1.currentLocation.numberOfObjectsWithName(Constant.ItemNames.Heater) <= 0)
            factors.Add(I18n.Animal_Complaints_NoHeater());

        // mood
        switch (animal.moodMessage.Value)
        {
            case FarmAnimal.newHome:
                factors.Add(I18n.Animal_Complaints_NewHome());
                break;

            case FarmAnimal.hungry:
                factors.Add(I18n.Animal_Complaints_Hungry());
                break;

            case FarmAnimal.disturbedByDog:
                factors.Add(I18n.Animal_Complaints_WildAnimalAttack());
                break;

            case FarmAnimal.leftOutAtNight:
                factors.Add(I18n.Animal_Complaints_LeftOut());
                break;
        }

        // not pet
        if (!animal.wasPet.Value)
            factors.Add(I18n.Animal_Complaints_NotPetted());

        // return factors
        return I18n.List(factors);
    }
}
