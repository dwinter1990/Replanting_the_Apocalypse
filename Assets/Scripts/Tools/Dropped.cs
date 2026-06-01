using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
public class Dropped : MonoBehaviour
{
    [SerializeField] private float speed;
    private bool isDropping;
    [SerializeField] private ParticleSystem impactEffect;

    private CinemachineImpulseSource impulseSource;

    private Rigidbody rb;
    private Animator animator;
    private void Start()
    {
        isDropping = true;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            animator.enabled = false; // Disable the animator to prevent any animations from playing
        }
        impulseSource = GetComponent<CinemachineImpulseSource>();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
           // rb.AddForce(Vector3.down * speed, ForceMode.VelocityChange);
        }
        rb.isKinematic = false; // Ensure the object is affected by physics
        //transform.Translate(Vector3.down * 90);
    }

    private void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
        if (rb != null)
        {
            rb.isKinematic = false; // Ensure the object is affected by physics when enabled
        }
    }

    private void FixedUpdate()
    {
        if(isDropping)
        {
            rb.AddForce(Vector3.down * speed, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(HandleImpact());

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
            isDropping = false;
            Destroy(impactEffect.gameObject, 5f); // Destroy the impact effect after it has played
        }
    }

    IEnumerator HandleImpact()
    {
        yield return new WaitForSeconds(0.5f); // Wait a short moment to ensure the impact effect is visible
        animator.enabled = true; // Enable the animator to play the impact animation
        animator.SetBool("HasHitGround", true); // Play the specific impact animation
        yield return new WaitForSeconds(2f); // Wait for the animation to finish
        
    }
}
