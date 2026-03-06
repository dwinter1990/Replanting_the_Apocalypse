using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance {get; private set;}

    public float maxWaterCapacity = 100;
    public float currentWaterCapacity;
    public float moveSpeed;

    
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

    public void UseWater(float amount)
    {
        currentWaterCapacity -= amount * Time.deltaTime;
        currentWaterCapacity = Mathf.Clamp(currentWaterCapacity, 0, maxWaterCapacity);
        Debug.Log("Water left: " + currentWaterCapacity + " : " + maxWaterCapacity);
    }

    public void RefillWater(float amount)
    {
        currentWaterCapacity += amount  * Time.deltaTime;
        currentWaterCapacity = Mathf.Clamp(currentWaterCapacity, 0, maxWaterCapacity);
        Debug.Log("Water has been filled to: " + currentWaterCapacity + " : " + maxWaterCapacity);
    }
}
