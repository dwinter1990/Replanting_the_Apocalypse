using UnityEngine;

public class HarvestToResearchTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (UnlockShop.USInstance != null)
        {
            UnlockShop.USInstance.ShowMenu();
        }
        else
        {
            Debug.LogWarning("UnlockShop instance is missing; unlock menu was not shown.");
        }

        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("PlayerStats instance is missing. Unable to award research points.");
            return;
        }

        HarvestTracker tracker = HarvestTracker.Instance;
        if (tracker == null)
        {
            Debug.LogWarning("HarvestTracker is missing in the scene. Unable to convert harvested plants.");
            return;
        }

        int awardedResearchPoints = tracker.ConvertAllBankedHarvestToResearchPoints();

        if (awardedResearchPoints <= 0)
        {
            Debug.Log("No harvested plants to convert into research points.");
            return;
        }

        PlayerStats.Instance.AddResearchPoints(awardedResearchPoints);
        Debug.Log("Converted harvested plants to research points: " + awardedResearchPoints);
    }
}
