using UnityEngine;
using System.Collections;
using CS.AudioToolkit;
public class GameStartFade : MonoBehaviour
{
    private Fade fade;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fade = GetComponent<Fade>();
        fade.FadeOut();

        StartCoroutine(Music());
    }

    private IEnumerator Music()
    {
        yield return new WaitForSeconds(2.5f);
        AudioController.PlayMusic("Music", 1f);
    }
}
