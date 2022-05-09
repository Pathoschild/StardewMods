using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>Summarizes details about the friendship between an NPC and a player.</summary>
    internal record FriendshipModel
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Flags
        ****/
        /// <summary>Whether the player can date the NPC.</summary>
        public bool CanDate { get; }

        /// <summary>Whether the NPC is dating the player.</summary>
        public bool IsDating { get; }

        /// <summary>Whether the NPC is married to the player.</summary>
        public bool IsSpouse { get; }

        /// <summary>Whether the NPC is the player's housemate.</summary>
        public bool IsHousemate { get; }

        /// <summary>Whether the NPC is divorced from the player.</summary>
        public bool IsDivorced { get; }

        /// <summary>Whether the NPC has a stardrop to give to the player once they reach enough points.</summary>
        public bool HasStardrop { get; }

        /// <summary>Whether the player talked to them today.</summary>
        public bool TalkedToday { get; }

        /// <summary>The number of gifts the player gave the NPC today.</summary>
        public int GiftsToday { get; }

        /// <summary>The number of gifts the player gave the NPC this week.</summary>
        public int GiftsThisWeek { get; }

        /// <summary>The current friendship status.</summary>
        public FriendshipStatus Status { get; }

        /****
        ** Points
        ****/
        /// <summary>The player's current friendship points with the NPC.</summary>
        public int Points { get; }

        /// <summary>The number of friendship points needed to obtain a stardrop (if applicable).</summary>
        public int? StardropPoints { get; }

        /// <summary>The maximum number of points which the player can currently reach with an NPC.</summary>
        public int MaxPoints { get; }

        /// <summary>The number of points per heart level.</summary>
        public int PointsPerLevel { get; }


        /****
        ** Hearts
        ****/
        /// <summary>The number of filled hearts in their friendship meter.</summary>
        public int FilledHearts { get; }

        /// <summary>The number of empty hearts in their friendship meter.</summary>
        public int EmptyHearts { get; }

        /// <summary>The number of locked hearts in their friendship meter.</summary>
        public int LockedHearts { get; }

        /// <summary>The total number of hearts that can be unlocked with this NPC.</summary>
        public int TotalHearts => this.FilledHearts + this.EmptyHearts + this.LockedHearts;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="player">The player.</param>
        /// <param name="npc">The NPC.</param>
        /// <param name="constants">The constant assumptions.</param>
        /// <param name="friendship">The current friendship data.</param>
        public FriendshipModel(SFarmer player, NPC npc, Friendship friendship, ConstantData constants)
        {
            bool marriedOrRoommate = friendship.IsMarried();
            bool roommate = friendship.IsRoommate();

            // flags
            this.CanDate = npc.datable.Value;
            this.IsDating = friendship.IsDating();
            this.IsSpouse = marriedOrRoommate && !roommate;
            this.IsHousemate = marriedOrRoommate && roommate;
            this.IsDivorced = friendship.IsDivorced();
            this.Status = friendship.Status;
            this.TalkedToday = friendship.TalkedToToday;
            this.GiftsToday = friendship.GiftsToday;
            this.GiftsThisWeek = friendship.GiftsThisWeek;

            // points
            this.MaxPoints = this.IsSpouse || this.IsHousemate ? constants.SpouseMaxFriendship : NPC.maxFriendshipPoints;
            this.Points = friendship.Points;
            this.PointsPerLevel = NPC.friendshipPointsPerHeartLevel;
            this.FilledHearts = this.Points / NPC.friendshipPointsPerHeartLevel;
            this.LockedHearts = this.CanDate && !this.IsDating ? constants.DatingHearts : 0;
            this.EmptyHearts = this.MaxPoints / NPC.friendshipPointsPerHeartLevel - this.FilledHearts - this.LockedHearts;
            if (this.IsSpouse || this.IsHousemate)
            {
                this.StardropPoints = constants.SpouseFriendshipForStardrop;
                this.HasStardrop = !player.mailReceived.Contains(Constant.MailLetters.ReceivedSpouseStardrop);
            }
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="points">The player's current friendship points with the NPC.</param>
        /// <param name="pointsPerLevel">The number of points per heart level.</param>
        /// <param name="maxPoints">The maximum number of points which the player can currently reach with an NPC.</param>
        public FriendshipModel(int points, int pointsPerLevel, int maxPoints)
        {
            this.Points = points;
            this.PointsPerLevel = pointsPerLevel;
            this.MaxPoints = maxPoints;
            this.FilledHearts = this.Points / pointsPerLevel;
            this.EmptyHearts = this.MaxPoints / pointsPerLevel - this.FilledHearts;
        }

        /// <summary>Get the number of points to the next heart level or stardrop.</summary>
        public int GetPointsToNext()
        {
            if (this.Points < this.MaxPoints)
                return this.PointsPerLevel - (this.Points % this.PointsPerLevel);
            if (this.StardropPoints.HasValue && this.Points < this.StardropPoints)
                return this.StardropPoints.Value - this.Points;
            return 0;
        }
    }
}
