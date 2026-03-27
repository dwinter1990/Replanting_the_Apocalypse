using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

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

        if (!PlayerStats.Instance.HasWater())
        {
            return;
        }

        sprayRoutine = StartCoroutine(Spray());
        waterParticles.Play();
    }

    public void StopSpray()
    {
        if (sprayRoutine != null)
        {
            StopCoroutine(sprayRoutine);
            sprayRoutine = null;
        }

        waterGunAnim.SetBool("isFiring", false);
        waterParticles.Stop();
    }

    IEnumerator Spray()
    {
        WaitForSeconds wait = new WaitForSeconds(sprayInterval);

        while (true)
        {
            if (!PlayerStats.Instance.HasWater())
            {
                StopSpray();
                yield break;
            }

            PlayerStats.Instance.UseWater();

            if (!PlayerStats.Instance.HasWater())
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
        Vector3 center = nozzle.position + nozzle.forward * range * 0.55f;
        
        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            range * 0.5f,
            plantBuffer,
            plantMask
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = plantBuffer[i];

            Vector3 dirToTarget =
                (col.bounds.center - nozzle.position).normalized;

            float dot = Vector3.Dot(nozzle.forward, dirToTarget);

            if (dot >= coneDot)
            {
                if (col.CompareTag("Plant"))
                {
                    if (col.TryGetComponent(out Growing plant))
                    {
                        plant.Water();
                    }
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

        // Debug cone lines
        Vector3 left = Quaternion.Euler(0, -coneAngle, 0) * nozzle.forward;
        Vector3 right = Quaternion.Euler(0, coneAngle, 0) * nozzle.forward;

        Debug.DrawRay(nozzle.position, left * range, Color.green, 0.1f);
        Debug.DrawRay(nozzle.position, right * range, Color.green, 0.1f);
        Debug.DrawRay(nozzle.position, nozzle.forward * range, Color.blue, 0.1f);
    }
}