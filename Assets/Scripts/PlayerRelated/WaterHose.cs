using UnityEngine;
using UnityEngine.InputSystem;
public class WaterHose : MonoBehaviour
{
    [SerializeField] ParticleSystem waterParticles;
    private ParticleSystem.EmissionModule emission;
    [SerializeField] float waterUsageRate = 10f;
    [SerializeField] float emissionBaseRate = 50f;
    [SerializeField] float maxParticles = 200f;
    private bool isSpraying = false;

    private void Start()
    {
        emission = waterParticles.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(waterUsageRate);
    }
    private void Update()
    {
        if (!isSpraying)
        {
            return;
        } 
        if(PlayerStats.Instance.currentWaterCapacity <= 0)
        {
            StopSpray();
        }

        PlayerStats.Instance.UseWater(waterUsageRate);
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(waterUsageRate);
    }
    //public void OnAttack(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        StartSpray();
    //    }

    //    if (context.canceled)
    //    {
    //        StopSpray();
    //    }
    //}
    public void StartSpray()
    {
        if(PlayerStats.Instance.currentWaterCapacity <= 0)
        {
            return;
        }
        isSpraying = true;
        waterParticles.Play();
        
    }
    public void StopSpray()
    {
        isSpraying = false;
        if(waterParticles.isPlaying)
        {
            waterParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

}
