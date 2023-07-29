using System;
using System.Linq;
using Netcode;
using StardewValley;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>A quest model.</summary>
    internal class QuestModel
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether the quest needs the given item.</summary>
        private readonly Func<Item, bool> NeedsItemImpl;

        /// <summary>A singleton donate objective used for context tag matching.</summary>
        private static readonly DonateObjective DonateObjective = new();


        /*********
        ** Accessors
        *********/
        /// <summary>The display name for the quest.</summary>
        public string DisplayText { get; }



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="quest">The underlying quest.</param>
        public QuestModel(Quest quest)
        {
            this.DisplayText = quest.GetName();
            this.NeedsItemImpl = item => this.NeedsItem(quest, item);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="order">The underlying special order.</param>
        public QuestModel(SpecialOrder order)
        {
            this.DisplayText = order.GetName();
            this.NeedsItemImpl = item => this.NeedsItem(order, item);

        }

        /// <summary>Get whether the quest needs the given item.</summary>
        /// <param name="obj">The item to check.</param>
        public bool NeedsItem(SObject? obj)
        {
            return obj != null && this.NeedsItemImpl(obj);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a quest needs the given item.</summary>
        /// <param name="quest">The quest to check.</param>
        /// <param name="item">The item to check.</param>
        private bool NeedsItem(Quest quest, Item? item)
        {
            if (item is null)
                return false;

            switch (quest)
            {
                case CraftingQuest required:
                    return required.ItemId.Value == item.QualifiedItemId;

                case ItemDeliveryQuest required:
                    return required.ItemId.Value == item.QualifiedItemId;

                case ItemHarvestQuest required:
                    return required.ItemId.Value == item.QualifiedItemId;

                case LostItemQuest required:
                    return required.ItemId.Value == item.QualifiedItemId;

                case ResourceCollectionQuest required:
                    return required.ItemId.Value == item.QualifiedItemId;

                case SecretLostItemQuest required:
                    return required.ItemId.Value == item.QualifiedItemId;

                default:
                    return false;
            }
        }

        /// <summary>Get whether a special order needs the given item.</summary>
        /// <param name="order">The special order to check.</param>
        /// <param name="item">The item to check.</param>
        private bool NeedsItem(SpecialOrder order, Item? item)
        {
            return order.objectives
                .Any(objective =>
                {
                    switch (objective)
                    {
                        case CollectObjective required:
                            return this.IsMatch(item, required.acceptableContextTagSets);

                        case DeliverObjective required:
                            return this.IsMatch(item, required.acceptableContextTagSets);

                        case DonateObjective required:
                            return required.IsValidItem(item);

                        case FishObjective required:
                            return this.IsMatch(item, required.acceptableContextTagSets);

                        case GiftObjective required:
                            return this.IsMatch(item, required.acceptableContextTagSets);

                        case ShipObjective required:
                            return this.IsMatch(item, required.acceptableContextTagSets);

                        default:
                            return false;
                    }
                });
        }

        /// <summary>Get whether an item matches the expected values.</summary>
        /// <param name="item">The item to check.</param>
        /// <param name="contextTags">The expected context tags.</param>
        private bool IsMatch(Item? item, NetStringList contextTags)
        {
            QuestModel.DonateObjective.acceptableContextTagSets = contextTags;
            return QuestModel.DonateObjective.IsValidItem(item);
        }
    }
}
