using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
public class Dropped : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private ParticleSystem impactEffect;

    private CinemachineImpulseSource impulseSource;
    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.down * speed, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // Stop the object from moving
                rb.isKinematic = true; // Make it kinematic to prevent further physics interactions

                // Spawn mound at same position
                GameObject mound = MoundPool.instance.Get();
                mound.transform.localScale = Vector3.one * 60f; // Adjust scale as needed
                mound.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                mound.transform.position = transform.position + new Vector3(0, -0.5f, 0); // Slightly above the ground to prevent clipping

                Instantiate(impactEffect, transform.position, impactEffect.transform.rotation); // Spawn the impact effect
                impactEffect.Play(); // Play the impact effect

                impulseSource.GenerateImpulse(); // Trigger the camera shake
                //CheckPlayerDistance();
            }
        }
    }
}
