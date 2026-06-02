using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Cinemachine;
/// <summary>
/// Example spawn gate: spawn one deer after at least 10 fully-grown grass plants exist.
/// Wire this to your Growing system by calling NotifyGrassGrown/NotifyGrassUngrown from grass lifecycle events.
/// </summary>
public class AnimalSpawner : MonoBehaviour
{
    [System.Serializable]
    public class AnimalSpawnRule
    {
        public string animalName;
        public GameObject animalPrefab;
        public PlantType requiredPlantType;
        public int fullyGrownPlantsRequired = 10;
        public bool spawnOnlyOnce;

        [HideInInspector] public bool hasSpawned;
    }
    public static AnimalSpawner ASInstance { get; private set; }

    [Header("Animals to Spawn")]
    [SerializeField] private List<AnimalSpawnRule> animalSpawnRules = new List<AnimalSpawnRule>();

    [Header("Spawn Positioning")]
    [SerializeField] private CinemachineCamera hoverHere;
    [SerializeField] private Vector3 offsetPos;
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private LayerMask groundLayer;

    private readonly Dictionary<PlantType, int> fullyGrownPlantCounts = new Dictionary<PlantType, int>();

    private void Awake()
    {
        if (ASInstance != null && ASInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        ASInstance = this;
    }

    public void NotifyPlantGrown(PlantType plantType)
    {
        int count = GetFullyGrownPlantCount(plantType) + 1;
        fullyGrownPlantCounts[plantType] = count;

        Debug.Log($"Fully grown {plantType} count: {count}");

        TrySpawnAnimalsForPlantType(plantType);
    }

    public void NotifyPlantUngrown(PlantType plantType)
    {
        int count = Mathf.Max(0, GetFullyGrownPlantCount(plantType) - 1);
        fullyGrownPlantCounts[plantType] = count;

        Debug.Log($"Fully grown {plantType} count: {count}");
    }

    public void NotifyGrassGrown()
    {
        NotifyPlantGrown(PlantType.Grass);
    }

    public void NotifyGrassUngrown()
    {
        NotifyPlantUngrown(PlantType.Grass);
    }

    public void NotifyBushGrown()
    {
        NotifyPlantGrown(PlantType.Bush);
    }

    public void NotifyBushUngrown()
    {
        NotifyPlantUngrown(PlantType.Bush);
    }

    private int GetFullyGrownPlantCount(PlantType plantType)
    {
        return fullyGrownPlantCounts.TryGetValue(plantType, out int count) ? count : 0;
    }

    private void TrySpawnAnimalsForPlantType(PlantType plantType)
    {
        foreach (AnimalSpawnRule rule in animalSpawnRules)
        {
            TrySpawnAnimal(rule, plantType);
        }
    }

    private void TrySpawnAnimal(AnimalSpawnRule rule, PlantType grownPlantType)
    {
        if (rule == null || rule.animalPrefab == null)
        {
            return;
        }

        if (rule.requiredPlantType != grownPlantType)
        {
            return;
        }

        if (rule.fullyGrownPlantsRequired <= 0)
        {
            return;
        }

        if (rule.spawnOnlyOnce && rule.hasSpawned)
        {
            return;
        }

        int plantCount = GetFullyGrownPlantCount(rule.requiredPlantType);

        if (plantCount < rule.fullyGrownPlantsRequired)
        {
            return;
        }

        Instantiate(rule.animalPrefab, GetSpawnPosition(), Quaternion.identity);

        fullyGrownPlantCounts[rule.requiredPlantType] = plantCount - rule.fullyGrownPlantsRequired;
        rule.hasSpawned = true;

        string animalName = string.IsNullOrWhiteSpace(rule.animalName)
            ? rule.animalPrefab.name
            : rule.animalName;

        Debug.Log($"{animalName} spawned after {rule.fullyGrownPlantsRequired} fully grown {rule.requiredPlantType} plants.");

        TrySpawnAnimalsForPlantType(rule.requiredPlantType);
    }

    private Vector3 GetSpawnPosition()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 150f, groundLayer))
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