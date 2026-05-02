using UnityEngine;
using UnityEngine.UI;
public class BubblesJitter : MonoBehaviour
{
    [SerializeField] private float jitterAmount = 0.1f; // Adjust this value to control the intensity of the jitter
    [SerializeField] private float scrollSpeed = 1f; // Adjust this value to control the speed of the jitter
    [SerializeField] private Vector2 scrollDirection = Vector2.up; // Adjust this value to control the direction of the jitter

    private RectTransform rectTransform;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 jitter = new Vector2(
            Mathf.PerlinNoise(Time.time * scrollSpeed, 0) - 0.5f,
            Mathf.PerlinNoise(0, Time.time * scrollSpeed) - 0.5f
        ) * jitterAmount;

        rectTransform.anchoredPosition += jitter + scrollDirection * scrollSpeed * Time.deltaTime;
    }
}
