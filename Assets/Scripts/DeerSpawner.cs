using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Example spawn gate: spawn one deer after at least 10 fully-grown grass plants exist.
/// Wire this to your Growing system by calling NotifyGrassGrown/NotifyGrassUngrown from grass lifecycle events.
/// </summary>
public class DeerSpawnLogic : MonoBehaviour
{
    public static DeerSpawnLogic DSLInstance { get; private set; }
    [Header("Spawn Requirements")]
    [SerializeField] private int fullyGrownGrassRequired = 10;

    [Header("Spawn Setup")]
    [SerializeField] private GameObject deerPrefab;
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float navMeshSampleDistance = 5f;
    

    private int fullyGrownGrassCount;
    private bool hasSpawnedDeer;

    private void Awake()
    {
        if (DSLInstance != null && DSLInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        DSLInstance = this;
    }
    public void NotifyGrassGrown()
    {
        fullyGrownGrassCount++;
        Debug.Log($"Fully grown grass count: {fullyGrownGrassCount}");
    }

    public void NotifyGrassUngrown()
    {
        fullyGrownGrassCount = Mathf.Max(0, fullyGrownGrassCount - 1);
    }

    public void SetFullyGrownGrassCount(int count)
    {
        fullyGrownGrassCount = Mathf.Max(0, count);
        if(fullyGrownGrassCount >= fullyGrownGrassRequired)
        {
            Debug.Log($"Fully grown grass count set to {fullyGrownGrassCount}. Ready to spawn deer.");
            TrySpawnDeer();
        }
    }

    private void TrySpawnDeer()
    {
        if (deerPrefab == null || (hasSpawnedDeer) || fullyGrownGrassCount < fullyGrownGrassRequired)
        {
            return;
        }

        if (hasSpawnedDeer)
        {
            return;
        }

        if (fullyGrownGrassCount < fullyGrownGrassRequired)
        {
            return;
        }


        GetSpawnPosition();
        Instantiate(deerPrefab, spawnPoint, Quaternion.identity);
        fullyGrownGrassCount = 0; // Reset count to allow for future spawns after more grass grows
        Debug.Log("Deer spawned.");
    }
   

    private Vector3 GetSpawnPosition()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, 150f, groundLayer))
        {

            spawnPoint = hit.point;
        }
        return spawnPoint;
    }
}
