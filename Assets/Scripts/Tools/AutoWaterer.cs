using System.Collections;
using UnityEngine;

public class AutoWaterer : MonoBehaviour
{
    [Header("Water FX")]
    [SerializeField] private Transform waterSpawnPoint;
    [SerializeField] private ParticleSystem waterSpoutPS;
    [SerializeField] private float sprayDuration = 0.5f;

    [Header("Water Capacity")]
    [SerializeField] private int maxWaterCapacity = 5;
    [SerializeField] private int waterDrain = 1;

    [Header("Plant Detection")]
    [SerializeField] private float range = 2f;
    [SerializeField] private LayerMask plantLayerMask;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private int animationLayer = 0;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string activeStateName = "Activation";
    [SerializeField] private string shutdownStateName = "Shutdown";
    [SerializeField] private string hasHitGroundTriggerName = "HasHitGround";
    [SerializeField] private string isWateringBoolName = "IsWatering";
    [SerializeField] private string outOfWaterTriggerName = "IsOutOfWater";

    private readonly Collider[] plantBuffer = new Collider[30];

    private int currentWaterCapacity;
    private int lastCompletedActiveLoop = -1;

    private bool hasLanded;
    private bool waitingForRefill;
    private bool pendingEmptyAfterSpray;

    private Coroutine sprayRoutine;

    private void Awake()
    {
        currentWaterCapacity = maxWaterCapacity;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (waterSpoutPS != null)
        {
            waterSpoutPS.Stop();
        }
    }

    private void Update()
    {
        if (!hasLanded || waitingForRefill || animator == null)
        {
            return;
        }

        TrySprayOnActiveLoopEnd();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        hasLanded = true;
        SetBoolOrTrigger(hasHitGroundTriggerName, true);

        if (currentWaterCapacity > 0)
        {
            StartWatering();
        }
        else
        {
            EnterEmptyState();
        }
    }

    private void StartWatering()
    {
        if (animator == null || currentWaterCapacity <= 0)
        {
            return;
        }

        waitingForRefill = false;
        //pendingEmptyAfterSpray = false;
        lastCompletedActiveLoop = 0;

        SetBoolOrTrigger(outOfWaterTriggerName, false);
        SetBoolOrTrigger(isWateringBoolName, true);
    }

    private void TrySprayOnActiveLoopEnd()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
        if (!stateInfo.IsName(activeStateName) || stateInfo.normalizedTime < 1f)
        {
            return;
        }

        int completedLoops = Mathf.FloorToInt(stateInfo.normalizedTime);
        if (completedLoops <= lastCompletedActiveLoop)
        {
            return;
        }

        lastCompletedActiveLoop = completedLoops;
        SprayOnce();
    }

    private void SprayOnce()
    {
        if (currentWaterCapacity <= 0)
        {
            EnterEmptyState();
            return;
        }

        PlaySprayFX();

        currentWaterCapacity = Mathf.Max(0, currentWaterCapacity - waterDrain);
        WaterHitCheck();

        if (currentWaterCapacity <= 0)
        {
            //pendingEmptyAfterSpray = true;
            EnterEmptyState();
        }
    }

    private void PlaySprayFX()
    {
        if (waterSpoutPS == null)
        {
            return;
        }

        waterSpoutPS.Play();

        if (sprayRoutine != null)
        {
            StopCoroutine(sprayRoutine);
        }

        sprayRoutine = StartCoroutine(StopSprayAfterDelay());
    }

    private IEnumerator StopSprayAfterDelay()
    {
        yield return new WaitForSeconds(sprayDuration);

        if (waterSpoutPS != null)
        {
            waterSpoutPS.Stop();
        }

        sprayRoutine = null;

        //if (pendingEmptyAfterSpray)
        //{
        //    EnterEmptyState();
        //}
    }

    private void EnterEmptyState()
    {
        waitingForRefill = true;
        lastCompletedActiveLoop = -1;

        if (animator == null)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
        bool isCurrentlyActive = stateInfo.IsName(activeStateName);

        SetBoolOrTrigger(isWateringBoolName, false);
        SetBoolOrTrigger(hasHitGroundTriggerName, false);

        if (isCurrentlyActive)
        {
            SetBoolOrTrigger(outOfWaterTriggerName, true);

            if (!string.IsNullOrWhiteSpace(shutdownStateName))
            {
                animator.Play(shutdownStateName, animationLayer, 0f);
                animator.Update(0f);
            }
        }
    }

    private void SetBoolOrTrigger(string paramName, bool boolValue)
    {
        if (animator == null || string.IsNullOrWhiteSpace(paramName))
        {
            return;
        }

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = animator.parameters[i];
            if (parameter.name != paramName)
            {
                continue;
            }

            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(paramName, boolValue);
                return;
            }

            if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                if (boolValue)
                {
                    animator.SetTrigger(paramName);
                }
                else
                {
                    animator.ResetTrigger(paramName);
                }

                return;
            }
        }
    }

    public void RefillWater()
    {
        bool wasEmpty = currentWaterCapacity <= 0;
        currentWaterCapacity = Mathf.Min(currentWaterCapacity + 1, maxWaterCapacity);

        if (!wasEmpty || !hasLanded || animator == null)
        {
            return;
        }

        SetBoolOrTrigger(hasHitGroundTriggerName, true);
        StartWatering();
    }
    private void WaterHitCheck()
    {
        if (waterSpawnPoint == null)
        {
            return;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(
            waterSpawnPoint.position,
            range,
            plantBuffer,
            plantLayerMask);

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = plantBuffer[i];
            if (!col.CompareTag("Plant"))
            {
                continue;
            }

            col.GetComponentInParent<Growing>()?.Water();
        }
    }
}
