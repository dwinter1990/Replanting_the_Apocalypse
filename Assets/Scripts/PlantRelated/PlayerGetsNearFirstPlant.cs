using UnityEngine;
using UnityEngine.UI;

public class PlayerGetsNearFirstPlant : MonoBehaviour
{
    [SerializeField] private ObjectivesTutorial objectivesTutorial; // Reference to the ObjectivesTutorial script
    private Collider triggerCollider; // Reference to the trigger collider
    public bool tutorialTriggered = false; // Flag to ensure the tutorial is triggered only once

    [SerializeField] private RawImage plantIsHereImage;
    private void Start()
    {
        if (objectivesTutorial == null)
            objectivesTutorial = FindAnyObjectByType<ObjectivesTutorial>();

        if(triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }

        if (plantIsHereImage == null)
        {
            plantIsHereImage = GameObject.FindWithTag("PlantIsHereImage").GetComponent<RawImage>();    
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

        plantIsHereImage.transform.position = transform.position + Vector3.up * 2f; // Position the image above the plant
        plantIsHereImage.gameObject.SetActive(true); // Show the "Plant is here" image

    }

    private void Update()
    {
        if(plantIsHereImage.gameObject.activeSelf)
        {
            plantIsHereImage.transform.rotation = Quaternion.LookRotation(plantIsHereImage.transform.position - Camera.main.transform.position); // Make the image face the camera
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && tutorialTriggered)
        {
            plantIsHereImage.gameObject.SetActive(false); // Hide the "Plant is here" image when the player leaves
        }
    }


}

