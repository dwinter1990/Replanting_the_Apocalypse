using System.Collections.Generic;
using UnityEngine;


public class PlantPoolManager : MonoBehaviour
{
    public static PlantPoolManager PlantPoolManagerInstance;

    public PlantPool[] plantPools;

    [Header("Research unlock settings")]
    [Min(1)][SerializeField] private int researchPointsPerPoolUnlock = 10;

    private readonly Dictionary<PlantType, int> unlockedPoolCountByType = new Dictionary<PlantType, int>();
    private readonly Dictionary<PlantType, int> totalPoolCountByType = new Dictionary<PlantType, int>();

    public PlantType selectedType = PlantType.Grass;

    public int ResearchPointsPerPoolUnlock => researchPointsPerPoolUnlock;

    private void Awake()
    {
        PlantPoolManagerInstance = this;
        RebuildPoolTypeCounts();
        ResetUnlockedCounts();
        EnsureInitialUnlockState();
        RefreshUnlockedPools();
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
        EnsureInitialUnlockState();

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
