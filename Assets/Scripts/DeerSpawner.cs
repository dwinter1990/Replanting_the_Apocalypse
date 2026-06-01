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
    [SerializeField] private int fullyGrownBushRequired = 10;

    [Header("Animals to Spawn")]
    [SerializeField] private GameObject deerPrefab;
    [SerializeField] private GameObject rabbitPrefab;

    [SerializeField] private float navMeshSampleDistance = 5f;

    [Header("Spawn Positioning")]
    [SerializeField] private Camera hoverHere;
    [SerializeField] private Vector3 offsetPos;
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private LayerMask groundLayer;

    

    
    private int fullyGrownGrassCount;
    private int fullyGrownBushCount;
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
        SetFullyGrownGrassCount(fullyGrownGrassCount);
    }

    public void NotifyGrassUngrown()
    {
        fullyGrownGrassCount = Mathf.Max(0, fullyGrownGrassCount - 1);
    }

    private void SetFullyGrownGrassCount(int count)
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

        GetSpawnPosition();
        Instantiate(deerPrefab, spawnPoint, Quaternion.identity);
        fullyGrownGrassCount = 0; // Reset count to allow for future spawns after more grass grows
        Debug.Log("Deer spawned.");
    }
    public void NotifyBushGrown()
    {
        fullyGrownBushCount++;
        Debug.Log($"Fully grown bush count: {fullyGrownBushCount}");
        SetFullyGrownBushCount(fullyGrownBushCount);
    }

    private void SetFullyGrownBushCount(int count)
    {
        fullyGrownBushCount = Mathf.Max(0, count);
        if (fullyGrownBushCount >= fullyGrownBushRequired)
        {
            Debug.Log($"Fully grown bush count set to {fullyGrownBushCount}. Ready to spawn deer.");
            TrySpawnRabbit();
        }
    }

    public void TrySpawnRabbit()
    {
        if (deerPrefab == null || (hasSpawnedDeer) || fullyGrownBushCount < fullyGrownBushRequired)
        {
            return;
        }
        if (hasSpawnedDeer)
        {
            return;
        }
        GetSpawnPosition();
        Instantiate(rabbitPrefab, spawnPoint, Quaternion.identity);
        fullyGrownBushCount = 0; // Reset count to allow for future spawns after more bushes grow
        Debug.Log("Deer spawned.");
    }

    public void NotifyBushUngrown()
    {
        fullyGrownBushCount = Mathf.Max(0, fullyGrownBushCount - 1);
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

    private void LateUpdate()
    {
        if (hoverHere == null)
        {
            return;
        }

        Transform cam = hoverHere.transform;

        Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;

        if (flatForward.sqrMagnitude < 0.001f)
        {
            return;
        }


        transform.position =
            cam.position
            - flatForward * offsetPos.z
            + Vector3.up * offsetPos.y;

        transform.rotation = Quaternion.LookRotation(flatForward);
    }
}

