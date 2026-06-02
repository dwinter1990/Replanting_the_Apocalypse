using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AnimalAI : MonoBehaviour
{
    [Header("NavMesh Settings")]
    private Transform[] waypoints;
    [SerializeField] private float speed = 3.5f;
    private NavMeshAgent agent;
    [SerializeField] private PlantType preferredPlantType; // The type of plant this animal prefers to eat

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    private float waitTime = 1f;
    private bool isWaitigAndChoosing;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        waitTime = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length; // Set wait time to the length of the current animation clip
        StartCoroutine("WaitAndMove");
    }

    private void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh) 
        { 
            return; 
        }

        if (isWaitigAndChoosing)
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    IEnumerator WaitAndMove()
    {
        isWaitigAndChoosing = true;
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(waitTime);

        
        waypoints = GetWayPoints();
        Transform target = waypoints[Random.Range(0, waypoints.Length)];

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            agent.isStopped = false;
        }

        animator.SetBool("isWalking", true);

        isWaitigAndChoosing = false;
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


