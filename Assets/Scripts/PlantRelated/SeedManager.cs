using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
public class SeedManager : MonoBehaviour
{
    public static SeedManager SMInstance;

    public ObjectPool pool;
    [SerializeField] float seedSpeed;
    [SerializeField] Transform seedSpawnPoint;

    [Header("Shoot stats")]
    [SerializeField] float timeBetweenShots;
    private float shootTime;
    private bool canShoot = false;

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
    public void ShootSeed()
    {
        GameObject seed = pool.GetObject();
        seed.transform.position = seedSpawnPoint.position;
        seed.transform.rotation = seedSpawnPoint.rotation;
        
        Rigidbody rb = seed.GetComponent<Rigidbody>();

        if(rb != null)
        {
            rb.linearVelocity = seed.transform.up * seedSpeed;
        }

        //StartCoroutine(DeactivateSeed(seed));
    }

    IEnumerator DeactivateSeed(GameObject seed)
    {
        yield return new WaitForSeconds(2f);
        pool.ReturnObject(seed);
    }
}
