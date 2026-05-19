using CS.AudioToolkit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnlockShop : MonoBehaviour
{
    private enum ShopItemType
    {
        GrenadeUnlock,
        AutoSprinklerCharge,
        WaterRefillStationCharge,
        PlayerWaterCapacityUpgrade
    }

    [System.Serializable]
    private class ShopUnlockButton
    {
        public ShopItemType itemType;
        public int researchPointCost = 1;
        public Button button;
        public TMP_Text label;
        public TMP_Text costLabel;
    }

    public static UnlockShop USInstance { get; private set; }

    [Header("Player UI")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private PlayerInput playerInput;

    [Header("Research point shop UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private ShopUnlockButton[] shopUnlockButtons;
    [SerializeField] private TMP_Text[] costLabels;
    [SerializeField] private TMP_Text researchPointsLabel;

    private GameObject previouslySelectedObject;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisibility;

    [Header("Animations")]
    [SerializeField] private VideoSelector videoSelector;
    private void Awake()
    {
        if (USInstance != null && USInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        USInstance = this;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HideMenu);
            closeButton.onClick.AddListener(HideMenu);
        }

        WireShopButtons();
        HideMenu();
        RefreshResearchPointsLabel();
        InitializeCostLabels();
    }

    private void OnDestroy()
    {
        if (USInstance == this)
            USInstance = null;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(HideMenu);
    }

    private void OnEnable()
    {
        PlayerStats.ResearchPointsChanged += HandleResearchPointsChanged;
        InitializeCostLabels();
    }

    private void OnDisable()
    {
        PlayerStats.ResearchPointsChanged -= HandleResearchPointsChanged;
    }

    public void ShowMenu()
    {
        if (PlayerStats.PSInstance == null)
        {
            Debug.LogWarning("Cannot open shop. PlayerStats is missing.");
            return;
        }

        previouslySelectedObject = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerUI != null)
            playerUI.SetActive(false);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");

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

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed && panelRoot != null && panelRoot.activeSelf)
            HideMenu();
    }

    public void OnShopItemButtonPressed(int shopItemTypeIndex)
    {
        if (!System.Enum.IsDefined(typeof(ShopItemType), shopItemTypeIndex))
            return;

        OnShopItemClicked((ShopItemType)shopItemTypeIndex);
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
        if (researchPointsLabel != null)
            researchPointsLabel.text = "Research Points: " + currentResearchPoints;
    }

    private void RefreshButtons()
    {
        if (shopUnlockButtons == null || shopUnlockButtons.Length == 0)
            return;

        PlayerStats stats = PlayerStats.PSInstance;
        SeedManager seedManager = SeedManager.SMInstance;
        CallDownEquipment equipment = CallDownEquipment.CDEInstance;

        if (stats == null)
            return;

        for (int i = 0; i < shopUnlockButtons.Length; i++)
        {
            ShopUnlockButton entry = shopUnlockButtons[i];
            if (entry == null || entry.button == null)
                continue;

            bool canAfford = stats.researchPoints >= entry.researchPointCost;
            bool canBuy = CanBuyShopItem(entry.itemType, seedManager, equipment,stats);
            entry.button.interactable = canAfford && canBuy;

            if (entry.label != null)
                entry.label.text = BuildShopLabel(entry, seedManager, equipment, stats);


            RefreshCostLabel(entry, i, canAfford, canBuy);
        }
    }


    private void RefreshCostLabel(ShopUnlockButton entry,int index, bool canAfford, bool canBuy)
    {
        TMP_Text targetCostLabel = GetCostLabel(entry, index);

        if (costLabels == null || index < 0 || index >= costLabels.Length)
            return;

        targetCostLabel.text = entry.researchPointCost + " RP";

        if (!canBuy)
        {
                targetCostLabel.color = Color.gray;
                return;
        }

        targetCostLabel.color = canAfford ? Color.white : Color.red;
    }
    private void InitializeCostLabels()
    {
        if (shopUnlockButtons == null || shopUnlockButtons.Length == 0)
            return;

        for (int i = 0; i < shopUnlockButtons.Length; i++)
        {
            ShopUnlockButton entry = shopUnlockButtons[i];
            if (entry == null)
                continue;

            TMP_Text targetCostLabel = GetCostLabel(entry, i);
            if (targetCostLabel == null)
                continue;

            targetCostLabel.text = entry.researchPointCost + " RP";
        }
    }

    private TMP_Text GetCostLabel(ShopUnlockButton entry, int index)
    {
        if (entry != null && entry.costLabel != null)
            return entry.costLabel;

        if (costLabels == null || index < 0 || index >= costLabels.Length)
            return null;

        return costLabels[index];
    }



    private bool CanBuyShopItem(ShopItemType itemType, SeedManager seedManager, CallDownEquipment equipment, PlayerStats stats)
    {
        switch (itemType)
        {
            case ShopItemType.GrenadeUnlock:
                return seedManager != null && !seedManager.GrenadeUnlocked;

            case ShopItemType.AutoSprinklerCharge:
            case ShopItemType.WaterRefillStationCharge:
                return equipment != null;

            case ShopItemType.PlayerWaterCapacityUpgrade:
                return stats != null;
            default:
                return false;
        }
    }

    private string BuildShopLabel(ShopUnlockButton entry, SeedManager seedManager, CallDownEquipment equipment, PlayerStats stats)
    {
        switch (entry.itemType)
        {
            case ShopItemType.GrenadeUnlock:
                if (seedManager != null && seedManager.GrenadeUnlocked)
                    return "Grenade Unlocked!";
                return "Unlock Grenade";

            case ShopItemType.AutoSprinklerCharge:
                int sprinklerCount = equipment != null ? equipment.SprinklerCharges : 0;
                return "Buy AutoSprinkler Charge\nCharges: " + sprinklerCount;

            case ShopItemType.WaterRefillStationCharge:
                int refillCount = equipment != null ? equipment.RefillStationCharges : 0;
                return "Buy Water Refill Charge\nCharges: " + refillCount;

            case ShopItemType.PlayerWaterCapacityUpgrade:
                float currentCapacity = stats != null ? stats.maxWaterCapacity : 0;
                return "Upgrade Water Capacity\nCurrent Capacity: " + currentCapacity;
            default:
                return "Unavailable";
        }
    }

    private void OnShopItemClicked(ShopItemType itemType)
    {
        PlayerStats stats = PlayerStats.PSInstance;
        SeedManager seedManager = SeedManager.SMInstance;
        CallDownEquipment equipment = CallDownEquipment.CDEInstance;

        if (stats == null)
            return;

        ShopUnlockButton config = FindShopItemConfig(itemType);
        if (config == null)
            return;

        if (!CanBuyShopItem(itemType, seedManager, equipment, stats))
        {
            AudioController.Play("Error");
            return;
        }
        if (!stats.TrySpendResearchPoints(config.researchPointCost))
            return;

        bool purchaseApplied = ApplyShopItem(itemType, seedManager, equipment, stats);
        if (!purchaseApplied)
        {
            stats.AddResearchPoints(config.researchPointCost);
            
            return;
        }

        AudioController.Play("Purchase");
        RefreshButtons();
        SelectDefaultButton();
    }

    private bool ApplyShopItem(ShopItemType itemType, SeedManager seedManager, CallDownEquipment equipment, PlayerStats stats)
    {
        switch (itemType)
        {
            case ShopItemType.GrenadeUnlock:
                if (SeedManager.SMInstance == null)
                    return false;

                seedManager.UnlockGrenade();
                return true;

            case ShopItemType.AutoSprinklerCharge:
                if (equipment == null)
                    return false;

                equipment.AddSprinklerCharge(1);
                ObjectivesTutorial.OTInstance.TryPlacerTutorial();
                return true;

            case ShopItemType.WaterRefillStationCharge:
                if (equipment == null)
                    return false;

                equipment.AddRefillStationCharge(1);
                ObjectivesTutorial.OTInstance.TryPlacerTutorial();
                return true;

                case ShopItemType.PlayerWaterCapacityUpgrade:
                    if (stats == null)
                        return false;

                    stats.IncreaseWaterCapacity(10); // Example value, adjust as needed
                    return true;    

            default:
                return false;


        }

    }
    private void WireShopButtons()
    {
        if (shopUnlockButtons == null)
            return;

        for (int i = 0; i < shopUnlockButtons.Length; i++)
        {
            ShopUnlockButton entry = shopUnlockButtons[i];
            if (entry == null || entry.button == null)
                continue;

            int capturedIndex = (int)entry.itemType;
            entry.button.onClick.RemoveAllListeners();
            entry.button.onClick.AddListener(() => OnShopItemButtonPressed(capturedIndex));
        }
    }
    private ShopUnlockButton FindShopItemConfig(ShopItemType itemType)
    {
        if (shopUnlockButtons == null)
            return null;

        for (int i = 0; i < shopUnlockButtons.Length; i++)
        {
            if (shopUnlockButtons[i] != null && shopUnlockButtons[i].itemType == itemType)
                return shopUnlockButtons[i];
        }

        return null;
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null)
            return;

        if (shopUnlockButtons != null)
        {
            for (int i = 0; i < shopUnlockButtons.Length; i++)
            {
                ShopUnlockButton entry = shopUnlockButtons[i];
                if (entry != null && entry.button != null && entry.button.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(entry.button.gameObject);
                    return;
                }
            }
        }

        if (closeButton != null)
            EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
    }

    private void RestorePreviousSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previouslySelectedObject);
    }
}