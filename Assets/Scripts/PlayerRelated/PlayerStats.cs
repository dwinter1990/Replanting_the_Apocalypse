using UnityEngine;
using System;
public class PlayerStats : MonoBehaviour
{
    public static event Action<int> ResearchPointsChanged;
    public static PlayerStats Instance {get; private set;}
    [Header("Watering stats")]
    public float maxWaterCapacity = 100;
    public float currentWaterCapacity;
    public float moveSpeed;
    public float waterDrainRate;
    public float waterRefillRate;

    [Header("Research")]
    public int researchPoints;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        currentWaterCapacity = maxWaterCapacity;
        ResearchPointsChanged?.Invoke(researchPoints);
    }

    public void IncreaseWaterCapacity(float amount)
    {
        maxWaterCapacity += amount;
    }
    public bool HasWater()
    {
        return currentWaterCapacity > 0f;
    }
    public void UseWater()
    {
        currentWaterCapacity -= waterDrainRate * Time.deltaTime;
        currentWaterCapacity = Mathf.Clamp(currentWaterCapacity, 0, maxWaterCapacity);
    }

    public void RefillWater()
    {
        currentWaterCapacity += waterRefillRate * Time.deltaTime;
        currentWaterCapacity = Mathf.Clamp(currentWaterCapacity, 0, maxWaterCapacity);
    }
    public bool TrySpendResearchPoints(int amount)
    {
        if (amount <= 0)
            return true;

        if (researchPoints < amount)
            return false;

        researchPoints -= amount;

        Debug.Log("Research points spent: " + amount + ", total: " + researchPoints);
        ResearchPointsChanged?.Invoke(researchPoints);

        if (PlantPoolManager.PlantPoolManagerInstance != null)
        {
            PlantPoolManager.PlantPoolManagerInstance.RefreshUnlockedPools();
        }

        return true;
    }

    public void AddResearchPoints(int amount)
    {
        if (amount <= 0)
            return;

        researchPoints += amount;
        Debug.Log("Research points gained: " + amount + ", total: " + researchPoints);
        ResearchPointsChanged?.Invoke(researchPoints);
        if (PlantPoolManager.PlantPoolManagerInstance != null)
        {
            PlantPoolManager.PlantPoolManagerInstance.RefreshUnlockedPools();
        }
        if(ObjectivesTutorial.OTInstance != null && researchPoints >= 50)
        {
            ObjectivesTutorial.OTInstance.TryResearchPointsTriggered();
        }
    }


}
