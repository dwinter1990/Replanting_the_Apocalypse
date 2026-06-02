using UnityEngine;
using System.Collections;
using CS.AudioToolkit;
using UnityEngine.Video;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
public class GameStartFade : MonoBehaviour
{
    private Fade fade;
    [SerializeField] private GameObject uiCanvas;
    [SerializeField] private Canvas introVideo;
    [SerializeField] private GameObject objectivesCanvas;
    [SerializeField] private VideoPlayer introVideoClip;

    [Header("Player Managing")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayActionMapName = "Player";
    [SerializeField] private string skipActionName = "Skip";

    private InputAction skipAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiCanvas.gameObject.SetActive(false);
        fade = GetComponent<Fade>();

        introVideo.enabled = false;
        introVideoClip.enabled = false;
        objectivesCanvas.SetActive(false);

        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        LockInputToSkipOnly();

        StartCoroutine(HoldForStart());

        fade.FadeIn(1f);
    }
    private void LockInputToSkipOnly()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        playerInput.actions.Disable();

        skipAction = playerInput.actions.FindAction(skipActionName, false);

        if (skipAction == null)
        {
            Debug.LogWarning("Could not find Skip action on PlayerInput.");
            return;
        }

        skipAction.ApplyBindingOverride("<Keyboard>/escape");
        skipAction.Enable();
    }

    private void UnlockGameplayInput()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        playerInput.actions.Enable();
        playerInput.SwitchCurrentActionMap(gameplayActionMapName);
    }
    public void OnSkip(InputAction.CallbackContext context)
    {
        if (!context.performed || !introVideoClip.isPlaying)
            return;

        StopAllCoroutines();
        fade.FadeIn(1f);
        StartCoroutine(EndOfVideoSequence());
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
        StartCoroutine(EndOfVideoSequence());

    }
    private IEnumerator EndOfVideoSequence()
    {
        introVideo.enabled = false;
        introVideoClip.Stop();
        introVideoClip.enabled = false;

        UnlockGameplayInput();

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
