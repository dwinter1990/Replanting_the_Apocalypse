using UnityEngine;
using System.Collections;
using CS.AudioToolkit;
using UnityEngine.Video;
public class GameStartFade : MonoBehaviour
{
    private Fade fade;
    [SerializeField] private GameObject uiCanvas;
    [SerializeField] private Canvas introVideo;
    [SerializeField] private GameObject objectivesCanvas;
    [SerializeField] private VideoPlayer introVideoClip;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiCanvas.gameObject.SetActive(false);
        fade = GetComponent<Fade>();

        introVideo.enabled = false;
        introVideoClip.enabled = false;
        objectivesCanvas.SetActive(false);
        
        StartCoroutine(HoldForStart());

        fade.FadeIn(1f);
    }

    private IEnumerator HoldForStart()
    {
        //fade.FadeIn(30f);
        yield return new WaitForSeconds(1f);
        
        introVideo.enabled = true;
        introVideoClip.enabled = true;
        introVideoClip.Play();
        StartCoroutine(StartRoutine());
    }
    private IEnumerator StartRoutine()
    {
        fade.FadeIn(1f);
        yield return new WaitForSeconds(29f);

        introVideo.enabled = false;
        introVideoClip.Stop();
        introVideoClip.enabled = false;
        
        StartCoroutine(EndOfVideoSequence());

    }
    private IEnumerator EndOfVideoSequence()
    {
        yield return new WaitForSeconds(1f);
        uiCanvas.gameObject.SetActive(true);
        objectivesCanvas.SetActive(true);
        fade.FadeOut(3f);
        ObjectivesTutorial.OTInstance.StartCoroutine(ObjectivesTutorial.OTInstance.DelayedCanvasUp());
        StartCoroutine(Music());
    }
    private IEnumerator Music()
    {
        yield return new WaitForSeconds(2.5f);
        AudioController.PlayMusic("Music", 1f);
    }
}
