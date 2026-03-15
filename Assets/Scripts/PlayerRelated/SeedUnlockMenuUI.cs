using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedUnlockMenuUI : MonoBehaviour
{
    [System.Serializable]
    private class PlantTypeUnlockButton
    {
        public PlantType plantType;
        public Button button;
        public TMP_Text label;
    }

    public static SeedUnlockMenuUI Instance { get; private set; }
    [Header("Player UI")]
    [SerializeField] private GameObject playerUI;

    [Header("Research point shop UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private PlantTypeUnlockButton[] unlockButtons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (closeButton != null)
            closeButton.onClick.AddListener(HideMenu);

        BindUnlockButtons();
        HideMenu();
    }

    public void ShowMenu()
    {
        if (PlantPoolManager.PlantPoolManagerInstance == null || PlayerStats.Instance == null)
        {
            Debug.LogWarning("Cannot open seed unlock menu. PlantPoolManager or PlayerStats is missing.");
            return;
        }
        playerUI.SetActive(false);

        RefreshButtons();
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void HideMenu()
    {
        playerUI.SetActive(true);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void BindUnlockButtons()
    {
        if (unlockButtons == null)
            return;

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];

            unlockButtons[i].button.transform.position = -35 * unlockButtons[i].button.transform.position.y;
            if (entry == null || entry.button == null)
                continue;

            PlantType capturedType = entry.plantType;
            entry.button.onClick.RemoveAllListeners();
            entry.button.onClick.AddListener(() => OnUnlockClicked(capturedType));
        }
    }

    private void RefreshButtons()
    {
        if (unlockButtons == null || unlockButtons.Length == 0)
        {
            Debug.LogWarning("SeedUnlockMenuUI has no unlock buttons configured.");
            return;
        }

        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;
        PlayerStats stats = PlayerStats.Instance;

        if (manager == null || stats == null)
            return;

        int unlockCost = manager.ResearchPointsPerPoolUnlock;

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];
            
            if (entry == null || entry.button == null)
                continue;

            bool canUnlockType = manager.CanUnlockType(entry.plantType);
            bool canAfford = stats.researchPoints >= unlockCost;
            entry.button.interactable = canUnlockType && canAfford;

            if (entry.label != null)
            {
                if (canUnlockType)
                    entry.label.text = "Unlock " + entry.plantType + " Seed (" + unlockCost + " RP)";
                else
                    entry.label.text = entry.plantType + " Fully Unlocked";
            }
        }
    }

    private void OnUnlockClicked(PlantType type)
    {
        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;
        PlayerStats stats = PlayerStats.Instance;

        if (manager == null || stats == null)
            return;

        int unlockCost = manager.ResearchPointsPerPoolUnlock;

        if (!manager.CanUnlockType(type))
            return;

        if (!stats.TrySpendResearchPoints(unlockCost))
            return;

        if (!manager.TryUnlockNextPool(type))
            return;

        RefreshButtons();
    }
}
