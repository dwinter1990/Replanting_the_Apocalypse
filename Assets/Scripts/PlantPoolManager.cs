using UnityEngine;


public class PlantPoolManager : MonoBehaviour
{
    public static PlantPoolManager PlantPoolManagerInstance;

    public PlantPool[] plantPools;

    [Header("Research unlock settings")]
    [Min(1)][SerializeField] private int researchPointsPerPoolUnlock = 10;

    private int unlockedPoolCount = 1;

    public PlantType selectedType = PlantType.Grass;
    private void Awake()
    {
        PlantPoolManagerInstance = this;
        RefreshUnlockedPools();
    }

    public GameObject GetRandomPlant(PlantType type)
    {
        var filteredPools = System.Array.FindAll(plantPools, p => p.plantType == type && IsPoolUnlocked(p));
        if(filteredPools.Length == 0)
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
        int points = PlayerStats.Instance != null ? PlayerStats.Instance.researchPoints : 0;
        RefreshUnlockedPools(points);
    }

    public void RefreshUnlockedPools(int currentResearchPoints)
    {
        int previousUnlockedCount = unlockedPoolCount;
        int poolCount = plantPools != null ? plantPools.Length : 0;

        if (poolCount == 0)
        {
            unlockedPoolCount = 0;
            return;
        }

        int unlocksFromResearch = currentResearchPoints / researchPointsPerPoolUnlock;
        unlockedPoolCount = Mathf.Clamp(1 + unlocksFromResearch, 1, poolCount);

        if (unlockedPoolCount != previousUnlockedCount)
        {
            Debug.Log("Unlocked plant pools: " + unlockedPoolCount + "/" + poolCount + " (research: " + currentResearchPoints + ")");
        }

        if (!HasUnlockedPoolForType(selectedType))
        {
            SetFirstAvailableSelectedType();
        }
    }

    private bool IsPoolUnlocked(PlantPool pool)
    {
        if (pool == null || plantPools == null)
            return false;

        int poolIndex = System.Array.IndexOf(plantPools, pool);
        return poolIndex >= 0 && poolIndex < unlockedPoolCount;
    }

    private bool HasUnlockedPoolForType(PlantType type)
    {
        if (plantPools == null)
            return false;

        for (int i = 0; i < plantPools.Length; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool != null && i < unlockedPoolCount && pool.plantType == type)
            {
                return true;
            }
        }

        return false;
    }

    private void SetFirstAvailableSelectedType()
    {
        if (plantPools == null)
            return;

        for (int i = 0; i < plantPools.Length && i < unlockedPoolCount; i++)
        {
            PlantPool pool = plantPools[i];
            if (pool != null)
            {
                selectedType = pool.plantType;
                return;
            }
        }
    }
}
