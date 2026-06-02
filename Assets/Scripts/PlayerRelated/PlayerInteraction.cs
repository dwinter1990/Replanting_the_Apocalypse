using CS.AudioToolkit;
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
    [SerializeField] private CallDownEquipment callDownEquipment;
    [SerializeField] private Animator chainSawAnimator;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private float sphereRadius = 0.35f;
    [SerializeField] private Camera playerCam;
    private readonly RaycastHit[] interactionHits = new RaycastHit[8];
    [SerializeField] private float interactionRadius = 0.5f;
    private float harvestTime;
    private Coroutine harvestCoroutine;
    private Coroutine startHarvestNextFrameCoroutine;

    [Header("Seed Shooting Settings")]
    private float shootTime;
    [SerializeField] float timeBetweenShots;
    public bool canShootSeed = false;
    private bool CanShootSeed => canShootSeed;

    [Header("Water/Harvest UI Elements")]
    [SerializeField] private Image waterThisPlant;
    [SerializeField] private Image harvestThisPlant;
    [SerializeField] private GameObject harvestFX;
    private ParticleSystem[] harvestParticles;
    [SerializeField] private Transform rightArmRoot;
    public static PlayerInteraction PIInstance { get; set; }

    private bool isHoldingEquipmentPlacement;

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

    public void OnRightClick(InputAction.CallbackContext context)
    {
        HandTypeRight activeHand = handManager.GetActiveHandRightType();

        if (context.performed)
        {
            if (activeHand == HandTypeRight.Water)
            {
                waterHose.StartSpray();
                StopHarvest();
            }
            else if (activeHand == HandTypeRight.Harvest)
            {
                StartHarvest();
                AudioController.Stop("ChainsawIdle");
                AudioController.Play("ChainsawRip");
                waterHose.StopSpray();
            }
        }

        if (context.canceled)
        {
            if (activeHand == HandTypeRight.Water)
            {
                waterHose.StopSpray();
            }

            if (activeHand == HandTypeRight.Harvest)
            {
                StopHarvest();
                AudioController.Stop("ChainsawRip");
                AudioController.Play("ChainsawIdle");
            }
        }

        if (context.canceled)
        {
            if (activeHand == HandTypeRight.Water)
            {
                waterHose.StopSpray();

            }

            if (activeHand == HandTypeRight.Harvest)
            {
                StopCoroutine("TryHarvest");
                chainSawAnimator.SetBool("Harvest", false);
                AudioController.Stop("ChainsawRip");
                AudioController.Play("ChainsawIdle");
                harvestTime = 0f;
                StopHarvestFX();
            }
        }
    }
    private IEnumerator StartHarvestNextFrame()
    {
        yield return null;

        startHarvestNextFrameCoroutine = null;
        harvestCoroutine = StartCoroutine(TryHarvest());
    }
    private void StartHarvest()
    {
        if (harvestCoroutine != null || startHarvestNextFrameCoroutine != null)
            return;

        StopHarvestFX();
        startHarvestNextFrameCoroutine = StartCoroutine(StartHarvestNextFrame());
    }

    private void StopHarvest()
    {
        if (startHarvestNextFrameCoroutine != null)
        {
            StopCoroutine(startHarvestNextFrameCoroutine);
            startHarvestNextFrameCoroutine = null;
        }

        if (harvestCoroutine != null)
        {
            StopCoroutine(harvestCoroutine);
            harvestCoroutine = null;
        }

        chainSawAnimator.SetBool("Harvest", false);
        harvestTime = 0f;
        StopHarvestFX();
    }
    private void CacheHarvestParticles()
    {
        if (rightArmRoot != null)
        {
            Transform found = rightArmRoot.Find("HarvestHandRoot/HarvestFXRoot");

            if (found != null)
                harvestFX = found.gameObject;
        }

        if (harvestFX == null)
        {
            harvestParticles = null;
            return;
        }

        harvestParticles = harvestFX.GetComponentsInChildren<ParticleSystem>(true);
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        HandTypeLeft activeHand = handManager.GetActiveHandLeftType();

        if (activeHand == HandTypeLeft.SeedLauncher)
        {
            if (!CanShootSeed)
                return;

            if (context.performed && shootTime <= 0f)
            {
                SeedManager.SMInstance.ShootSeed();
                shootTime = timeBetweenShots;
            }

            return;
        }

        if (activeHand != HandTypeLeft.Placer || callDownEquipment == null)
            return;

        if (context.started || context.performed)
        {
            if (!isHoldingEquipmentPlacement)
            {
                isHoldingEquipmentPlacement = true;
                callDownEquipment.BeginPlacementPreview();
            }

            HandManager.HMInstance.canSwapLeft = false;
            TryUpdateEquipmentPreview();
        }

        if (context.canceled)
        {
            HandManager.HMInstance.canSwapLeft = true;
            isHoldingEquipmentPlacement = false;
            callDownEquipment.ConfirmPlacement();
        }
    }

    private void Update()
    {
        shootTime -= Time.deltaTime;

        if (isHoldingEquipmentPlacement)
            TryUpdateEquipmentPreview();

        Ray ray = GetInteractionRay();

        harvestThisPlant.enabled = false;
        waterThisPlant.enabled = false;

        int hitCount = Physics.SphereCastNonAlloc(
            ray,
            interactionRadius,
            interactionHits,
            interactDistance
        );

        if (hitCount <= 0)
            return;

        Growing closestGrowing = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Growing candidate = interactionHits[i].collider.GetComponent<Growing>();

            if (candidate == null)
                continue;

            if (interactionHits[i].distance < closestDistance)
            {
                closestDistance = interactionHits[i].distance;
                closestGrowing = candidate;
            }
        }

        if (closestGrowing == null)
            return;

        if (closestGrowing.HasFullyGrown)
        {
            harvestThisPlant.enabled = true;
            waterThisPlant.enabled = false;
        }
        else
        {
            waterThisPlant.enabled = true;
            harvestThisPlant.enabled = false;
        }
    }

    private void TryUpdateEquipmentPreview()
    {
        if (playerCam == null)
            return;

        Ray placementRay = GetInteractionRay();
        callDownEquipment.UpdatePlacementPreview(placementRay, playerCam.transform.up);
    }

    private Ray GetInteractionRay()
    {
        bool useCenterScreenRay = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;

        if (useCenterScreenRay)
            return playerCam.ViewportPointToRay(new Vector3(0.5f, 0.4f, 0f));

        return playerCam.ScreenPointToRay(Mouse.current.position.ReadValue());
    }


    private void PlayHarvestFX()
    {
        CacheHarvestParticles();

        if (harvestFX == null || !harvestFX.activeInHierarchy)
            return;

        if (harvestParticles == null)
            return;

        foreach (ParticleSystem particle in harvestParticles)
        {
            if (particle == null)
                continue;

            if (!particle.gameObject.activeSelf)
                particle.gameObject.SetActive(true);

            if (!particle.gameObject.activeInHierarchy)
                continue;

            ParticleSystem.EmissionModule emission = particle.emission;
            emission.enabled = true;

            if (!particle.isPlaying)
                particle.Play(true);
        }
    }

    private void StopHarvestFX()
    {
        CacheHarvestParticles();

        if (harvestParticles == null)
            return;

        foreach (ParticleSystem particle in harvestParticles)
        {
            if (particle == null)
                continue;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }




    private IEnumerator TryHarvest()
    {
        Growing currentHarvestTarget = null;

        chainSawAnimator.SetBool("Harvest", true);
        
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            if (playerCam == null || Mouse.current == null)
            {
                chainSawAnimator.SetBool("Harvest", false);
                harvestTime = 0f;
                currentHarvestTarget = null;
                StopHarvestFX();
                continue;
            }

            Ray ray = GetInteractionRay();

            int hitCount = Physics.SphereCastNonAlloc(
                ray,
                interactionRadius,
                interactionHits,
                interactDistance
            );

            Growing closestGrowing = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = interactionHits[i].collider;
                Growing growing = hitCollider.GetComponent<Growing>();

                if (growing == null)
                    continue;

                if (!growing.HasFullyGrown)
                    continue;

                if (interactionHits[i].distance < closestDistance)
                {
                    closestDistance = interactionHits[i].distance;
                    closestGrowing = growing;
                }
            }

            if (closestGrowing == null)
            {
                chainSawAnimator.SetBool("Harvest", false);
                harvestTime = 0f;
                currentHarvestTarget = null;
                StopHarvestFX();
                continue;
            }

            CacheHarvestParticles();
            PlayHarvestFX();

            if (currentHarvestTarget != closestGrowing)
            {
                currentHarvestTarget = closestGrowing;
                harvestTime = closestGrowing.profile != null ? closestGrowing.profile.harvestTime : 0f;
            }

            harvestTime -= 0.25f;

            if (harvestTime <= 0f)
            {
                closestGrowing.Harvest();

                harvestTime = 0f;
                currentHarvestTarget = null;
                StopHarvestFX();
            }
        }
    }
}