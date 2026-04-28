using CS.AudioToolkit;
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
    private string seedLauncherSound;
    [Header("Launcher animations")]
    [SerializeField] private Animator launcherAnimator;

    private void Awake()
    {
        SMInstance = this;
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
        GameObject seed = null;

        launcherAnimator.SetTrigger("FireTrigger");

        switch (currentShotType)
        {
            case ShotType.seed:
                seed = pool.GetObject();
                seedLauncherSound = "SeedLauncherShot";
                break;

            case ShotType.grenade:
                seed = payloadPool.GetObject();

                SeedGrenade grenade = seed.GetComponent<SeedGrenade>();
                if (grenade != null)
                {
                    grenade.payloadPool = payloadPool;
                    grenade.pool = pool;
                    seedLauncherSound = "SeedGrenadeShot";
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

        AudioController.Play(seedLauncherSound);
        Rigidbody rb = seed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = seed.transform.up * seedSpeed;
        }
    }

}
