using UnityEngine;

public class GameStartFade : MonoBehaviour
{
    private Fade fade;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fade = GetComponent<Fade>();
        fade.FadeOut();
    }
}
