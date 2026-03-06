using UnityEngine;

public class RefillWater : MonoBehaviour
{
    [SerializeField] int waterFillAmount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats.Instance.RefillWater(waterFillAmount);
        }
    }
}
