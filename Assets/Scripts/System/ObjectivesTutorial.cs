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

    private bool firstPlantFullyWateredTriggered;
    private bool firstPlantHarvestedTriggered;
    private bool hasRunPlacerTutorialTriggered;
    private bool firstSeedShotTriggered;
    private bool researchPointsTriggered;

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
    public void TryTriggerFirstPlantFullyWatered()
    {
        if (firstPlantFullyWateredTriggered) return;
        firstPlantFullyWateredTriggered = true;
        FirstPlantFullyWatered();
    }
    public void FirstPlantFullyWatered()
    {
        if (firstPlantWatered) return;
        firstPlantWatered = true;

        // Player jumped ahead => stop any current step.
        StopTutorialCoroutines();

        tutorialFlowRoutine = StartCoroutine(FirstPlantFullyWateredCoroutine());
    }
    public void TryTriggerFirstPlantHarvested()
    {
        if (firstPlantHarvestedTriggered) return;
        firstPlantHarvestedTriggered = true;
        FirstPlantHarvested();
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

    public void TryShootSeedObjective()
    {
        if (firstSeedShotTriggered) return;
        firstSeedShotTriggered = true;
        FirstSeedShotTriggered();
    }

    private void FirstSeedShotTriggered()
    {
        // Player jumped ahead => stop current tutorial flow/text immediately.
        StopTutorialCoroutines();
        tutorialFlowRoutine = StartCoroutine(FirstSeedShotCoroutine());
    }
    private IEnumerator FirstSeedShotCoroutine()
    {
        yield return ShowMessage("Great job shooting your first seed! Now you can water the seedling.", 5f);
        yield return ShowMessage("When it's fully grown, you can harvest it and each harvest will gain you Research Points", 5f);
    }

    public void TryResearchPointsTriggered()
    {
        if (researchPointsTriggered) return;
        researchPointsTriggered = true;
        ResearchPointsTriggered();
    }

    private void ResearchPointsTriggered()
    {
        // Player jumped ahead => stop current tutorial flow/text immediately.
        StopTutorialCoroutines();
        tutorialFlowRoutine = StartCoroutine(ResearchPointsCoroutine());
    }

    private IEnumerator ResearchPointsCoroutine()
    {
        yield return ShowMessage("Research Points are used to unlock new tools and abilities. You can access the Research Menu by heading to the drop pod computer.", 5f);
        yield return ShowMessage("Try it out now and see what you can unlock!", 0f);
    }
    public void GrenadeTutorial()
    {
        // Player jumped ahead => stop current tutorial flow/text immediately.
        StopTutorialCoroutines();
        tutorialFlowRoutine = StartCoroutine(GrenadeTutorialCoroutine());
    }

    private IEnumerator GrenadeTutorialCoroutine()
    {
        yield return ShowMessage("You've unlocked the Seed Grenade! This powerful tool allows you to plant a seed that will explode after a short delay planting new seedlings in an area.", 5f);
        yield return ShowMessage("To switch to the Seed Grenade, press the Q key while the Seed Launcher is equipped.", 4f);
        yield return ShowMessage("Then, if you want to launch a single seed, press Q again. You can swap back and forth as much as you like.", 0f);
    }

    public void TryPlacerTutorial()
    {
        if (hasRunPlacerTutorialTriggered)
        { 
            return; 
        }
        hasRunPlacerTutorialTriggered = true;
        PlacerTutorial();
    }
    public void PlacerTutorial()
    {
        // Player jumped ahead => stop current tutorial flow/text immediately.
        StopTutorialCoroutines();
        tutorialFlowRoutine = StartCoroutine(PlacerTutorialCoroutine());
        HandManager.HMInstance.canSwapLeft = true;
    }

    private IEnumerator PlacerTutorialCoroutine()
    {
        yield return ShowMessage("You've unlocked the Placer! This handy tool allows you to call down equipment from the ship in orbit", 5f);
        yield return ShowMessage("To equip the Placer, press the 1 key to swap back and forth with your Seed Launcher.", 4f);
        yield return ShowMessage("Then, when you've found a suitable spot, use the Right Mouse Click and we'll send the payload hurtling toward your location!", 0f);
    }
}