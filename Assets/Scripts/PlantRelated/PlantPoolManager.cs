using System.Collections.Generic;
using UnityEngine;


public class PlantPoolManager : MonoBehaviour
{
    [System.Serializable]
    private class PlantUnlockPricing
    {
        public PlantType plantType;
        [Min(1)] public int baseResearchCost = 10;
        [Min(1f)] public float costGrowthPerUnlock = 1.25f;
    }


    public static PlantPoolManager PlantPoolManagerInstance;

    public PlantPool[] plantPools;

    [Header("Research unlock settings")]
    [SerializeField] private PlantUnlockPricing[] unlockPricingByType;

    private readonly Dictionary<PlantType, int> unlockedPoolCountByType = new Dictionary<PlantType, int>();
    private readonly Dictionary<PlantType, int> totalPoolCountByType = new Dictionary<PlantType, int>();
    private readonly Dictionary<PlantType, PlantUnlockPricing> unlockPricingLookup = new Dictionary<PlantType, PlantUnlockPricing>();
    private readonly HashSet<PlantPool> unlockedPools = new HashSet<PlantPool>();

    public PlantType selectedType = PlantType.Grass;

    private bool firstHarvest = true;
    private void Awake()
    {
        PlantPoolManagerInstance = this;
        BuildUnlockPricingLookup();
        RebuildPoolTypeCounts();
        ResetUnlockedCounts();
        RefreshUnlockedPools();
    }

    public int GetUnlockCost(PlantType type)
    {
        PlantUnlockPricing pricing = GetPricingForType(type);
        int unlocksAlreadyPurchased = GetPurchasedUnlockCount(type);
        float scaledCost = pricing.baseResearchCost * Mathf.Pow(pricing.costGrowthPerUnlock, unlocksAlreadyPurchased);
        return Mathf.Max(1, Mathf.RoundToInt(scaledCost));
    }

    public GameObject GetRandomPlant(PlantType type)
    {
        var filteredPools = System.Array.FindAll(plantPools, p => p != null && p.plantType == type && IsPoolUnlocked(p));
        if (filteredPools.Length == 0)
        {
            Debug.LogWarning("No unlocked pools of type " + type);
            return null;
        }

        int index = Random.Range(0, filteredPools.Length);
        return filteredPools[index].Get();
    }

    public void ChangeSelectedType(int delta)
    {
        int typeCount = System.Enum.GetNames(typeof(PlantType)).Length;

        for (int i = 1; i <= typeCount; i++)
        {
            int newType = ((int)selectedType + (delta * i) + typeCount) % typeCount;
            PlantType candidateType = (PlantType)newType;
            if (HasUnlockedPoolForType(candidateType))
            {
                selectedType = candidateType;
                Debug.Log("Selected plant type is: " + selectedType);
                return;
            }
        }

        Debug.LogWarning("No unlocked plant pools are available to select.");
    }

    public void RefreshUnlockedPools()
    {
        RebuildPoolTypeCounts();

        if (!HasUnlockedPoolForType(selectedType))
            SetFirstAvailableSelectedType();

        int currentResearch = PlayerStats.Instance != null ? PlayerStats.Instance.researchPoints : 0;
        Debug.Log("Unlocked pools by type -> " + BuildUnlockSummary(currentResearch));
    }

    public List<PlantType> GetUnlockableTypes()
    {
        List<PlantType> types = new List<PlantType>();

        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
        {
            if (CanUnlockType(type))
                types.Add(type);
        }

        return types;
    }

    public bool CanUnlockType(PlantType type)
    {
        return GetUnlockedCountForType(type) < GetTotalPoolCountForType(type);
    }

    public int GetUnlockedCount(PlantType type)
    {
        return GetUnlockedCountForType(type);
    }

    public int GetTotalCount(PlantType type)
    {
        return GetTotalPoolCountForType(type);
    }


    public bool TryUnlockNextPool(PlantType type)
    {
        if (!CanUnlockType(type))
            return false;

        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool == null || pool.plantType != type || IsPoolUnlocked(pool))
                continue;

            MarkPoolUnlocked(pool);
            Debug.Log("Unlocked " + type + " seed pool " + (GetUnlockedCountForType(type)) + "/" + GetTotalPoolCountForType(type));


            if (!HasUnlockedPoolForType(selectedType))
                SetFirstAvailableSelectedType();

            return true;
        }
        return false;
    }
    
    
    public bool TryUnlockSpecificPool(PlantPool targetPool)
    {
        if (targetPool == null || plantPools == null)
            return false;

        bool poolExists = false;
        for (int i = 0; i < plantPools.Length; i++)
        {
            if (plantPools[i] == targetPool)
            {
                poolExists = true;
                break;
            }
        }

        if (!poolExists)
            return false;

        if (IsPoolUnlocked(targetPool))
            return false;

        MarkPoolUnlocked(targetPool);
        Debug.Log("Unlocked specific " + targetPool.plantType + " seed pool " + GetUnlockedCountForType(targetPool.plantType) + "/" + GetTotalPoolCountForType(targetPool.plantType));

        if (!HasUnlockedPoolForType(selectedType))
            SetFirstAvailableSelectedType();

        return true;
    }

public bool TryResolvePoolByPlantId(string plantId, out PlantPool matchingPool)
    {
        matchingPool = null;
        if (string.IsNullOrWhiteSpace(plantId) || plantPools == null)
            return false;

        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool == null)
                continue;

            if (PoolMatchesPlantId(pool, plantId))
            {
                matchingPool = pool;
                return true;
            }
        }

        return false;
    }

    private bool IsPoolUnlocked(PlantPool pool)
    {
        return pool != null && unlockedPools.Contains(pool);
    }

    private bool HasUnlockedPoolForType(PlantType type)
    {
        if (plantPools == null)
            return false;

        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool != null && pool.plantType == type && IsPoolUnlocked(pool))
                return true;
        }

        return false;
    }

    private void SetFirstAvailableSelectedType()
    {
        if (plantPools == null)
            return;

        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool != null && IsPoolUnlocked(pool))
            {
                selectedType = pool.plantType; 
                if (firstHarvest)
                {
                    firstHarvest = false;   
                    ObjectivesTutorial.OTInstance.FirstPlantHarvested();
                }
                return;
            }
        }
    }

    private void RebuildPoolTypeCounts()
    {
        totalPoolCountByType.Clear();

        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
            totalPoolCountByType[type] = 0;

        if (plantPools == null)
            return;

        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool == null)
                continue;

            totalPoolCountByType[pool.plantType]++;
        }
    }
    private void BuildUnlockPricingLookup()
    {
        unlockPricingLookup.Clear();

        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
            unlockPricingLookup[type] = null;

        if (unlockPricingByType != null)
        {
            for (int i = 0; i < unlockPricingByType.Length; i++)
            {
                PlantUnlockPricing pricing = unlockPricingByType[i];
                if (pricing == null)
                    continue;

                unlockPricingLookup[pricing.plantType] = pricing;
            }
        }

        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
        {
            if (unlockPricingLookup[type] == null)
            {
                unlockPricingLookup[type] = new PlantUnlockPricing
                {
                    plantType = type,
                    baseResearchCost = 10,
                    costGrowthPerUnlock = 1.25f
                };
            }
        }
    }

    private PlantUnlockPricing GetPricingForType(PlantType type)
    {
        if (!unlockPricingLookup.TryGetValue(type, out PlantUnlockPricing pricing) || pricing == null)
        {
            pricing = new PlantUnlockPricing
            {
                plantType = type,
                baseResearchCost = 10,
                costGrowthPerUnlock = 1.25f
            };
            unlockPricingLookup[type] = pricing;
        }

        return pricing;
    }

    private int GetPurchasedUnlockCount(PlantType type)
    {
        int unlockedCount = GetUnlockedCountForType(type);
        int freeUnlocks = type == PlantType.Grass ? 1 : 0;
        return Mathf.Max(0, unlockedCount - freeUnlocks);
    }




    private void ResetUnlockedCounts()
    {
        unlockedPoolCountByType.Clear();
        unlockedPools.Clear();

        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
            unlockedPoolCountByType[type] = 0;
    }

    private int GetTotalPoolCountForType(PlantType type)
    {
        return totalPoolCountByType.TryGetValue(type, out int count) ? count : 0;
    }

    private int GetUnlockedCountForType(PlantType type)
    {
        return unlockedPoolCountByType.TryGetValue(type, out int count) ? count : 0;
    }

    private string BuildUnlockSummary(int currentResearchPoints)
    {
        List<string> parts = new List<string>();
        foreach (PlantType type in System.Enum.GetValues(typeof(PlantType)))
        {
            int unlocked = GetUnlockedCountForType(type);
            int total = GetTotalPoolCountForType(type);
            parts.Add(type + ": " + unlocked + "/" + total);
        }

        return string.Join(", ", parts) + "; research=" + currentResearchPoints;
    }

    private void MarkPoolUnlocked(PlantPool pool)
    {
        if (pool == null || unlockedPools.Contains(pool))
            return;

        unlockedPools.Add(pool);
        unlockedPoolCountByType[pool.plantType] = GetUnlockedCountForType(pool.plantType) + 1;
    }

    private bool PoolMatchesPlantId(PlantPool pool, string plantId)
    {
        if (pool == null || pool.Prefab == null || string.IsNullOrWhiteSpace(plantId))
            return false;

        Growing growing = pool.Prefab.GetComponent<Growing>();
        if (growing != null && growing.profile != null && growing.profile.name == plantId)
            return true;

        string prefabName = pool.Prefab.name.Replace("(Clone)", string.Empty).Trim();
        return prefabName == plantId;
    }

}
