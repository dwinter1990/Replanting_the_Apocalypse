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
    [SerializeField] private Transform[] spawnPoints;
    //[SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private Vector3 mapCentre = Vector3.zero;
    [SerializeField] private Vector2 mapExtents = new Vector2(50f, 50f);
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
        TrySpawnDeer();
    }

    public void NotifyGrassUngrown()
    {
        fullyGrownGrassCount = Mathf.Max(0, fullyGrownGrassCount - 1);
    }

    public void SetFullyGrownGrassCount(int count)
    {
        fullyGrownGrassCount = Mathf.Max(0, count);
        TrySpawnDeer();
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

        Vector3 spawnPosition = GetSpawnPosition();
        Instantiate(deerPrefab, spawnPosition, Quaternion.identity);
        hasSpawnedDeer = true;
        Debug.Log("Deer spawned.");
    }
   

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = Random.Range(0, spawnPoints.Length);
            Transform chosenPoint = spawnPoints[index];
            if (chosenPoint != null)
            {
                return chosenPoint.position;
            }
        }

        return transform.position;
    }
}
