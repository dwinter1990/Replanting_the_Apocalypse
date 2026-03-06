using UnityEngine;
using UnityEngine.UI;
public class WaterGlobeUI : MonoBehaviour
{
    [SerializeField] Image waterFill;
    [SerializeField] float animationSpeed =5f;

    private float displayFilled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        displayFilled = 1;
    }

    // Update is called once per frame
    void Update()
    {
        float current = PlayerStats.Instance.currentWaterCapacity;
        float max = PlayerStats.Instance.maxWaterCapacity;

        float targetFill = current / max;

        displayFilled = Mathf.Lerp(displayFilled, targetFill, Time.deltaTime * animationSpeed);

        waterFill.fillAmount = displayFilled;
    }
}
