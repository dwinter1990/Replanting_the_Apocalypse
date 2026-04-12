using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class SeedManager : MonoBehaviour
{
    public static SeedManager SMInstance;

    [Header("Seeds and other things to be launched")]
    [SerializeField] private ShotType currentShotType;
    public ObjectPool pool;
    public PayloadPool payloadPool;
    
    [Header("Shoot stats")]
    [SerializeField] float seedSpeed;
    [SerializeField] Transform seedSpawnPoint;
    [SerializeField] float timeBetweenShots;
    private float shootTime;

    [Header("Launcher animations")]
    [SerializeField] private Animator launcherAnimator;

    private void Awake()
    {
        SMInstance = this;
    }
    private void Update()
    {
        shootTime -= Time.deltaTime;
    }
    public void OnShootSeedInput(InputAction.CallbackContext context)
    {
        
        if (context.started && shootTime <= 0f)
        {
            ShootSeed();
            shootTime = timeBetweenShots;
        }
        
    }

    public void OnShotTypeChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentShotType == ShotType.seed)
            {
                currentShotType = ShotType.grenade;
            }
            else if(currentShotType == ShotType.grenade)
            {
                currentShotType = ShotType.seed;
            }
        }
    }
    public void ShootSeed()
    {
        //if(PlantPoolManager.PlantPoolManagerInstance.selectedType == PlantType)
        //{
        //    Debug.LogWarning("No plant type selected, cannot shoot seed!");
        //    return;
        //}
        GameObject seed = null;

        launcherAnimator.SetTrigger("FireTrigger");

        switch (currentShotType)
        {
            case ShotType.seed:
                seed = pool.GetObject();
                break;

            case ShotType.grenade:
                seed = payloadPool.GetObject();

                SeedGrenade grenade = seed.GetComponent<SeedGrenade>();
                if (grenade != null)
                {
                    grenade.payloadPool = payloadPool;
                    grenade.pool = pool;
                }
                break;
        }

        if (seed == null)
        {
            Debug.LogError("No seed spawned!");
            return;
        }

        //Trigger seed launching animation
        Debug.Log("Firing seed: " + seed.name);
        //seed.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        seed.transform.position = seedSpawnPoint.position;
        seed.transform.rotation = seedSpawnPoint.rotation;

        Rigidbody rb = seed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = seed.transform.up * seedSpeed;
        }
    }

}
