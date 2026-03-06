using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(TweenQueue))]
public class Growing : MonoBehaviour
{
    [SerializeField] public GrowthSO profile;
    private PlantPool originPool;

    [Header("Animation settings")]
    private TweenQueue tweenQueue;
   // private Tween growTween;
    private Tween stepBounceTween;
    private Tween finalBounceTween;
    private Tween rotateTween;

    public Vector3 startScale;
    public Quaternion startRotation;

    private float currentScale;
    private float maxScale;
    private float nextBounceThreshold;

    private bool isBeingWatered;
    private float growthProgress;
    private float waterTimer;
    private bool hasFullyGrown;
    private bool bounceInProgress;

    private int plantLayer;
    private int ignoreWaterLayer;

    private GameObject spawnedMound;
    private bool pendingHarvest;
    private void Awake()
    {
        tweenQueue = GetComponent<TweenQueue>();
        plantLayer = LayerMask.NameToLayer("Plants");
        ignoreWaterLayer = LayerMask.NameToLayer("IgnoreWater");
        
    }

    private void Start()
    {
        startScale = profile.startScale;
        startRotation = transform.rotation;

        transform.localScale = startScale;

        maxScale = Random.Range(
            profile.maxScale * 0.75f,
            profile.maxScale * 1.5f);

        currentScale = transform.localScale.x;
        nextBounceThreshold = currentScale + profile.growthStep;

        CreateTweens();
    }
    void OnDisable()
    {
        DOTween.Kill(transform);
    }
    private void CreateTweens()
    {
        // STEP BOUNCE (small squash + pop)
        stepBounceTween = DOTween.Sequence()
            .AppendCallback(() =>
            {
                float baseScale = transform.localScale.x;

                transform.DOScale(
                    new Vector3(baseScale, baseScale * 0.92f, baseScale),
                    0.08f);
            })
            .AppendInterval(0.08f)

            .AppendCallback(() =>
            {
                float baseScale = transform.localScale.x;

                transform.DOScale(
                    new Vector3(baseScale, baseScale * (1f + profile.stepOvershoot), baseScale),
                    0.12f);
            })
            .AppendInterval(0.12f)

            .AppendCallback(() =>
            {
                float baseScale = transform.localScale.x;

                transform.DOScale(
                    Vector3.one * baseScale,
                    0.15f).SetEase(Ease.OutBack, 2f);
            })
            .AppendInterval(0.15f)

            .OnComplete(() =>
            {
                bounceInProgress = false;
            })
            .Pause()
            .SetAutoKill(false);


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
        rotateTween = transform.DOPunchRotation(
            new Vector3(0, 0, profile.wobbleAmount),
            profile.finalDuration,
            8,
            0.7f)
            .Pause()
            .SetAutoKill(false);
    }
    public void SetPool(PlantPool pool)
    {
        originPool = pool;
    }
    private void OnParticleCollision(GameObject other)
    {
        if (hasFullyGrown)
        {
            return;
        }

        isBeingWatered = true;
        waterTimer = 0.15f;
    }

    private void Update()
    {
        if (hasFullyGrown)
            return;

        HandleWater();

        if (!isBeingWatered)
            return;

        growthProgress += profile.growthSpeed * Time.deltaTime;

        if (growthProgress >= profile.growthStep)
        {
            growthProgress = 0f;
            GrowOneStep();
        }
    }
    void GrowOneStep()
    {
        if (bounceInProgress || hasFullyGrown)
            return;

        currentScale += profile.growthStep;

        // Return mound once plant starts growing
        if (currentScale >= maxScale * 0.25f && spawnedMound != null)
        {
            MoundPool.instance.Return(spawnedMound);
            spawnedMound = null;
        }

        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
            hasFullyGrown = true;

            transform.DOScale(Vector3.one * currentScale, 0.35f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    finalBounceTween.Restart();
                    rotateTween.Restart();
                });

            gameObject.layer = ignoreWaterLayer;
            return;
        }

        bounceInProgress = true;

        transform.DOScale(Vector3.one * currentScale, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                stepBounceTween.Restart();
            });
    }
    private void HandleWater()
    {
        if (waterTimer > 0)
            waterTimer -= Time.deltaTime;
        else
            isBeingWatered = false;
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
            Debug.LogWarning("Harvesting: " + gameObject.name);
            ResetPlant();
            originPool.Return(gameObject);
        }
    }

    void ResetPlant()
    {
        stepBounceTween.Rewind();
        finalBounceTween.Rewind();
        rotateTween.Rewind();

        transform.localScale = profile.startScale;
        transform.rotation = startRotation;

        currentScale = startScale.x;
        nextBounceThreshold = currentScale + profile.growthStep;

        hasFullyGrown = false;
        isBeingWatered = false;
        bounceInProgress = false;
        pendingHarvest = false;
    }
}