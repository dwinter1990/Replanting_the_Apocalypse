using UnityEngine;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    [System.Serializable]
    public class Hand
    {
        public GameObject handObject;
        public HandType handType;
    }

    [SerializeField] private Hand[] hands;
    private int activeHandIndex = 0;

    // References to scripts for actions
    [SerializeField] private WaterHose waterHose;

    private void Start()
    {
        UpdateHands();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (!context.performed || hands.Length == 0) return;

        // Deactivate current hand
        hands[activeHandIndex].handObject.SetActive(false);

        // Stop watering if it was active
        if (hands[activeHandIndex].handType == HandType.Water)
        {
            waterHose.StopSpray();
        }

        // Move to next hand
        activeHandIndex = (activeHandIndex + 1) % hands.Length;

        // Activate new hand
        UpdateHands();
    }

    private void UpdateHands()
    {
        Hand activeHand = hands[activeHandIndex];
        activeHand.handObject.SetActive(true);

        //if (activeHand.handType == HandType.Water)
        //{

        //}
    }

    public HandType GetActiveHandType()
    {
        return hands[activeHandIndex].handType;
    }
}