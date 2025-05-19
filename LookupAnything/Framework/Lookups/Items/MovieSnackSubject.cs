using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

/// <summary>Describes a movie snack.</summary>
internal class MovieSnackSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The lookup target.</summary>
    private readonly MovieConcession Target;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="item">The underlying target.</param>
    public MovieSnackSubject(GameHelper gameHelper, MovieConcession item)
        : base(gameHelper)
    {
        this.Target = item;
        this.Initialize(item.DisplayName, item.getDescription(), I18n.Type_Other());
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        MovieConcession item = this.Target;

        // added by mod
        {
            IModInfo? fromMod = this.GameHelper.TryGetModFromStringId(item.Id);
            if (fromMod != null)
                yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(modName: fromMod.Manifest.Name));
        }

        // date's taste
        NPC? date = Game1.player.team.movieInvitations.FirstOrDefault(p => p.farmer == Game1.player)?.invitedNPC;
        if (date != null)
        {
            string taste = MovieTheater.GetConcessionTasteForCharacter(date, item);
            yield return new GenericField(I18n.Item_MovieSnackPreference(), I18n.ForMovieTasteLabel(taste, date.displayName));
        }

        // internal ID
        yield return new GenericField(I18n.InternalId(), I18n.Item_InternalId_Summary(itemId: item.Id, qualifiedItemId: item.QualifiedItemId));
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        foreach (IDebugField field in this.GetDebugFieldsFrom(this.Target))
            yield return field;
    }

    /// <inheritdoc />
    public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
    {
        this.Target.drawInMenu(spriteBatch, position, 1, 1f, 1f, StackDrawType.Hide, Color.White, false);
        return true;
    }
}
