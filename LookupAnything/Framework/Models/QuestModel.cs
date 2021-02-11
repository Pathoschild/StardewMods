using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Items.ItemData;
using StardewValley;
using StardewValley.Quests;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>A quest model.</summary>
    internal class QuestModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The display text for the quest name.</summary>
        public string DisplayText { get; }

        /// <summary>Whether any items are needed to complete this quest..</summary>
        public bool NeedsItems { get; }

        /// <summary>The checks for whether an item is valid for this quest.</summary>
        private readonly List<Func<Item, bool>> ItemValidChecks;

        /// <summary>Construct a new quest model from a Quest.</summary>
        /// <param name="quest">The Quest.</param>
        public QuestModel(Quest quest)
        {
            this.DisplayText = quest.GetName();
            this.ItemValidChecks = new List<Func<Item, bool>>();

            // add checks for valid items
            this.NeedsItems = true;
            switch (quest)
            {
                case CraftingQuest specificQuest:
                    this.ItemValidChecks.Add(i =>
                        this.IsItemMatch(specificQuest.indexToCraft.Value, specificQuest.isBigCraftable.Value, i));
                    break;
                case ItemDeliveryQuest specificQuest:
                    this.ItemValidChecks.Add(i =>
                        this.IsItemMatch(specificQuest.item.Value, false, i));
                    break;
                case ItemHarvestQuest specificQuest:
                    this.ItemValidChecks.Add(i =>
                        this.IsItemMatch(specificQuest.itemIndex.Value, false, i));
                    break;
                case LostItemQuest specificQuest:
                    this.ItemValidChecks.Add(i =>
                        this.IsItemMatch(specificQuest.itemIndex.Value, false, i));
                    break;
                case ResourceCollectionQuest specificQuest:
                    this.ItemValidChecks.Add(i =>
                        this.IsItemMatch(specificQuest.resource.Value, false, i));
                    break;
                case SecretLostItemQuest specificQuest:
                    this.ItemValidChecks.Add(i =>
                        this.IsItemMatch(specificQuest.itemIndex.Value, false, i));
                    break;
                default:
                    this.NeedsItems = false;
                    break;
            }
        }

        /// <summary>Construct a new quest model from a Special Order.</summary>
        /// <param name="specialOrder">The Special Order.</param>
        public QuestModel(SpecialOrder specialOrder)
        {
            this.DisplayText = specialOrder.GetName();
            this.ItemValidChecks = new List<Func<Item, bool>>();

            // add checks for valid items
            this.NeedsItems = true;
            foreach (OrderObjective objective in specialOrder.objectives)
            {
                switch (objective)
                {
                    case CollectObjective specificObjective:
                        this.ItemValidChecks.Add(i =>
                            this.IsValidItem(i, specificObjective.acceptableContextTagSets));
                        break;
                    case DeliverObjective specificObjective:
                        this.ItemValidChecks.Add(i =>
                            this.IsValidItem(i, specificObjective.acceptableContextTagSets));
                        break;
                    case DonateObjective specificObjective:
                        this.ItemValidChecks.Add(specificObjective.IsValidItem);
                        break;
                    case FishObjective specificObjective:
                        this.ItemValidChecks.Add(i =>
                            this.IsValidItem(i, specificObjective.acceptableContextTagSets));
                        break;
                    case GiftObjective specificObjective:
                        this.ItemValidChecks.Add(i =>
                            this.IsValidItem(i, specificObjective.acceptableContextTagSets));
                        break;
                    case ShipObjective specificObjective:
                        this.ItemValidChecks.Add(i =>
                            this.IsValidItem(i, specificObjective.acceptableContextTagSets));
                        break;
                    default:
                        this.NeedsItems = false;
                        break;
                }
            }
        }

        /// <summary>Check whether an object is valid for this quest model.</summary>
        /// <param name="obj">The object.</param>
        public bool IsValidItem(SObject obj)
        {
            return this.ItemValidChecks.Any(f => f(obj));
        }

        /// <summary>Check whether an item is valid for this quest model.</summary>
        /// <param name="item">The item.</param>
        /// <param name="acceptableContextTagSets">The Special Order objective's acceptable context tag sets.</param>
        /// <remarks>Based on <see cref="StardewValley.DonateObjective.IsValidItem"/>.</remarks>
        private bool IsValidItem(Item item, IEnumerable<string> acceptableContextTagSets)
        {
            if (item == null)
                return false;
            foreach (string acceptableContextTagSet in acceptableContextTagSets)
            {
                bool flag1 = false;
                char[] chArray1 = new char[1] {','};
                foreach (string str1 in acceptableContextTagSet.Split(chArray1))
                {
                    bool flag2 = false;
                    char[] chArray2 = new char[1] {'/'};
                    foreach (string str2 in str1.Split(chArray2))
                    {
                        if (item.HasContextTag(str2.Trim()))
                        {
                            flag2 = true;
                            break;
                        }
                    }

                    if (!flag2)
                        flag1 = true;
                }

                if (!flag1)
                    return true;
            }

            return false;
        }

        /// <summary>Check if itemA is the same item as itemB.</summary>
        /// <param name="itemAIndex">The index for itemA.</param>
        /// <param name="isItemABigCraftable">Whether itemA is a BigCraftable.</param>
        /// <param name="itemB">The other item.</param>
        private bool IsItemMatch(int itemAIndex, bool isItemABigCraftable, Item itemB)
        {
            bool isSameIndex = itemAIndex == itemB.ParentSheetIndex;
            bool isItemBBigCraftable = itemB.GetItemType() == ItemType.BigCraftable;
            return isSameIndex && isItemABigCraftable == isItemBBigCraftable;
        }
    }
}
