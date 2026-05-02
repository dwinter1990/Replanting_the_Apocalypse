using UnityEngine;

public class PlayerGetsNearFirstPlant : MonoBehaviour
{
    [SerializeField] private ObjectivesTutorial objectivesTutorial; // Reference to the ObjectivesTutorial script
    private void Start()
    {
            objectivesTutorial = FindAnyObjectByType<ObjectivesTutorial>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is near the first plant! Starting tutorial...");
            objectivesTutorial.FirstPlantFound();
        }
    }
}

