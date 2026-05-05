using UnityEngine;

public class RefillWater : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats.PSInstance.RefillWater();
        }
    }
}
