using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class CallDownEquipment : MonoBehaviour
{
    private enum EquipmentType
    {
        RefillStation,
        Sprinkler
    }

    [Header("Equipment Prefabs")]
    [SerializeField] private GameObject refillStationPrefab;
    [SerializeField] private GameObject sprinklerPrefab;

    [Header("Placement")]
    [SerializeField] private float maxPlacementDistance = 25f;
    [SerializeField] private float previewSurfaceOffset = 0.02f;
    [SerializeField] private Vector3 decalRotationOffsetEuler = new Vector3(90f, 0f, 0f);
    [SerializeField] private EquipmentType currentEquipment = EquipmentType.RefillStation;
    [SerializeField] private DecalProjector landingDecalProjector;

    [Header("Drop")]
    [SerializeField] private float spawnHeight = 50f;
    [SerializeField] private float downwardLaunchSpeed = 35f;

    public static CallDownEquipment CDEInstance { get; private set; }

    public int SprinklerCharges { get; private set; }
    public int RefillStationCharges { get; private set; }

    private bool hasValidPlacement;
    private Vector3 lastValidPlacementPoint;

    private void Awake()
    {
        CDEInstance = this;
        SetPreviewVisible(false);
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

    public void BeginPlacementPreview()
    {
        hasValidPlacement = false;
        SetPreviewVisible(false);
    }

    public void UpdatePlacementPreview(Ray placementRay, Vector3 upReference)
    {
        if (!Physics.Raycast(placementRay, out RaycastHit hit, maxPlacementDistance))
        {
            hasValidPlacement = false;
            SetPreviewVisible(false);
            return;
        }

        hasValidPlacement = true;
        lastValidPlacementPoint = hit.point;
        UpdatePreviewTransform(hit.point, hit.normal, placementRay.direction, upReference);
    }

    public void ConfirmPlacement()
    {
        if (hasValidPlacement)
        {
            TrySummonEquipment(lastValidPlacementPoint);
        }

        hasValidPlacement = false;
        SetPreviewVisible(false);
    }

    public void CancelPlacement()
    {
        hasValidPlacement = false;
        SetPreviewVisible(false);
    }

    public void AddSprinklerCharge(int chargeAdded)
    {
        SprinklerCharges += Mathf.Max(0, chargeAdded);
    }

    public void AddRefillStationCharge(int chargeAdded)
    {
        RefillStationCharges += Mathf.Max(0, chargeAdded);
    }

    private void UpdatePreviewTransform(Vector3 hitPoint, Vector3 hitNormal, Vector3 forwardReference, Vector3 upReference)
    {
        if (landingDecalProjector == null)
        {
            return;
        }

        Vector3 projectedForward = Vector3.ProjectOnPlane(forwardReference, hitNormal).normalized;

        Quaternion surfaceRotation = projectedForward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(projectedForward, hitNormal)
            : Quaternion.FromToRotation(upReference, hitNormal);

        Quaternion previewRotation = surfaceRotation * Quaternion.Euler(decalRotationOffsetEuler);
        Vector3 previewPosition = hitPoint + (hitNormal * previewSurfaceOffset);

        landingDecalProjector.transform.SetPositionAndRotation(previewPosition, previewRotation);
        SetPreviewVisible(true);
    }

    private void SetPreviewVisible(bool isVisible)
    {
        if (landingDecalProjector != null)
        {
            landingDecalProjector.enabled = isVisible;
        }
    }

    private void TrySummonEquipment(Vector3 position)
    {
        GameObject selectedPrefab = GetCurrentEquipmentPrefab();
        if (selectedPrefab == null)
        {
            Debug.LogWarning("No prefab assigned for current equipment type.");
            return;
        }

        if (currentEquipment == EquipmentType.Sprinkler)
        {
            if (SprinklerCharges <= 0)
            {
                Debug.Log("No sprinkler charges left!");
                return;
            }

            SprinklerCharges--;
        }
        else
        {
            if (RefillStationCharges <= 0)
            {
                Debug.Log("No refill station charges left!");
                return;
            }

            RefillStationCharges--;
        }

        Vector3 spawnPosition = position + (Vector3.up * spawnHeight);
        GameObject spawnedEquipment = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

    }

    private GameObject GetCurrentEquipmentPrefab()
    {
        return currentEquipment == EquipmentType.RefillStation ? refillStationPrefab : sprinklerPrefab;
    }
}