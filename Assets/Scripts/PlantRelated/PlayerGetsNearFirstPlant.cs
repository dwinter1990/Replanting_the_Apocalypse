using UnityEngine;

public class PlayerGetsNearFirstPlant : MonoBehaviour
{
    [SerializeField] private ObjectivesTutorial objectivesTutorial; // Reference to the ObjectivesTutorial script
    private Collider triggerCollider; // Reference to the trigger collider
    private bool tutorialTriggered = false; // Flag to ensure the tutorial is triggered only once
    private void Start()
    {
        if (objectivesTutorial == null)
            objectivesTutorial = FindAnyObjectByType<ObjectivesTutorial>();

        if(triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tutorialTriggered)
        {
            Debug.Log("Player is near the first plant! Starting tutorial...");
            objectivesTutorial.FirstPlantFound();
            tutorialTriggered = true; // Set the flag to true to prevent retriggering
        }
    }
}

