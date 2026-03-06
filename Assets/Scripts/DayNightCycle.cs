using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] Light sun;
    [SerializeField] float dayDuration;
    [SerializeField] Gradient ambientLightGradient;

    private float timeOfDay = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(sun == null) 
        {  
            return; 
        }

        timeOfDay += Time.deltaTime / dayDuration;
        if(timeOfDay > 1f)
        {
            timeOfDay = 0f;
        }

        sun.transform.rotation = Quaternion.Euler(timeOfDay * 360f, -90f, 0f);

        RenderSettings.ambientLight = ambientLightGradient.Evaluate(timeOfDay);
        DynamicGI.UpdateEnvironment();
    }
}
