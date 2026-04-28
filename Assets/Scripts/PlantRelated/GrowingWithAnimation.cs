using UnityEngine;

public class GrowingWithAnimation : MonoBehaviour
{
    [SerializeField] public GrowthSO profile;
    private PlantPool originPool;
    private int startingLayer;

    [Header("Watering settings")]
    private bool hasFullyGrown;
    public bool HasFullyGrown => hasFullyGrown;
    private int ignoreWaterLayer;
    private float lastWateredTime;
    private float waterDuration;
    private float stepDuration;
    private float scalePerStep;
    private float nextGrowthTimer;

    [Header("Animation settings")]
    public Quaternion startRotation;
    private GameObject spawnedMound;
    private Animator animator;
    [SerializeField] private string growStateName = "Grow";
    private int totalSteps = 0;
    private int currentStep = 0;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        ignoreWaterLayer = LayerMask.NameToLayer("IgnoreWater");
        startingLayer = gameObject.layer;
    }
    private void Start()
    {
        if(profile == null)
        {
            Debug.LogWarning("GrowthSO profile is not assigned on " + gameObject.name);
            return;
        }
        
        startRotation = transform.rotation;
        waterDuration = profile.waterMemory;
        totalSteps = Mathf.Max(1, Mathf.RoundToInt(profile.growthSteps));
        stepDuration = profile.growthDuration / totalSteps;
        currentStep = 0;
        nextGrowthTimer = 0f;

        //ApplyGrowthAnimation();
    }
    public void SetPool(PlantPool pool)
    {
        originPool = pool;
    }


    public void Water()
    {
        if (hasFullyGrown)
            return;

        lastWateredTime = Time.time;
        if(nextGrowthTimer <= Time.time)
        {
            nextGrowthTimer = Time.time + stepDuration;
        }
            GrowOneStep();
    }

    public bool UpdateGrowth(float time)
    {
        if (hasFullyGrown) 
        {
            return false;
        }

        if (time - lastWateredTime > waterDuration)
            return false;

        if (time >= nextGrowthTimer)
        {
            nextGrowthTimer += stepDuration;
            GrowOneStep();
        }

        return true;
    }

    private void GrowOneStep()
    {
        currentStep = Mathf.Min(currentStep +1, totalSteps);

        if(spawnedMound != null && currentStep >= Mathf.CeilToInt(totalSteps * 0.25f))
        {
            MoundPool.instance.Return(spawnedMound);
            spawnedMound = null;
        }
        float normalized = Mathf.Clamp01((float)currentStep / profile.growthSteps);

        if(currentStep >= totalSteps)
        {
            currentStep = totalSteps;
            hasFullyGrown = true;
            gameObject.layer = ignoreWaterLayer;
        }

        ApplyGrowthAnimation();

    }

    public void SetSpawnedMound(GameObject mound)
    {
        spawnedMound = mound;
    }


    private void ApplyGrowthAnimation()
    {
        if(animator == null)
        {
            return; 
        }

        float normalized = totalSteps > 0 ? (float)currentStep / totalSteps : 0f;
        animator.Play(growStateName, 0, Mathf.Clamp01(normalized));
        animator.Update(0f); // Force the animator to update immediately to reflect the new state
    }

    public void Harvest()
    {

        if (!hasFullyGrown)
        {
            Debug.LogWarning("Plant has no source pool configured for harvesting: " + gameObject.name);
            return;
        }
        string plantId = profile != null ? profile.name : gameObject.name.Replace("(Clone)", string.Empty).Trim();
        int researchPointsValue = profile != null ? profile.researchPointValue : 1;

        PlantPool sourcePool = originPool;
        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;

        if(sourcePool == null && manager != null)
        {
            manager.TryResolvePoolByPlantId(plantId, out sourcePool);
        }

        if(sourcePool == null)
        {
            Debug.LogWarning("Could not find source PlantPool for harvested plant: " + gameObject.name);
            return;
        }

        HarvestTracker tracker = HarvestTracker.Instance;
            if (tracker != null)
                tracker.RecordHarvest(originPool.plantType, plantId, researchPointsValue);
            else
                Debug.LogWarning("HarvestTracker is missing in the scene; harvest was not tracked.");

        if(manager != null && manager.TryUnlockSpecificPool(sourcePool))
        {
            Debug.Log("Unlocked new pool for type: " + sourcePool.plantType + ": " + sourcePool.name);
        }

        Debug.LogWarning("Harvesting: " + plantId);
        ResetPlant();
        //Spawn in seeds to collect from harvesting, when configured
        if (originPool != null)
        {
            originPool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    


    void ResetPlant()
    {
        currentStep = 0;
        if(animator != null)
        {
            animator.Play(growStateName, 0, 0f);
            animator.Update(0f); // Force the animator to update immediately to reflect the new state
        }

        transform.rotation = startRotation;
        currentStep = 0;
        nextGrowthTimer = 0f;
        lastWateredTime = 0f;
        gameObject.layer = startingLayer;
        hasFullyGrown = false;
        ApplyGrowthAnimation();
    }
}