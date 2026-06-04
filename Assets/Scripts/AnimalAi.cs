using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAI : MonoBehaviour
{
    private enum MovementStyle
    {
        Continuous,
        HopStop
    }

    [Header("NavMesh Settings")]
    private Transform[] waypoints;
    [SerializeField] private float speed = 3.5f;
    private NavMeshAgent agent;
    [SerializeField] private PlantType preferredPlantType;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string movingParameterName = "isWalking";
    [SerializeField] private float movementAnimationDelay = 0.1f;

    [Header("Movement Style")]
    [SerializeField] private MovementStyle movementStyle = MovementStyle.Continuous;
    [SerializeField] private float hopMoveDuration = 0.35f;
    [SerializeField] private float hopIdleDuration = 0.2f;

    private float waitTime = 1f;
    private bool isWaitingAndChoosing;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null && animator.GetCurrentAnimatorClipInfo(0).Length > 0)
        {
            waitTime = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        }

        StartCoroutine(WaitAndMove());
    }

    private void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (isWaitingAndChoosing)
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    private IEnumerator WaitAndMove()
    {
        isWaitingAndChoosing = true;

        if (animator != null)
        {
            animator.SetBool(movingParameterName, false);
        }

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(waitTime);

        waypoints = GetWayPoints();

        if (waypoints.Length == 0)
        {
            isWaitingAndChoosing = false;
            yield break;
        }

        Transform target = waypoints[Random.Range(0, waypoints.Length)];

        if (movementStyle == MovementStyle.HopStop)
        {
            yield return HopStopMoveToTarget(target);
        }
        else
        {
            MoveContinuouslyToTarget(target);
        }

        isWaitingAndChoosing = false;
    }

    private void MoveContinuouslyToTarget(Transform target)
    {
        if (animator != null)
        {
            animator.SetBool(movingParameterName, true);
        }

        StartCoroutine(StartAgentAfterAnimationDelay(target));
    }

    private IEnumerator StartAgentAfterAnimationDelay(Transform target)
    {
        yield return new WaitForSeconds(movementAnimationDelay);

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            agent.isStopped = false;
        }
    }

    private IEnumerator HopStopMoveToTarget(Transform target)
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
        }

        yield return null;

        while (agent.enabled && agent.isOnNavMesh && agent.pathPending)
        {
            yield return null;
        }

        while (agent.enabled &&
               agent.isOnNavMesh &&
               agent.remainingDistance > agent.stoppingDistance)
        {
            if (animator != null)
            {
                animator.SetBool(movingParameterName, true);
            }

            agent.isStopped = false;

            yield return new WaitForSeconds(hopMoveDuration);

            agent.isStopped = true;

            if (animator != null)
            {
                animator.SetBool(movingParameterName, false);
            }

            yield return new WaitForSeconds(hopIdleDuration);
        }

        if (animator != null)
        {
            animator.SetBool(movingParameterName, false);
        }

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    private Transform[] GetWayPoints()
    {
        GameObject[] preferredPlants = GameObject.FindGameObjectsWithTag(preferredPlantType.ToString());
        List<Transform> targets = new List<Transform>(preferredPlants.Length);

        for (int i = 0; i < preferredPlants.Length; i++)
        {
            if (preferredPlants[i] != null)
            {
                targets.Add(preferredPlants[i].transform);
            }
        }

        return targets.ToArray();
    }
}