using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ShowComic : MonoBehaviour
{
    [SerializeField] private GameObject comic0, comic1, comic2;
    [SerializeField] private float comicHoldTimer;
    [SerializeField] Fade fade;
    private void Start()
    {
        fade = GetComponent<Fade>();
        fade.FadeOut();
        comic0.SetActive(false);
        comic1.SetActive(false);
        comic2.SetActive(false);
        StartCoroutine("ShowComicStrip");
    }
    IEnumerator ShowComicStrip()
    {
        yield return new WaitForSeconds(comicHoldTimer);
        comic0.SetActive(true);
        yield return new WaitForSeconds(comicHoldTimer);
        comic1.SetActive(true);
        yield return new WaitForSeconds(comicHoldTimer);
        comic2.SetActive(true);
        yield return new WaitForSeconds(3f);

        fade.FadeIn();
        yield return new WaitForSeconds(fade.fadeSpeed);
        SceneManager.LoadScene("PlayerControllerScene");
    }
}
