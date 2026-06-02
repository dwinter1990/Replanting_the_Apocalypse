using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using CS.AudioToolkit;

public class WaterHose : MonoBehaviour
{
    [Header("Particle System Settings")]
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private Transform nozzle;

    [Header("Hit Detection Settings")]
    [SerializeField] private float range = 8f;
    [SerializeField] private float coneAngle = 25f;
    [SerializeField] private float sprayInterval = 0.25f;
    [SerializeField] private LayerMask plantMask;

    [Header("WaterGun Animation")]
    [SerializeField] private Animator waterGunAnim;
    private Coroutine sprayRoutine;
    private Collider[] plantBuffer = new Collider[64];
    private float coneDot;

    [Header("WaterGun Settings")]
    [SerializeField] private int steps = 6;
    private Vector3 origin;
    private Vector3 direction;
    private float maxDistance;
    private float maxRadius;
    private int seenCount = 0;
    private int[] seenIds = new int[64]; 
    private void Awake()
    {
        coneDot = Mathf.Cos(coneAngle * Mathf.Deg2Rad);


    }
    public void StartSpray()
    {
        if (sprayRoutine != null)
        {
            return;
        }

        if (!PlayerStats.PSInstance.HasWater())
        {
            return;
        }

        sprayRoutine = StartCoroutine(Spray());
        waterParticles.Play();
        AudioController.Play("SprayWater");
    }

    public void StopSpray()
    {
        if (sprayRoutine != null)
        {
            StopCoroutine(sprayRoutine);
            sprayRoutine = null;
            AudioController.Stop("SprayWater"); 
        }

        waterGunAnim.SetBool("isFiring", false);
        waterParticles.Stop();
    }

    IEnumerator Spray()
    {
        WaitForSeconds wait = new WaitForSeconds(sprayInterval);

        while (true)
        {
            if (!PlayerStats.PSInstance.HasWater())
            {
                StopSpray();
                yield break;
            }

            PlayerStats.PSInstance.UseWater();

            if (!PlayerStats.PSInstance.HasWater())
            {
                StopSpray();
                yield break;
            }
            waterGunAnim.SetBool("isFiring", true);
            FireCone();

            yield return wait;
        }
    }

    void FireCone()
    {
        Vector3 origin = nozzle.position;
        Vector3 direction = nozzle.forward;

        float maxDistance = range;
        float maxRadius = range * 0.5f;

        int steps = 6; // tweak for performance vs accuracy
        int seenCount = 0;

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);

            Vector3 center = origin + direction * (t * maxDistance);

            float radius = Mathf.Lerp(0f, maxRadius, t);

            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                radius,
                plantBuffer,
                plantMask
            );

            for (int j = 0; j < hitCount; j++)
            {
                Collider col = plantBuffer[j];
                int id = col.GetInstanceID();

                // Duplicate check (non-alloc)
                bool alreadySeen = false;
                for (int k = 0; k < seenCount; k++)
                {
                    if (seenIds[k] == id)
                    {
                        alreadySeen = true;
                        break;
                    }
                }

                if (alreadySeen)
                    continue;
                if (seenCount < seenIds.Length)
                {
                    seenIds[seenCount++] = id;
                }
                seenIds[seenCount++] = id;

                Vector3 dirToTarget =
                    (col.bounds.center - origin).normalized;

                float dot = Vector3.Dot(direction, dirToTarget);

                if (dot >= coneDot)
                {
                    if (col.CompareTag("Plant"))
                    {
                        col.GetComponentInParent<Growing>()?.Water();
                    }
                    else if (col.CompareTag("Tool"))
                    {
                        if (col.TryGetComponent(out AutoWaterer autoWaterer))
                        {
                            autoWaterer.RefillWater();
                        }
                    }
                }
            }
        }
        Vector3 left = Quaternion.Euler(0, -coneAngle, 0) * direction;
        Vector3 right = Quaternion.Euler(0, coneAngle, 0) * direction;

        Debug.DrawRay(origin, left * range, Color.green, 0.1f);
        Debug.DrawRay(origin, right * range, Color.green, 0.1f);
        Debug.DrawRay(origin, direction * range, Color.blue, 0.1f);
    }
}