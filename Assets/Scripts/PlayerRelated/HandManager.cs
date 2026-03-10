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

        // Stop watering if the current hand is water
        if (hands[activeHandIndex].handType == HandType.Water)
        {
            waterHose.StopSpray();
        }

        activeHandIndex = (activeHandIndex + 1) % hands.Length;

        UpdateHands();
    }

    private void UpdateHands()
    {
        for (int i = 0; i < hands.Length; i++)
        {
            hands[i].handObject.SetActive(i == activeHandIndex);
        }
    }

    public HandType GetActiveHandType()
    {
        return hands[activeHandIndex].handType;
    }
}