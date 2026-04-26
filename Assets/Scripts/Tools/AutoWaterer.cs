using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AutoWaterer : MonoBehaviour
{
    [Header("Water Particle System")]
    [SerializeField] private Transform waterSpawnPoint;
    [SerializeField] private ParticleSystem waterSpoutPS;
    private float sprayTime;
    private Coroutine SprayWater;

    [Header("Water Capacity Settings")]
    [SerializeField] private float waterTimer;
    private float currentWaterTimer;
    [SerializeField] private int maxWaterCapacity;
    private int currentWaterCapacity;
    [SerializeField] private int waterDrain;
    //[SerializeField] CapsuleCollider waterInput;

    [Header("Water Hit Detection")]
    [SerializeField] private float range;
    [SerializeField] private LayerMask plantLayerMask;
    private HashSet<Growing> wateredThisCycle = new HashSet<Growing>();
    private Collider[] plantBuffer = new Collider[30];

    private void Awake()
    {
        currentWaterCapacity = maxWaterCapacity;

        currentWaterTimer = waterTimer;
        //waterSpoutPS.emission.enabled = false;
        waterSpoutPS.Stop();

        
    }

    private void Update()
    {
        if (currentWaterCapacity <= 0 && SprayWater != null)
        {
            StopCoroutine(SprayWater);
            waterSpoutPS.Stop();

            if(currentWaterCapacity > 0)
            {
                SprayWater = StartCoroutine(Watering());
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        { 
            StartCoroutine(DelayedStartWatering());
        }
    }

    IEnumerator DelayedStartWatering()
    {
        yield return new WaitForSeconds(2.75f); // Delay before starting to water
        SprayWater = StartCoroutine(Watering());
    }
    IEnumerator Watering()
    {
        while (true)
        {
            yield return new WaitForSeconds(waterTimer);
            if (currentWaterCapacity > 0)
            {
                waterSpoutPS.Play();
                currentWaterCapacity -= waterDrain;
                yield return new WaitForSeconds(0.5f);
                WaterHitCheck();
                yield return new WaitForSeconds(sprayTime);
            }
            waterSpoutPS.Stop();
            yield return null;
        }
    }
    public void RefillWater()
    {
        currentWaterCapacity = Mathf.Min(currentWaterCapacity + 1, maxWaterCapacity);
        Debug.Log("Water capacity: " + currentWaterCapacity);
    }

    public void WaterHitCheck()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            waterSpawnPoint.position,
            range,
            plantBuffer,
            plantLayerMask
            );

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = plantBuffer[i];
            if (col.CompareTag("Plant"))
            {
                Debug.Log("AutoWaterer hit a plant: " + col.name + " on " + gameObject.name);
                col.GetComponentInParent<Growing>()?.Water();
            }
        }
    }
}
