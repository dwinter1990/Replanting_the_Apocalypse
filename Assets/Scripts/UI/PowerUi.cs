using UnityEngine;
using UnityEngine.UI;   
public class PowerUi : MonoBehaviour
{
    [SerializeField] private Image powerFill;
    [SerializeField] private float animationSpeed = 5f;

    private float displayFilled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        powerFill.fillAmount = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        float current = PlayerStats.PSInstance.currentPower;
        float max = PlayerStats.PSInstance.maxPower;

        float targetFill = current / max;

        displayFilled = Mathf.Lerp(displayFilled, targetFill, Time.deltaTime * animationSpeed);

        powerFill.fillAmount = displayFilled;
    }
}
