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

    public PlantType selectedType = PlantType.Grass;

    private void Awake()
    {
        PlantPoolManagerInstance = this;
        BuildUnlockPricingLookup();
        RebuildPoolTypeCounts();
        ResetUnlockedCounts();
        //EnsureInitialUnlockState();
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
        //EnsureInitialUnlockState();

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

        unlockedPoolCountByType[type] = GetUnlockedCountForType(type) + 1;
        Debug.Log("Unlocked " + type + " seed pool " + GetUnlockedCountForType(type) + "/" + GetTotalPoolCountForType(type));

        if (!HasUnlockedPoolForType(selectedType))
            SetFirstAvailableSelectedType();

        return true;
    }

    private void EnsureInitialUnlockState()
    {
        if (GetTotalPoolCountForType(PlantType.Grass) > 0 && GetUnlockedCountForType(PlantType.Grass) == 0)
            unlockedPoolCountByType[PlantType.Grass] = 1;
    }

    private bool IsPoolUnlocked(PlantPool pool)
    {
        if (pool == null || plantPools == null)
            return false;

        int unlockedCountForType = GetUnlockedCountForType(pool.plantType);
        if (unlockedCountForType <= 0)
            return false;

        int poolIndexWithinType = 0;
        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool candidate = plantPools[i];
            if (candidate == null || candidate.plantType != pool.plantType)
                continue;

            if (candidate == pool)
                return poolIndexWithinType < unlockedCountForType;

            poolIndexWithinType++;
        }

        return false;
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
}
