using System.Collections.Generic;
using UnityEngine;

public class HarvestTracker : MonoBehaviour
{
    public static HarvestTracker Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<HarvestTracker>();

            return instance;
        }
    }

    private static HarvestTracker instance;

    private readonly Dictionary<PlantType, int> lifetimeHarvestedCountByType = new Dictionary<PlantType, int>();
    private readonly Dictionary<PlantType, int> bankedHarvestedCountByType = new Dictionary<PlantType, int>();
    private readonly HashSet<PlantType> harvestedTypes = new HashSet<PlantType>();
    private readonly HashSet<string> harvestedPlantIds = new HashSet<string>();
    //private int bankedResearchPoints;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple HarvestTracker components found. Removing duplicate component.");
            Destroy(this);
            return;
        }

        instance = this;
    }

    public void RecordHarvest(PlantType type, string plantId, int researchPointsValue)
    {
        IncrementCount(lifetimeHarvestedCountByType, type);
        IncrementCount(bankedHarvestedCountByType, type);

        harvestedTypes.Add(type);

        if (!string.IsNullOrWhiteSpace(plantId))
            harvestedPlantIds.Add(plantId);
        Debug.Log($"Harvest tracked -> Type: {type}, Lifetime Type Count: {lifetimeHarvestedCountByType[type]}, Banked Plants: {GetBankedHarvestTotalCount()}");
        PlayerStats.PSInstance.AddResearchPoints(researchPointsValue);
    }

    public int GetHarvestCount(PlantType type)
    {
        return lifetimeHarvestedCountByType.TryGetValue(type, out int count) ? count : 0;
    }

    public int GetBankedHarvestCount(PlantType type)
    {
        return bankedHarvestedCountByType.TryGetValue(type, out int count) ? count : 0;
    }

    public int GetBankedHarvestTotalCount()
    {
        int total = 0;

        foreach (var entry in bankedHarvestedCountByType)
            total += entry.Value;

        return total;
    }

    public bool HasHarvestedType(PlantType type)
    {
        return harvestedTypes.Contains(type);
    }

    public bool HasHarvestedPlant(string plantId)
    {
        return !string.IsNullOrWhiteSpace(plantId) && harvestedPlantIds.Contains(plantId);
    }

    public IReadOnlyCollection<string> GetHarvestedPlantIds()
    {
        return harvestedPlantIds;
    }

    private void IncrementCount(Dictionary<PlantType, int> map, PlantType type)
    {
        if (map.ContainsKey(type))
            map[type]++;
        else
            map[type] = 1;
    }
}
