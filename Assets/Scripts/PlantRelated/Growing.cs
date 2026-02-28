using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(TweenQueue))]
public class Growing : MonoBehaviour
{
    [SerializeField] private GrowthSO profile;

    private TweenQueue tweenQueue;

    private float currentScale;
    private float maxScale;
    private float nextBounceThreshold;

    private bool isBeingWatered;
    private float waterTimer;
    private bool hasFullyGrown;

    private int plantLayer;
    private int ignoreWaterLayer;
    private void Awake()
    {

        tweenQueue = GetComponent<TweenQueue>();
        plantLayer = LayerMask.NameToLayer("Plants");
        ignoreWaterLayer = LayerMask.NameToLayer("IgnoreWater");
    }

    private void Start()
    {
        maxScale = Random.Range(
            profile.maxScale * 0.75f,
            profile.maxScale * 1.5f);

        currentScale = transform.localScale.x;
        nextBounceThreshold = currentScale + profile.growthStep;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (this.hasFullyGrown)
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

        Grow();
    }

    private void HandleWater()
    {
        if (waterTimer > 0)
            waterTimer -= Time.deltaTime;
        else
            isBeingWatered = false;
    }

    private void Grow()
    {
        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
            SetScale(currentScale);
            FinalBounce();
            return;
        }

        currentScale += profile.growthSpeed * Time.deltaTime;
        currentScale = Mathf.Min(currentScale, maxScale);

        SetScale(currentScale);

        if (currentScale >= nextBounceThreshold)
        {
            StepBounce();
            nextBounceThreshold += profile.growthStep;
        }
    }

    private void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    private void StepBounce()
    {
        float baseScale = currentScale;

        var seq = tweenQueue.CreateSequence("Scale");

        seq.Append(transform.DOScale(
            new Vector3(baseScale, baseScale * 0.92f, baseScale),
            0.08f));

        seq.Append(transform.DOScale(
            new Vector3(baseScale, baseScale * (1f + profile.stepOvershoot), baseScale),
            0.12f));

        seq.Append(transform.DOScale(
            Vector3.one * baseScale,
            0.15f).SetEase(Ease.OutBack, 2f));
    }

    private void FinalBounce()
    {
        if (hasFullyGrown)
            return;

        hasFullyGrown = true;

        tweenQueue.KillChannel("Scale");
        tweenQueue.KillChannel("Rotate");

        Vector3 baseScale = Vector3.one * maxScale;

        var seq = tweenQueue.CreateSequence("Scale");

        seq.Append(transform.DOScale(
            baseScale * (1f + profile.finalOvershoot),
            profile.finalDuration * 0.4f).SetEase(Ease.OutQuad));

        seq.Append(transform.DOScale(
            baseScale,
            profile.finalDuration * 0.6f).SetEase(Ease.OutBack, 2f));

        tweenQueue.CreateTween(
            transform.DOPunchRotation(
                new Vector3(0, 0, profile.wobbleAmount),
                profile.finalDuration,
                8,
                0.7f),
            "Rotate");

        gameObject.layer = ignoreWaterLayer;
        Debug.Log(gameObject.name + " has gone to the " + gameObject.layer + " layer!");
    }
}