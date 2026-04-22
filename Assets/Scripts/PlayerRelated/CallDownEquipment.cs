using UnityEngine;
using UnityEngine.InputSystem;

public class CallDownEquipment : MonoBehaviour
{
    [Header("Equipment Prefabs")]
    [SerializeField] private GameObject refillStationPrefab;
    [SerializeField] private GameObject sprinklerPrefab;

    [Header("Placement")]
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private EquipmentType currentEquipment = EquipmentType.RefillStation;

    public static CallDownEquipment CDEInstance;

    private void Awake()
    {
        CDEInstance = this;
    }

    public void OnChangeDropEquipment(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        currentEquipment = currentEquipment == EquipmentType.RefillStation
            ? EquipmentType.Sprinkler
            : EquipmentType.RefillStation;
    }

    public void ChoosePlaceForEquipment()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("No main camera found. Equipment was not summoned.");
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, spawnDistance))
        {
            SummonEquipment(hit.point);
            Debug.Log("Raycast hit at position: " + hit.point);
        }
        else
        {
            Debug.Log("Raycast did not hit anything. Equipment was not summoned.");
        }
    }

    private void SummonEquipment(Vector3 position)
    {
        GameObject selectedPrefab = GetCurrentEquipmentPrefab();
        if (selectedPrefab == null)
        {
            Debug.LogWarning("No prefab assigned for current equipment type.");
            return;
        }

        Instantiate(selectedPrefab, position + new Vector3(0f, 100f, 0f), Quaternion.identity);
    }

    private GameObject GetCurrentEquipmentPrefab()
    {
        switch (currentEquipment)
        {
            case EquipmentType.RefillStation:
                return refillStationPrefab;
            case EquipmentType.Sprinkler:
                return sprinklerPrefab;
            default:
                return null;
        }
    }
}
