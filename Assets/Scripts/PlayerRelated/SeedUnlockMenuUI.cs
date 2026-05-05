using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
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
    private class ToolUnlockButton
    {
        public ToolType toolType;
        public Button button;
        public TMP_Text label;
    }

    public static SeedUnlockMenuUI Instance { get; private set; }
    [Header("Player UI")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private PlayerInput playerInput;

    [Header("Research point shop UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private PlantTypeUnlockButton[] unlockButtons;
    [SerializeField] private ToolUnlockButton[] toolUnlockButtons;
    [SerializeField] private TMP_Text researchPointsLabel;

    private GameObject previouslySelectedObject;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisibility;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        ValidateUnlockButtons();
        HideMenu();
        RefreshResearchPointsLabel();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(HideMenu);
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
        if (PlantPoolManager.PlantPoolManagerInstance == null || PlayerStats.PSInstance == null)
        {
            Debug.LogWarning("Cannot open seed unlock menu. PlantPoolManager or PlayerStats is missing.");
            return;
        }
        previouslySelectedObject = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerUI != null)
        {
            playerUI.SetActive(false);

        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }

        RefreshButtons();
        RefreshResearchPointsLabel();
        SelectDefaultButton();

    }

    public void HideMenu()
    {
        
        if (playerUI != null)
            playerUI.SetActive(true);


        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");


        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisibility;

        RestorePreviousSelection();
    }

    private void ValidateUnlockButtons()
    {
        if (closeButton != null && closeButton.onClick.GetPersistentEventCount() == 0)
            closeButton.onClick.AddListener(HideMenu);


        if (unlockButtons == null)
            return;

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];

            if (entry?.button == null)
                continue;
        }
    }


    public void OnUnlockButtonPressed(int plantTypeIndex)
    {
        if (!System.Enum.IsDefined(typeof(PlantType), plantTypeIndex))
            return;

        OnUnlockClicked((PlantType)plantTypeIndex);
    }
    
    public void OnCancel(InputAction.CallbackContext context)
    {
        if(context.performed && panelRoot != null && panelRoot.activeSelf)
        {
            Debug.Log("Close button clicked");
            HideMenu();
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
        PlayerStats stats = PlayerStats.PSInstance;

        if (manager == null || stats == null)
            return;

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];
            
            if (entry?.button == null)
                continue;

            int unlockCost = manager.GetUnlockCost(entry.plantType);

            bool canUnlockType = manager.CanUnlockType(entry.plantType);
            bool canAfford = stats.researchPoints >= unlockCost;
            entry.button.interactable = canUnlockType && canAfford;

            if (entry.label == null)
                continue;
            
            if (canUnlockType)
            {
                int unlocked = manager.GetUnlockedCount(entry.plantType);
                int total = manager.GetTotalCount(entry.plantType);
                entry.label.text = "Unlock next " + entry.plantType + " seed (" + unlockCost + " RP)\n" + unlocked + "/" + total + " unlocked";
            }
            else
            {
                entry.label.text = entry.plantType + " Fully Unlocked!";
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
        int currentResearchPoints = PlayerStats.PSInstance != null ? PlayerStats.PSInstance.researchPoints : 0;
        RefreshResearchPointsLabel(currentResearchPoints);
    }

    private void RefreshResearchPointsLabel(int currentResearchPoints)
    {
        if (researchPointsLabel == null)
            return;

        researchPointsLabel.text = "Research Points: " + currentResearchPoints;
    }

    public void FirstPlantUnlock(PlantType type)
    {
        Debug.Log("Unlocking plant type: " + type);
        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;
        PlayerStats stats = PlayerStats.PSInstance;

        if (!manager.TryUnlockNextPool(type))
            return;
    }

    private void OnUnlock(ToolType type)
    {
        // Implement tool unlocking logic here, similar to plant unlocking
        Debug.Log("Tool unlock not implemented yet for " + type);
    }
    private void OnUnlockClicked(PlantType type)
    {
        PlantPoolManager manager = PlantPoolManager.PlantPoolManagerInstance;
        PlayerStats stats = PlayerStats.PSInstance;

        if (manager == null || stats == null)
            return;

        if (!manager.CanUnlockType(type))
            return;

        int unlockCost = manager.GetUnlockCost(type);

        if (!stats.TrySpendResearchPoints(unlockCost))
            return;

        if (!manager.TryUnlockNextPool(type))
            return;

        Debug.Log("button clicked");
        RefreshButtons();
        SelectDefaultButton();
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null)
            return;

        for (int i = 0; i < unlockButtons.Length; i++)
        {
            PlantTypeUnlockButton entry = unlockButtons[i];

            if (entry?.button != null && entry.button.interactable)
            {
                EventSystem.current.SetSelectedGameObject(entry.button.gameObject);
                return;
            }
        }

        if (closeButton != null)
            EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
    }

    private void RestorePreviousSelection()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(previouslySelectedObject);

    }
}
