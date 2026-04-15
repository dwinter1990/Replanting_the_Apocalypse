using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private HandManager handManager;
    [SerializeField] private WaterHose waterHose;
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private Camera playerCam;

    private Outline currentOutline;
    public void OnAttack(InputAction.CallbackContext context)
    {
        HandTypeRight activeHand = handManager.GetActiveHandRightType();

        // Start action when button is pressed
        if (context.performed)
        {
            if (activeHand == HandTypeRight.Water)
            {
                waterHose.StartSpray();
                StopCoroutine("TryHarvest");
            }
            else if (activeHand == HandTypeRight.Harvest)
            {
                //harvestTry = true;
                StartCoroutine("TryHarvest");
                waterHose.StopSpray();
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
                //harvestTry = false;
                StopCoroutine("TryHarvest");
            }
        }
    }
    private void Update()
    {
        if (playerCam == null || Mouse.current == null)
        {
            HideCurrentOutline();
            return;
        }

        Ray ray = GetInteractionRay();
        Outline newOutline = null;
        Growing growing = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // Same-object lookup (as you requested)
            growing = hit.collider.GetComponent<Growing>();
            if (growing != null)
                newOutline = hit.collider.GetComponent<Outline>();
        }

        // Switched target (or lost target): hide previous
        if (currentOutline != null && currentOutline != newOutline)
        {
            currentOutline.OutlineMode = Outline.Mode.OutlineHidden;
            currentOutline.enabled = false;
        }

        // Apply live state to current target every frame
        if (newOutline != null && growing != null)
        {
            newOutline.enabled = true;
            newOutline.OutlineMode = Outline.Mode.OutlineVisible;
            newOutline.OutlineColor = growing.HasFullyGrown ? Color.green : Color.cyan;
        }

        currentOutline = newOutline;
    }

    private void HideCurrentOutline()
    {
        if (currentOutline == null) return;

        currentOutline.OutlineMode = Outline.Mode.OutlineHidden;
        currentOutline.enabled = false;
        currentOutline = null;
    }

    private Ray GetInteractionRay()
    {
        bool useCenterScreenRay = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;
        if (useCenterScreenRay)
            return playerCam.ViewportPointToRay(new Vector3(0.5f, 0.4f, 0f));

        return playerCam.ScreenPointToRay(Mouse.current.position.ReadValue());
    }
    IEnumerator TryHarvest()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            
            if(playerCam == null || Mouse.current == null)
            {
                continue;
            }

            Ray ray = GetInteractionRay();
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