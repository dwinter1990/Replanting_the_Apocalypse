using System.Collections;
using UnityEngine;

public class SeedGrenade : MonoBehaviour
{
    [SerializeField] private float upForce;
    [SerializeField] private float seedLaunchForce;
    [SerializeField] private float seedDelay;
    [SerializeField] public ObjectPool pool;
    [SerializeField] public PayloadPool payloadPool;
    [SerializeField] private int seedsToLaunch;
    [SerializeField] GameObject payloadPrefab;


    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            //Initiate stage 2
            StartCoroutine(LaunchUpwards());
        }
    }
    IEnumerator LaunchUpwards()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        yield return new WaitForSeconds(0.25f);

        rb.AddForce(Vector3.up * upForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.25f);
        //rb.freezeRotation = true;
        //rb.constraints = RigidbodyConstraints.FreezePosition;
        //Initiate Stage 3
        StartCoroutine(SpraySeeds());
    }

    private IEnumerator SpraySeeds()
    {
        float angleStep = 360f / seedsToLaunch;
        float angle = 0f;

        Vector3 origin = transform.position;


        for (int i = 0; i < seedsToLaunch; i++)
        {
            
            GameObject seed = pool.GetObject();
            if (pool == null)
            {
                Debug.LogError("ObjectPool is NULL on SeedGrenade!");
                yield break;
            }
            if (seed == null)
            {
                Debug.LogError("Pool returned NULL!");
                yield break;
            }

            seed.transform.position = origin;
            seed.SetActive(true);

            float rad = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(
                Mathf.Cos(rad),
                0f,
                Mathf.Sin(rad)
            );

            Rigidbody rb = seed.GetComponent<Rigidbody>();

            if (rb == null)
            {
                Debug.LogError("Seed has no Rigidbody!");
                yield break;
            }

            // Constant force = no spiral
            rb.AddForce(direction * seedLaunchForce, ForceMode.Impulse);

            angle += angleStep;

            yield return new WaitForSeconds(seedDelay);
        }

        payloadPool.ReturnObject(gameObject);
    }
}
