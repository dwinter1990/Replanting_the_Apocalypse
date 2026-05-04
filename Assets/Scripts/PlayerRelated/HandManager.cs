using UnityEngine;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    public static HandManager HMInstance { get; set; }
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
    public bool canSwapRight = false;
    private bool CanSwapRight => canSwapRight; 
    private bool firstTimeSwapRight = true;
    // References to scripts for actions
    [SerializeField] private WaterHose waterHose;


    [Header("Left hand")]
    [SerializeField] private LeftHand[] leftHands;
    private int activeLeftHandIndex = 0;
    public bool canSwapLeft = false;
    private bool CanSwapLeft => canSwapLeft;
    private void Awake()
    {
        if (HMInstance != null && HMInstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            HMInstance = this;
        }
    }
    private void Start()
    {
        UpdateRightHands();
        UpdateLeftHands();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (!CanSwapRight)
        {
            return;
        }
        if (!context.performed || rightHands.Length == 0) return;

        if (rightHands[activeRightHandIndex].handType == HandTypeRight.Water)
        {
            waterHose.StopSpray();
        }

        activeRightHandIndex = (activeRightHandIndex + 1) % rightHands.Length;

        UpdateRightHands();
    }

    public void OnLeftNext(InputAction.CallbackContext context)
    {
        if(!CanSwapLeft)
        {
            return;
        }
        Debug.Log("Should be changing left hand now");
        if (!context.performed || leftHands.Length == 0)
        {
            return;
        }

        activeLeftHandIndex = (activeLeftHandIndex + 1) % leftHands.Length;

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

    public HandTypeLeft GetActiveHandLeftType()
    {
        return leftHands[activeLeftHandIndex].handType;
    }
}