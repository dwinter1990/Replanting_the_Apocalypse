using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandManager handManager;
    [SerializeField] private WaterHose waterHose;
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private Camera playerCam;
    [SerializeField] private CallDownEquipment callDownEquipment;
    private float shootTime;
    [SerializeField] float timeBetweenShots;
    private Outline currentOutline;
    [SerializeField] private Animator chainSawAnimator;
    public bool canShootSeed = false;
    private bool CanShootSeed => canShootSeed;

    [Header("Water/Harvest UI Elements")]
    [SerializeField] private Image waterThisPlant;
    [SerializeField] private Image harvestThisPlant;

    public static PlayerInteraction PIInstance { get; set; }

    private void Awake()
    {
        if (PIInstance != null && PIInstance != this)
        {
            Destroy(this);
        }
        else
        {
            PIInstance = this;
        }
    }
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
                chainSawAnimator.SetBool("Harvest", false);
            }
        }
    }

    public void OnShootSeedInput(InputAction.CallbackContext context)
    {
        if(!CanShootSeed)
        {
            return;
        }
        HandTypeLeft activeHand = handManager.GetActiveHandLeftType();
        if (activeHand == HandTypeLeft.SeedLauncher)
        {
            if (context.performed && shootTime <= 0f)
            {
                SeedManager.SMInstance.ShootSeed();
                shootTime = timeBetweenShots;
            }
        }
        if(activeHand == HandTypeLeft.Placer)
        {
            if (context.performed)
            {
                CallDownEquipment.CDEInstance.ChoosePlaceForEquipment();
                Debug.Log("Hand should be placing equipment");
            }
        }
    }
    private void Update()
    {
        shootTime -= Time.deltaTime;
        Ray ray = GetInteractionRay();
        Growing growing = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            growing = hit.collider.GetComponent<Growing>();
            if (growing != null)
                if (growing.HasFullyGrown)
                {
                    harvestThisPlant.enabled = true;
                    waterThisPlant.enabled = false;
                }
                else
                {
                    harvestThisPlant.enabled = false;
                    waterThisPlant.enabled = true;
                }

        }
        if (growing == null)
        {
            harvestThisPlant.enabled = false;
            waterThisPlant.enabled = false;
        }
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
            chainSawAnimator.SetBool("Harvest", true);
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