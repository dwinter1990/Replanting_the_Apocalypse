using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectivesTutorial : MonoBehaviour
{
    public static ObjectivesTutorial OTInstance { get; set; }

    [Header("Tutorial Settings")]
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private TextMeshProUGUI text;

    [Header("First plant triggerboxes")]
    [SerializeField] private Collider[] firstPlantTrigger;

    [Header("Typewriter")]
    [SerializeField] private float characterDelay = 0.05f;

    private bool tutorialStarted = false;
    private bool firstPlantFound = false;
    private bool firstPlantWatered = false;

    // Track running coroutines so we can stop them cleanly.
    private Coroutine tutorialFlowRoutine;
    private Coroutine typewriterRoutine;

    private void Awake()
    {
        if (OTInstance != null && OTInstance != this)
        {
            Destroy(this);
        }
        else
        {
            OTInstance = this;
        }
    }


    private void Start()
    {
        foreach (Collider trigger in firstPlantTrigger)
        {
            if (trigger != null)
            {
                trigger.enabled = true;
                Debug.Log(trigger.name + " trigger enabled.");
            }
            else
            {
                Debug.LogError("One of the first plant triggers is not assigned in the inspector.");
            }
        }

        if (!tutorialStarted)
        {
            StartTutorial();
        }
    }

    private void StartTutorial()
    {
        tutorialStarted = true;
        tutorialFlowRoutine = StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        yield return ShowMessage("Welcome to the game! Let's go through the basics.", 3f);
        yield return ShowMessage("Use WASD to move around.", 3f);
        yield return ShowMessage("You can use the mouse to look around.", 3f);
        yield return ShowMessage("Now go find a plant!", 0f);
    }

    private IEnumerator ShowMessage(string message, float holdTime)
    {
        // Stop any currently running typewriter first.
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }

        typewriterRoutine = StartCoroutine(TypeText(message));
        yield return typewriterRoutine; // Wait until full sentence is typed.

        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);
    }

    private IEnumerator TypeText(string fullMessage)
    {
        text.text = string.Empty;

        foreach (char c in fullMessage)
        {
            text.text += c;
            yield return new WaitForSeconds(characterDelay);
        }

        typewriterRoutine = null;
    }

    private void StopTutorialCoroutines()
    {
        if (tutorialFlowRoutine != null)
        {
            StopCoroutine(tutorialFlowRoutine);
            tutorialFlowRoutine = null;
        }

        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
    }

    public void FirstPlantFound()
    {
        if (firstPlantFound) return;
        firstPlantFound = true;
        foreach (Collider trigger in firstPlantTrigger)
        {
            if (trigger != null)
            {
                trigger.enabled = false;
                Debug.Log(trigger.name + " trigger disabled.");
            }
        }
        // Player jumped ahead => stop current tutorial flow/text immediately.
        StopTutorialCoroutines();

        tutorialFlowRoutine = StartCoroutine(FirstPlantFoundCoroutine());
    }

    private IEnumerator FirstPlantFoundCoroutine()
    {
        foreach (Collider trigger in firstPlantTrigger)
        {
            if (trigger != null)
            {
                trigger.enabled = false;
                Debug.Log(trigger.name + " trigger disabled.");
            }
        }

        yield return ShowMessage("Great job finding the first plant! Now let's learn how to interact with it.", 5f);
        yield return ShowMessage("With the water gun equipped, water plants by holding down the left mouse button.", 0f);
    }

    public void FirstPlantFullyWatered()
    {
        if (firstPlantWatered) return;
        firstPlantWatered = true;

        // Player jumped ahead => stop any current step.
        StopTutorialCoroutines();

        tutorialFlowRoutine = StartCoroutine(FirstPlantFullyWateredCoroutine());
    }

    public void FirstPlantHarvested()
    {
        // Player completed the tutorial => stop all coroutines and clear text.
        StopTutorialCoroutines();
        text.text = string.Empty;
        tutorialFlowRoutine = StartCoroutine(FirstPlantHavestedCororoutine());
    }

    private IEnumerator FirstPlantHavestedCororoutine()
    {
        yield return ShowMessage("Congratulations on harvesting your first plant. You're now ready to start replanting the apocalypse!", 5f);
        PlayerInteraction.PIInstance.canShootSeed = true;
        yield return ShowMessage("With the Seed Launcher equipped in your left hand, press the Right Mouse Button to launch a seed", 0f);
    }
    private IEnumerator FirstPlantFullyWateredCoroutine()
    {
        yield return ShowMessage("Well done! You've fully watered the first plant! Now it's time to harvest!", 5f);

        if (HandManager.HMInstance != null)
        {
            HandManager.HMInstance.canSwapRight = true;
        }
        else
        {
            Debug.LogError("HandManager.HMInstance is null. Cannot enable right-hand swapping.");
        }

        yield return ShowMessage("To harvest a plant, switch to the chainsaw by pressing 2.", 0f);
    }
}