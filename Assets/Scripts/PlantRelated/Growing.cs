using UnityEngine;
using DG.Tweening;

public class Growing : MonoBehaviour
{
    [Header("Growth")]
    [SerializeField] private float growthSpeed = 0.5f;
    [SerializeField] private float growthStep = 0.15f;
    [SerializeField] private float maxScale = 2f;

    [Header("Juice")]
    [SerializeField] private float squashAmount = 0.15f;
    [SerializeField] private float overshoot = 0.25f;

    [Header("Final Bounce")]
    [SerializeField] private float finalOvershoot = 0.5f;
    [SerializeField] private float finalDuration = 0.4f;

    private bool isBeingWatered;
    private float waterTimer;

    private float nextBounceThreshold;
    private bool hasFullyGrown = false;

    private void Start()
    {
        nextBounceThreshold = transform.localScale.x + growthStep;
    }

    private void OnParticleCollision(GameObject other)
    {
        isBeingWatered = true;
        waterTimer = 0.15f;
    }

    private void Update()
    {
        if (hasFullyGrown)
            return;

        // Water timeout
        if (waterTimer > 0)
            waterTimer -= Time.deltaTime;
        else
            isBeingWatered = false;

        if (!isBeingWatered)
            return;

        if (transform.localScale.x >= maxScale)
        {
            transform.localScale = Vector3.one * maxScale;
            TriggerFinalBounce();
            return;
        }

        // Smooth growth
        float growAmount = growthSpeed * Time.deltaTime;
        transform.localScale += Vector3.one * growAmount;

        if (transform.localScale.x > maxScale)
            transform.localScale = Vector3.one * maxScale;

        // Step bounce
        if (transform.localScale.x >= nextBounceThreshold)
        {
            PlayJuicyBounce();
            nextBounceThreshold += growthStep;
        }
    }

    private void PlayJuicyBounce()
    {
        Vector3 baseScale = transform.localScale;

        Sequence step = DOTween.Sequence();

        step.Append(transform.DOScale(
            new Vector3(baseScale.x + squashAmount,
                        baseScale.y - squashAmount,
                        baseScale.z + squashAmount),
            0.1f));

        step.Append(transform.DOScale(
            baseScale + Vector3.up * overshoot,
            0.1f));

        step.Append(transform.DOPunchScale(
            baseScale,
            0.15f,5,0.5f).SetEase(Ease.OutBack));

        step.Join(transform.DOPunchRotation(
            new Vector3(0, 0, 5f),
            0.2f,
            5,
            0.5f));
    }

    private void TriggerFinalBounce()
    {
        if (hasFullyGrown)
            return;

        hasFullyGrown = true;

        Sequence final = DOTween.Sequence();

        final.Append(transform.DOScale(
            Vector3.one * (maxScale * 0.9f),
            finalDuration * 0.3f));

        final.Append(transform.DOPunchScale(
            Vector3.one * (maxScale + finalOvershoot),
            finalDuration * 0.4f).SetEase(Ease.OutQuad));

        final.Append(transform.DOPunchScale(
            Vector3.one * maxScale,
            finalDuration * 0.3f, 5, 0.5f).SetEase(Ease.OutBack, 2f));

        final.Join(transform.DOPunchRotation(
            new Vector3(0, 0, 15f),
            finalDuration,
            8,
            0.6f));
    }
}