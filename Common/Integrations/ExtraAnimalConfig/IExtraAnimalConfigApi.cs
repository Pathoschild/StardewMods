using System.Collections.Generic;
using StardewValley.GameData;

namespace Pathoschild.Stardew.Common.Integrations.ExtraAnimalConfig;

public interface IExtraAnimalConfigApi
{
    // Get a list of item queries, in order, that can potentially replace the specified output of this animal. Returns an empty list if there are no overrides.
    // animalType: the animal type (ie. the key in Data/FarmAnimals)
    // produceId: the qualified or unqualified ID of the base produce (ie. the value in (Deluxe)ProduceItemIds)
    public List<GenericSpawnItemDataWithCondition> GetItemQueryOverrides(string animalType, string produceId);

    // Get a list of extra custom drops associated with this animal using EAC's feature (ie not in Data/FarmAnimals).
    // This is a dictionary of strings to lists of unqualified item IDs, with each dictionary corresponding to one slot.
    // Each slot will be filled by at most one produce from one item in the list.
    // NOTE: EAC may also override the drop with an item query. Use the API function GetItemQueryOverrides to check if this is the case.
    public Dictionary<string, List<string>> GetExtraDrops(string animalType);
}
