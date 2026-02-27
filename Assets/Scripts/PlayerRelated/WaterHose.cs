using UnityEngine;
using UnityEngine.InputSystem;
public class WaterHose : MonoBehaviour
{
    [SerializeField] ParticleSystem waterParticles;


    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            waterParticles.Play();
        }

        if (!context.performed && waterParticles.isPlaying)
        {
            waterParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
