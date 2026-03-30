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
            }
            else if (activeHand == HandTypeRight.Harvest)
            {
                //harvestTry = true;
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
                //harvestTry = false;
                StopCoroutine("TryHarvest");
            }
        }
    }

    private void Update()
    {
        if (playerCam == null || Mouse.current == null)
        {
            return;
        }

        Ray objectCast = playerCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        Outline newOutline = null;
        if (Physics.Raycast(objectCast, out RaycastHit hitinfo, interactDistance))
        {
            Growing growing = hitinfo.collider.GetComponent<Growing>();
            if (growing != null)
            {
                newOutline = growing.GetComponent<Outline>();
                if (newOutline != null)
                {
                    Debug.Log("Now looking at: " + hitinfo.collider.gameObject.name);
                    newOutline.OutlineMode = Outline.Mode.OutlineVisible;
                    newOutline.OutlineColor = growing.HasFullyGrown ? Color.green : Color.blue;
                }
            }
        }

        if (currentOutline != null && currentOutline != newOutline)
        {
            Debug.Log("no longer looking at: " + hitinfo.collider.gameObject.name);
            currentOutline.OutlineMode = Outline.Mode.OutlineHidden;
        }

        currentOutline = newOutline;
    }
    IEnumerator TryHarvest()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
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