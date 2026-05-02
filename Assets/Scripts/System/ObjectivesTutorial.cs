using System.Collections;
using TMPro;
using UnityEngine;


public class ObjectivesTutorial : MonoBehaviour
{
    //This script will handle the tutorial, showing the player how to use each button and introduce the game's mechanics.

    [Header("Tutorial Settings")]
    [SerializeField] private Canvas tutorialCanvas; // The canvas group for the tutorial UI
    [SerializeField] private TextMeshProUGUI text;
    void Start()
    {
        StartTutorial();
    }

    private void StartTutorial()
    {
        // Start the tutorial sequence
        text.text = "Welcome to the game! Let's go through the basics.";

        StartCoroutine(TutorialSequence());
    }
    IEnumerator OneCharacterAtATime(string fullMessage)
    {
        text.text = "";
        foreach (char c in fullMessage)
        {
            text.text += c;
            yield return new WaitForSeconds(0.05f); // Adjust the speed of the text display here
        }
    }

    IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(OneCharacterAtATime("Use WASD to move around."));
        yield return new WaitForSeconds(3f);
        StartCoroutine(OneCharacterAtATime("You can use the mouse to look around."));
        yield return new WaitForSeconds(3f);
        StartCoroutine(OneCharacterAtATime("Now go find a plant!"));
    }
    public IEnumerator FirstPlantFound()
    {
        StopCoroutine(TutorialSequence()); // Stop the initial tutorial sequence if it's still running
        StartCoroutine(OneCharacterAtATime("Great job finding the first plant! Now let's learn how to interact with it."));
        yield return new WaitForSeconds(3f);
        StartCoroutine(OneCharacterAtATime("With the water gun equipped, you can water the plants by holding left mouse button."));
    }
}
