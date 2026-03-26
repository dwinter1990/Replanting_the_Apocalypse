using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public Image blackScreen;
    public float fadeSpeed = 1.0f;
    private bool isFading = false;
    private float targetAlpha = 0;

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

    public void FadeIn()
    {
        targetAlpha = 1;
        isFading = true;
    }

    public void FadeOut()
    {
        targetAlpha = 0;
        isFading = true;
    }
}
