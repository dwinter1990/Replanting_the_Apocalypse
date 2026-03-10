using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private HandManager handManager;
    [SerializeField] private WaterHose waterHose;
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] Camera playerCam;
    // Called by the Input System "Attack" action
    public void OnAttack(InputAction.CallbackContext context)
    {
        HandType activeHand = handManager.GetActiveHandType();

        // Start action when button is pressed
        if (context.performed)
        {
            if (activeHand == HandType.Water)
            {
                waterHose.StartSpray();
            }
            else if (activeHand == HandType.Harvest)
            {
                TryHarvest();
            }
        }

        // Stop action when button is released
        if (context.canceled)
        {
            if (activeHand == HandType.Water)
            {
                waterHose.StopSpray();
            }
            // Harvest doesn’t need stopping
        }
    }

    private void TryHarvest()
    {
        // Raycast from camera to mouse position
        Ray ray = playerCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            var growing = hit.collider.GetComponent<Growing>();
            if (growing != null)
            {
                growing.Harvest();
            }
        }
    }
}