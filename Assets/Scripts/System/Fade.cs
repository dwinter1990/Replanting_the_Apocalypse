using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public Image blackScreen;
    public float fadeSpeed = 1.0f;
    private bool isFading = false;
    private float targetAlpha;

    private void Start()
    {
        if (blackScreen == null)
        {
            Debug.LogError("Black Screen Image is not assigned.");
            enabled = false;
            return;
        }

        Color color = blackScreen.color;
        color.a = 1;
        blackScreen.color = color;
    }
    void Update()
    {
        if (isFading)
        {
            Color color = blackScreen.color;
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            blackScreen.color = color;

            if (color.a == targetAlpha)
                isFading = false;
        }
    }

    public void FadeIn(float duration)
    {
        targetAlpha = 1;
        fadeSpeed = duration > 0 ? 1.0f / duration : float.MaxValue;
        isFading = true;
        
    }

    public void FadeOut(float duration)
    {
        targetAlpha = 0;
        fadeSpeed = duration > 0 ? 1.0f / duration : float.MaxValue;
        isFading = true;
    }
}
