using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
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
    [SerializeField]public float sprintPowerCost = 5f;
    [Header("Jetpack stats")]
    [SerializeField] public float jetpackFuel = 100f;
    [SerializeField] public float jetpackFuelConsumptionRate = 10f;
    [SerializeField] public float jetpackThrust = 10f;

    [Header("Power stats")]
    [SerializeField] public float maxPower = 100f;
    [SerializeField] public float currentPower;
    [SerializeField] public float powerRechargeRate = 20f;

    [Header("Research")]
    public int researchPoints;
    //[SerializeField] private TextMeshProUGUI researchPointsText;
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
        currentPower = maxPower;

        StartCoroutine(PowerRecharge());

        //researchPointsText.text = $"Research points: {researchPoints}";
    }

    public IEnumerator PowerRecharge()
    {
        while (currentPower < maxPower /*&& PlayerMovement.PMInstance.isGrounded && PlayerMovement.PMInstance.isSprint*/)
        {
            yield return new WaitForSeconds(0.25f);
            currentPower += powerRechargeRate * 0.25f;
            currentPower = Mathf.Clamp(currentPower, 0, maxPower);
        }
    }
    public void UsePower(float amount)
    {
        currentPower -= amount;
        currentPower = Mathf.Clamp(currentPower, 0, maxPower);
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

        ResearchPointsPop.RPPInstance.PlayPopAnimation(amount);
    }


}
