using CS.AudioToolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
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
    [SerializeField] private RawImage[] rightHandImages;
    private int activeRightHandIndex = 0;
    public bool canSwapRight = false;
    private bool CanSwapRight => canSwapRight; 
    private bool firstTimeSwapRight = true;
    // References to scripts for actions
    [SerializeField] private WaterHose waterHose;


    [Header("Left hand")]
    [SerializeField] private LeftHand[] leftHands;
    [SerializeField] private RawImage[] leftHandImages;
    private int activeLeftHandIndex = 0;
    public bool canSwapLeft = false;
    private bool CanSwapLeft => canSwapLeft;

    [Header("Tweening")]
    [SerializeField] private float tweenDuration = 0.5f;
    [SerializeField] private float activeIconScale = 1f;
    [SerializeField] private float inactiveIconScale = 0.75f;
    [SerializeField] private Ease iconEase = Ease.OutBack;
    private Sequence[] rightIconToActiveSequences;
    private Sequence[] rightIconToInactiveSequences;

    private Sequence[] leftIconToActiveSequences;
    private Sequence[] leftIconToInactiveSequences;

    private Vector2 rightActiveSlot;
    private Vector2 rightInactiveSlot;

    private Vector2 leftActiveSlot;
    private Vector2 leftInactiveSlot;
    [SerializeField] private float activeIconAlpha = 1f;
    [SerializeField] private float inactiveIconAlpha = 0.5f;
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
        CreateTweens();
        UpdateRightHands();
        UpdateLeftHands();

        ApplyIconStateImmediately(rightHandImages, activeRightHandIndex);
        ApplyIconStateImmediately(leftHandImages, activeLeftHandIndex);
    }

    private void CreateTweens()
    {
        // Assumes icon 0 starts active/front, icon 1 starts inactive/back.
        rightActiveSlot = rightHandImages[0].rectTransform.anchoredPosition;
        rightInactiveSlot = rightHandImages[1].rectTransform.anchoredPosition;

        leftActiveSlot = leftHandImages[0].rectTransform.anchoredPosition;
        leftInactiveSlot = leftHandImages[1].rectTransform.anchoredPosition;

        rightIconToActiveSequences = CreateMoveSequences(
            rightHandImages,
            rightActiveSlot,
            activeIconScale,
            activeIconAlpha);

        rightIconToInactiveSequences = CreateMoveSequences(
            rightHandImages,
            rightInactiveSlot,
            inactiveIconScale,
            inactiveIconAlpha);

        leftIconToActiveSequences = CreateMoveSequences(
            leftHandImages,
            leftActiveSlot,
            activeIconScale,
            activeIconAlpha);

        leftIconToInactiveSequences = CreateMoveSequences(
            leftHandImages,
            leftInactiveSlot,
            inactiveIconScale,
            inactiveIconAlpha);
    }
    private void ApplyIconStateImmediately(RawImage[] handImages, int activeIndex)
    {
        for (int i = 0; i < handImages.Length; i++)
        {
            if (handImages[i] == null)
            {
                continue;
            }

            Color color = handImages[i].color;
            color.a = i == activeIndex ? activeIconAlpha : inactiveIconAlpha;
            handImages[i].color = color;
        }
    }

    private Sequence[] CreateMoveSequences(
        RawImage[] handImages,
        Vector2 targetPosition,
        float targetScale,
        float targetAlpha)
    {
        Sequence[] sequences = new Sequence[handImages.Length];

        for (int i = 0; i < handImages.Length; i++)
        {
            if (handImages[i] == null)
            {
                continue;
            }

            RectTransform iconTransform = handImages[i].rectTransform;

            sequences[i] = DOTween.Sequence()
                .Append(iconTransform.DOAnchorPos(targetPosition, tweenDuration).SetEase(iconEase))
                .Join(iconTransform.DOScale(targetScale, tweenDuration).SetEase(iconEase))
                .Join(handImages[i].DOFade(targetAlpha, tweenDuration).SetEase(iconEase))
                .SetAutoKill(false)
                .Pause();
        }

        return sequences;
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
            if(rightHands[i].handType == HandTypeRight.Harvest && i == activeRightHandIndex)
            {
                AudioController.Play("ChainsawIdle");
            }
        }

        TweenHandIcons(
            rightHandImages,
            rightIconToActiveSequences,
            rightIconToInactiveSequences,
            activeRightHandIndex);
    }

    private void UpdateLeftHands()
    {
        for (int i = 0; i < leftHands.Length; i++)
        {
            leftHands[i].handObject.SetActive(i == activeLeftHandIndex);
        }

        TweenHandIcons(
            leftHandImages,
            leftIconToActiveSequences,
            leftIconToInactiveSequences,
            activeLeftHandIndex);

        Debug.Log("Should be Updating left hand now: " + activeLeftHandIndex);
    }

    private void TweenHandIcons(
     RawImage[] handImages,
     Sequence[] toActiveSequences,
     Sequence[] toInactiveSequences,
     int activeIndex)
    {
        for (int i = 0; i < handImages.Length; i++)
        {
            if (handImages[i] == null)
            {
                continue;
            }

            handImages[i].rectTransform.DOKill();

            if (i == activeIndex)
            {
                handImages[i].rectTransform.SetAsLastSibling();
                toActiveSequences[i].Restart();
            }
            else
            {
                
                toInactiveSequences[i].Restart();
            }
        }
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
