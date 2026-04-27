using UnityEngine;
using System.Collections;

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
    public Vector3 startScale;
    public Quaternion startRotation;
    private float currentScale;
    private float maxScale;
    //private Outline outline;
    private Animator animator;
    private GameObject spawnedMound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ignoreWaterLayer = LayerMask.NameToLayer("IgnoreWater");
        startingLayer = gameObject.layer;
    }
    private void Start()
    {
        
            startScale = profile.startScale;
            startRotation = transform.rotation;
            transform.localScale = startScale;

            waterDuration = profile.waterMemory;

            maxScale = profile.maxScale * Random.Range(profile.minScale, profile.maxScaleMultiplier);

            stepDuration = profile.growthDuration / profile.growthSteps;

            scalePerStep = (maxScale - startScale.x) / profile.growthSteps;

            currentScale = transform.localScale.x;
    }


    public void SetPool(PlantPool pool)
    {
        originPool = pool;
    }


    public void Water()
    {
        if (hasFullyGrown)
            return;

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

    void GrowOneStep()
    {
        if (hasFullyGrown)
        {
            return;
        }
        currentScale += scalePerStep;

        if (currentScale >= maxScale * 0.25f && spawnedMound != null)
        {
            MoundPool.instance.Return(spawnedMound);
            spawnedMound = null;
        }

        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
            hasFullyGrown = true;

            gameObject.layer = ignoreWaterLayer;
            return;
        }

    }

    public void SetSpawnedMound(GameObject mound)
    {
        spawnedMound = mound;
    }


    private void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
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
        transform.localScale = profile.startScale;
        transform.rotation = startRotation;

        currentScale = startScale.x;
        nextGrowthTimer = 0f;
        gameObject.layer = startingLayer;
        hasFullyGrown = false;
    }
}