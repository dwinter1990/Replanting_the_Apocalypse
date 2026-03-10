using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance {get; private set;}
    [Header("Watering stats")]
    public float maxWaterCapacity = 100;
    public float currentWaterCapacity;
    public float moveSpeed;
    public float waterDrainRate;
    public float waterRefillRate;
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
        Debug.Log("Water left: " + currentWaterCapacity + " : " + maxWaterCapacity);
    }

    public void RefillWater()
    {
        currentWaterCapacity += waterRefillRate * Time.deltaTime;
        currentWaterCapacity = Mathf.Clamp(currentWaterCapacity, 0, maxWaterCapacity);
        Debug.Log("Water has been filled to: " + currentWaterCapacity + " : " + maxWaterCapacity);
    }
}
