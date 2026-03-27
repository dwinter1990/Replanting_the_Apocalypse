using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private HandManager handManager;
    [SerializeField] private WaterHose waterHose;
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private Camera playerCam;

    public void OnAttack(InputAction.CallbackContext context)
    {
        HandTypeRight activeHand = handManager.GetActiveHandRightType();

        // Start action when button is pressed
        if (context.performed)
        {
            if (activeHand == HandTypeRight.Water)
            {
                waterHose.StartSpray();
            }
            else if (activeHand == HandTypeRight.Harvest)
            {
                StartCoroutine("TryHarvest");
            }
        }

        // Stop action when button is released
        if (context.canceled)
        {
            if (activeHand == HandTypeRight.Water)
            {
                waterHose.StopSpray();
            }

            if(activeHand == HandTypeRight.Harvest)
            {
                StopCoroutine("TryHarvest");
            }
        }
    }

    IEnumerator TryHarvest()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
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
}