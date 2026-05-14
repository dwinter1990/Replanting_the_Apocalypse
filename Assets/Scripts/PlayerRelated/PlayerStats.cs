using UnityEngine;
using System;
using System.Collections;
public class PlayerStats : MonoBehaviour
{
    public static event Action<int> ResearchPointsChanged;
    public static PlayerStats PSInstance {get; private set;}

    [Header("Watering stats")]
    public float maxWaterCapacity = 100;
    public float currentWaterCapacity;
    public float moveSpeed;
    public float waterDrainRate;
    public float waterRefillRate;

    [Header("Movement stats")]
    [SerializeField] public float moveSpeedMultiplier = 1f;
    [SerializeField] public float jumpHeightMultiplier = 1f;

    [Header("Jetpack stats")]
    [SerializeField] public float jetpackFuel = 100f;
    [SerializeField] public float jetpackFuelConsumptionRate = 10f;
    [SerializeField] public float jetpackFuelRechargeRate = 5f;
    [SerializeField] public float jetpackThrust = 10f;

    [Header("Power stats")]
    [SerializeField] public float power = 100f;

    [Header("Research")]
    public int researchPoints;

    private bool CanInteract = false;
    public bool canInteract => CanInteract;

    
    private void Awake()
    {
        if(PSInstance != null && PSInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        PSInstance = this;
        DontDestroyOnLoad(this.gameObject);

        currentWaterCapacity = maxWaterCapacity;
        ResearchPointsChanged?.Invoke(researchPoints);
    }

    public IEnumerator ConsumePower()
    {
        yield return new WaitForSeconds(0.25f);
        UsePower(jetpackFuelConsumptionRate);
    }
    public void UsePower(float amount)
    {
        power -= amount;
        power = Mathf.Clamp(power, 0, 100);
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
