using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AutoWaterer : MonoBehaviour
{
    [Header("Water Particle System")]
    [SerializeField] private Transform waterSpawnPoint;
    [SerializeField] private ParticleSystem waterSpoutPS;
    private Coroutine SprayWater;
    [SerializeField] private float sprayDuration = 0.5f;

    [Header("Water Capacity Settings")]
    private float currentWaterTimer;
    [SerializeField] private int maxWaterCapacity;
    private int currentWaterCapacity;
    [SerializeField] private int waterDrain;

    [Header("Water Hit Detection")]
    [SerializeField] private float range;
    [SerializeField] private LayerMask plantLayerMask;
    private HashSet<Growing> wateredThisCycle = new HashSet<Growing>();
    private Collider[] plantBuffer = new Collider[30];
    private bool hasLanded;
    private bool isWateringActive;
    private bool outOfWaterTriggered;
    private int lastCompletedSprayLoop = -1;
    private Coroutine delayedStartRoutine;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private int animationLayer = 0;
    [SerializeField] private string sprayCycleStateName = "Active";
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string hasHitGroundTriggerName = "HasHitGround";
    [SerializeField] private string isWateringParamName = "IsWatering";
    [SerializeField] private string isOutOfWaterTriggerName = "IsOutOfWater";

    private void Awake()
    {
        currentWaterCapacity = maxWaterCapacity;

        animator = GetComponentInChildren<Animator>();

        waterSpoutPS.Stop();

    }

    private void Update()
    {
        if (!hasLanded || animator == null)
        {
            return;
        }

        if(currentWaterCapacity <= 0)
        {
            StopWateringBecauseEmpty();
            return;
        }

        if(!isWateringActive)
        {
            StartWateringAnimation();
        }

        TrySprayOnAnimationCycleEnd();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            animator.SetTrigger("HasHitGround");

            if (delayedStartRoutine != null)
            {
                StopCoroutine(delayedStartRoutine);
            }

            if (currentWaterCapacity > 0)
            {
                delayedStartRoutine = StartCoroutine(DelayedStartWatering());
            }
        }
    }

    IEnumerator DelayedStartWatering()
    {
        yield return new WaitForSeconds(2f); // Delay before starting to water
        
        if(currentWaterCapacity > 0)
        {
            StartWateringAnimation();
        }
        
    }

    private void StartWateringAnimation()
    {
        if(animator == null || currentWaterCapacity <= 0)
        {
            return;
        }
        animator.SetTrigger("IsWatering");
        isWateringActive = true;
        outOfWaterTriggered = false;
        lastCompletedSprayLoop = 0;
    }

    private void StopWateringBecauseEmpty()
    {
        if (waterSpoutPS != null)
        {
            waterSpoutPS.Stop();
        }

        if (!outOfWaterTriggered)
        {
            TrySetBool(isWateringParamName, false);
            TrySetTrigger(isOutOfWaterTriggerName);
            PlayIdleState();
            outOfWaterTriggered = true;
        }

        isWateringActive = false;
        lastCompletedSprayLoop = -1;
    }
    private bool TrySetTrigger(string triggerName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            return false;

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = animator.parameters[i];
            if (parameter.name != triggerName || parameter.type != AnimatorControllerParameterType.Trigger)
                continue;

            animator.SetTrigger(triggerName);
            return true;
        }

        return false;
    }

    private bool TrySetBool(string boolName, bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(boolName))
            return false;

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = animator.parameters[i];
            if (parameter.name != boolName || parameter.type != AnimatorControllerParameterType.Bool)
                continue;

            animator.SetBool(boolName, value);
            return true;
        }

        return false;
    }

    private void TrySprayOnAnimationCycleEnd()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
        if (!stateInfo.IsName(sprayCycleStateName))
            return;

        if(stateInfo.normalizedTime < 1f)
            return;

        int completedLoops = Mathf.FloorToInt(stateInfo.normalizedTime);
        if (completedLoops <= lastCompletedSprayLoop)
            return;

        lastCompletedSprayLoop = completedLoops;
        SprayOnce();
    }

    private void SprayOnce()
    {
        if (currentWaterCapacity <= 0)
        {
            StopWateringBecauseEmpty();
            return;
        }

        if (waterSpoutPS != null)
        {
            waterSpoutPS.Play();
            StartCoroutine(StopSprayAfterDelay());
        }

        currentWaterCapacity = Mathf.Max(0, currentWaterCapacity - waterDrain);
        WaterHitCheck();

        if (currentWaterCapacity <= 0)
        {
            StopWateringBecauseEmpty();
        }
    }
    private void PlayIdleState()
    {
        if (animator == null || string.IsNullOrWhiteSpace(idleStateName))
        {
            return;
        }
        
        animator.Play(idleStateName, animationLayer, 0f);
        animator.Update(0f);
    }
    private IEnumerator StopSprayAfterDelay()
    {
        yield return new WaitForSeconds(sprayDuration);

        if (waterSpoutPS != null)
        {
            waterSpoutPS.Stop();
        }
    }
    public void RefillWater()
    {
        bool wasEmpty = currentWaterCapacity <= 0;

        currentWaterCapacity = Mathf.Min(currentWaterCapacity + 1, maxWaterCapacity);
        Debug.Log("Water capacity: " + currentWaterCapacity);

        if (wasEmpty)
        {
            outOfWaterTriggered = false;

            if (hasLanded)
            {
                StartWateringAnimation();
            }
        }
    }


    public void WaterHitCheck()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            waterSpawnPoint.position,
            range,
            plantBuffer,
            plantLayerMask
            );

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = plantBuffer[i];
            if (col.CompareTag("Plant"))
            {
                Debug.Log("AutoWaterer hit a plant: " + col.name + " on " + gameObject.name);
                col.GetComponentInParent<Growing>()?.Water();
            }
        }
    }
}
