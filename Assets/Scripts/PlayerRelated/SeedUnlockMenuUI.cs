using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    [SerializeField] private TMP_Text researchPointsLabel;
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

        ValidateUnlockButtons();
        HideMenu();
        RefreshResearchPointsLabel();
    }

    private void OnEnable()
    {
        PlayerStats.ResearchPointsChanged += HandleResearchPointsChanged;
    }

    private void OnDisable()
    {
        PlayerStats.ResearchPointsChanged -= HandleResearchPointsChanged;
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

    private void ValidateUnlockButtons()
    {
        if (unlockButtons == null)
            return;

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];

            if (entry == null || entry.button == null)
                continue;
        }
    }

    public void OnUnlockButtonPressed(int plantTypeIndex)
    {
        if (!System.Enum.IsDefined(typeof(PlantType), plantTypeIndex))
            return;

        OnUnlockClicked((PlantType)plantTypeIndex);
    }

    public void OnUnlockGrassButton()
    {
        OnUnlockClicked(PlantType.Grass);
    }

    public void OnUnlockFlowerButton()
    {
        OnUnlockClicked(PlantType.Flower);
    }

    public void OnUnlockBushButton()
    {
        OnUnlockClicked(PlantType.Bush);
    }

    public void OnUnlockTreeButton()
    {
        OnUnlockClicked(PlantType.Tree);
    }

    public void OnUnlockGrass(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnUnlockClicked(PlantType.Grass);
    }

    public void OnUnlockFlower(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnUnlockClicked(PlantType.Flower);
    }

    public void OnUnlockBush(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnUnlockClicked(PlantType.Bush);
    }

    public void OnUnlockTree(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnUnlockClicked(PlantType.Tree);
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

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];
            
            if (entry == null || entry.button == null)
                continue;

            int unlockCost = manager.GetUnlockCost(entry.plantType);

            bool canUnlockType = manager.CanUnlockType(entry.plantType);
            bool canAfford = stats.researchPoints >= unlockCost;
            entry.button.interactable = canUnlockType && canAfford;

            if (entry.label != null)
            {
                if (canUnlockType)
                {
                    int unlocked = manager.GetUnlockedCount(entry.plantType);
                    int total = manager.GetTotalCount(entry.plantType);
                    entry.label.text = "Unlock next " + entry.plantType + " seed (" + unlockCost + " RP)\n" + unlocked + "/" + total + " unlocked";
                }

                else
                    entry.label.text = entry.plantType + " Fully Unlocked";
            }
        }
    }
    private void HandleResearchPointsChanged(int currentResearchPoints)
    {
        RefreshResearchPointsLabel(currentResearchPoints);

        if (panelRoot != null && panelRoot.activeSelf)
            RefreshButtons();
    }

    private void RefreshResearchPointsLabel()
    {
        int currentResearchPoints = PlayerStats.Instance != null ? PlayerStats.Instance.researchPoints : 0;
        RefreshResearchPointsLabel(currentResearchPoints);
    }

    private void RefreshResearchPointsLabel(int currentResearchPoints)
    {
        if (researchPointsLabel == null)
            return;

        researchPointsLabel.text = "Research Points: " + currentResearchPoints;
    }

    private void OnUnlockClicked(PlantType type)
    {
        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;
        PlayerStats stats = PlayerStats.Instance;

        if (manager == null || stats == null)
            return;

        if (!manager.CanUnlockType(type))
            return;

        int unlockCost = manager.GetUnlockCost(type);

        if (!stats.TrySpendResearchPoints(unlockCost))
            return;

        if (!manager.TryUnlockNextPool(type))
            return;

        RefreshButtons();
    }
}
