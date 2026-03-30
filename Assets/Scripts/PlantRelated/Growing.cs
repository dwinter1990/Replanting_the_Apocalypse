using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(TweenQueue))]
public class Growing : MonoBehaviour
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
    private TweenQueue tweenQueue;
    private Tween stepBounceTween;
    private Tween finalBounceTween;
    private Tween rotateTween;
    private Tweener scaleTween;
    public Vector3 startScale;
    public Quaternion startRotation;
    private float currentScale;
    private float maxScale;
    private Outline outline;

    private GameObject spawnedMound;


    private void Awake()
    {
        tweenQueue = GetComponent<TweenQueue>();
        ignoreWaterLayer = LayerMask.NameToLayer("IgnoreWater");
        startingLayer = gameObject.layer;

        outline = GetComponent<Outline>();
        SetOutlineHidden();
    }

    private void OnEnable()
    {
        SetOutlineHidden();
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

            CreateTweens();
    }
    void OnDisable()
    {
        SetOutlineHidden();
        stepBounceTween?.Pause();
        finalBounceTween?.Pause();
        rotateTween?.Pause();
        scaleTween?.Pause();
    }
    private void CreateTweens()
    {
        Vector3 punch = Vector3.up * profile.stepOvershoot;
        stepBounceTween = transform
            .DOPunchScale(punch, 0.25f, 6, 0.5f)
            .SetAutoKill(false)
            .Pause();

        scaleTween = transform
            .DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack, 2f)
            .SetAutoKill(false)
            .Pause();

        // FINAL BOUNCE (when fully grown)
        finalBounceTween = DOTween.Sequence()
            .Append(transform.DOScale(
                Vector3.one * maxScale * (1f + profile.finalOvershoot),
                profile.finalDuration * 0.4f).SetEase(Ease.OutQuad))

            .Append(transform.DOScale(
                Vector3.one * maxScale,
                profile.finalDuration * 0.6f).SetEase(Ease.OutBack, 2f))

            .OnComplete(() =>
            {
                hasFullyGrown = true;
            })
            .Pause()
            .SetAutoKill(false);


        // WOBBLE ROTATION
        Vector3 wobble = Vector3.forward * profile.wobbleAmount;
        rotateTween = transform.DOPunchRotation(
            wobble,
            profile.finalDuration,
            8,
            1f)
            .Pause()
            .SetAutoKill(false);
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

            scaleTween.ChangeEndValue(Vector3.one * currentScale);
            scaleTween.OnComplete(() =>
            {
                finalBounceTween.Restart();
                rotateTween.Restart();
            });

            scaleTween.Restart(true);

            gameObject.layer = ignoreWaterLayer;
            return;
        }

        scaleTween.ChangeEndValue(Vector3.one * currentScale);
        scaleTween.OnComplete(() => { });
        scaleTween.Restart(true);

        stepBounceTween.Restart();
    }

    public void SetSpawnedMound(GameObject mound)
    {
        spawnedMound = mound;
    }

    private void ResetTweens()
    {
        stepBounceTween.Rewind();
        finalBounceTween.Rewind();
        rotateTween.Rewind();
        scaleTween.Rewind();
    }

    private void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    public void Harvest()
    {
        if(originPool == null)
        {
            Debug.Log("Original pool is null");
            return;
        }
        if (!hasFullyGrown)
        {
            Debug.Log(gameObject.name + " is not ready for harvesting");
            return;
        }
        else
        {
            string plantId = profile != null ? profile.name : gameObject.name.Replace("(Clone)", string.Empty).Trim();
            int researchPointsValue = profile != null ? profile.researchPointValue : 1;

            HarvestTracker tracker = HarvestTracker.Instance;
            if (tracker != null)
                tracker.RecordHarvest(originPool.plantType, plantId, researchPointsValue);
            else
                Debug.LogWarning("HarvestTracker is missing in the scene; harvest was not tracked.");

            Debug.LogWarning("Harvesting: " + plantId);
            ResetPlant();
            originPool.Return(gameObject);
        }
    }


    void ResetPlant()
    {
        SetOutlineHidden();

        stepBounceTween.Rewind();
        finalBounceTween.Rewind();
        rotateTween.Rewind();
        scaleTween.Rewind();

        transform.localScale = profile.startScale;
        transform.rotation = startRotation;

        currentScale = startScale.x;
        nextGrowthTimer = 0f;
        gameObject.layer = startingLayer;
        hasFullyGrown = false;
    }

    public void SetOutlineHidden()
    {
        if(outline == null)
        {
            return;
        }
        outline.OutlineMode = Outline.Mode.OutlineHidden;
        outline.OutlineColor = Color.yellow;

    }
}