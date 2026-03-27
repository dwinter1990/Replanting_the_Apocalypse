using UnityEngine;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    [System.Serializable]
    public class RightHand
    {
        public GameObject handObject;
        public HandTypeRight handType;
    }
    [System.Serializable]
    public class LeftHand
    {
        public GameObject handObject;
        public HandTypeLeft handType;
    }

    [Header("Right hand")]
    [SerializeField] private RightHand[] rightHands;
    private int activeRightHandIndex = 0;

    // References to scripts for actions
    [SerializeField] private WaterHose waterHose;


    [Header("Left hand")]
    [SerializeField] private LeftHand[] leftHands;
    private int activeLeftHandIndex = 0;
    private void Start()
    {
        UpdateRightHands();
        UpdateLeftHands();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (!context.performed || rightHands.Length == 0) return;

        // Stop watering if the current hand is water
        if (rightHands[activeRightHandIndex].handType == HandTypeRight.Water)
        {
            waterHose.StopSpray();
        }

        activeRightHandIndex = (activeRightHandIndex + 1) % rightHands.Length;

        UpdateRightHands();
    }

    public void OnLeftNext(InputAction.CallbackContext context)
    {
        Debug.Log("Should be changing left hand now");
        if (!context.performed || leftHands.Length == 0)
        {
            return;
        }

        activeRightHandIndex = (activeLeftHandIndex + 1) % leftHands.Length;

        UpdateLeftHands();
    }
    private void UpdateRightHands()
    {
        for (int i = 0; i < rightHands.Length; i++)
        {
            rightHands[i].handObject.SetActive(i == activeRightHandIndex);
        }

    }
    private void UpdateLeftHands()
    {
        for (int i = 0; i < leftHands.Length; i++)
        {
            leftHands[i].handObject.SetActive(i == activeLeftHandIndex);
        }
        Debug.Log("Should be Updating left hand now: " + activeLeftHandIndex);
    }
    public HandTypeRight GetActiveHandRightType()
    {
        return rightHands[activeRightHandIndex].handType;
        
    }

    public HandTypeLeft GetActiveHandTypeLeft()
    {
        return leftHands[activeLeftHandIndex].handType;
    }
}