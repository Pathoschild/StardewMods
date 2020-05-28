using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the load game menu animations.</summary>
    /// <remarks>See game logic in <see cref="LoadGameMenu"/>.</remarks>
    internal class LoadGameMenuHandler : BaseAnimationHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public LoadGameMenuHandler(float multiplier, IReflectionHelper reflection)
            : base(multiplier)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.activeClickableMenu is TitleMenu
                && this.Reflection.GetField<IClickableMenu>(typeof(TitleMenu), "_subMenu").GetValue() is LoadGameMenu loadGameMenu
                && this.GetTimerToLoad(loadGameMenu).GetValue() > 0;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            LoadGameMenu menu = (LoadGameMenu)this.Reflection.GetField<IClickableMenu>(typeof(TitleMenu), "_subMenu").GetValue();
            IReflectedField<int> timerToLoad = this.GetTimerToLoad(menu);

            this.ApplySkips(
                run: () => menu.update(Game1.currentGameTime),
                until: () => timerToLoad.GetValue() <= 0
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the protected load game menu field which indicates whether it's currently counting down before loading a save.</summary>
        /// <param name="menu">The load game menu.</param>
        private IReflectedField<int> GetTimerToLoad(LoadGameMenu menu)
        {
            return this.Reflection.GetField<int>(menu, "timerToLoad");
        }
    }
}
