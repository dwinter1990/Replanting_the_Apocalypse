using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedUnlockMenuUI : MonoBehaviour
{
    public static SeedUnlockMenuUI Instance { get; private set; }

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button unlockButtonPrefab;
    [SerializeField] private Button closeButton;

    [SerializeField] private GameObject playerUiRoot;
    private readonly List<Button> spawnedButtons = new List<Button>();

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

        HideMenu();
    }

    public void ShowMenu()
    {
        if (PlantPoolManager.PlantPoolManagerInstance == null || PlayerStats.Instance == null)
        {
            Debug.LogWarning("Cannot open seed unlock menu. PlantPoolManager or PlayerStats is missing.");
            return;
        }
        playerUiRoot.SetActive(false);

        RebuildButtons();
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void HideMenu()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        playerUiRoot.SetActive(true);
    }

    private void RebuildButtons()
    {
        ClearButtons();

        if (buttonContainer == null || unlockButtonPrefab == null)
        {
            Debug.LogWarning("SeedUnlockMenuUI is missing button references.");
            return;
        }

        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;
        List<PlantType> unlockableTypes = manager.GetUnlockableTypes();
        int unlockCost = manager.ResearchPointsPerPoolUnlock;

        for (int i = 0; i < unlockableTypes.Count; i++)
        {
            PlantType type = unlockableTypes[i];
            Button button = Instantiate(unlockButtonPrefab, buttonContainer);
            spawnedButtons.Add(button);

            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = "Unlock " + type + " Seed (" + unlockCost + " RP)";

            bool canAfford = PlayerStats.Instance.researchPoints >= unlockCost;
            button.interactable = canAfford;
            button.onClick.AddListener(() => OnClick(type));
        }

        if (unlockableTypes.Count == 0)
            HideMenu();
    }

    private void OnClick(PlantType type)
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

        manager.TryUnlockNextPool(type);
        RebuildButtons();
    }

    private void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
                Destroy(spawnedButtons[i].gameObject);
        }

        spawnedButtons.Clear();
    }
}
