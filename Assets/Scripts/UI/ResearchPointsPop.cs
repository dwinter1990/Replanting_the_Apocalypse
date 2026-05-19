using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ResearchPointsPop : MonoBehaviour
{
    public static ResearchPointsPop RPPInstance { get; private set; }

    [SerializeField] private TextMeshProUGUI tMPText;
    [SerializeField] private RectTransform endPos;
    [SerializeField] private TextMeshProUGUI playerResearchPointsText;

    private Sequence RPPopTween;
    void Awake()
    {
        RPPInstance = this;
    }

    private void Start()
    {
                CreateTween();
    }
    private void CreateTween()
    {
        RPPopTween?.Kill();
        RPPopTween = DOTween.Sequence();

        RPPopTween
            .Append(tMPText.DOFade(1f, 0.1f))
            .Join(tMPText.rectTransform.DOScale(1.5f, 0.5f).SetEase(Ease.OutCubic))
            .Join(tMPText.rectTransform.DOMove(endPos.position, 0.75f).SetEase(Ease.OutCubic))
            .Append(tMPText.rectTransform.DOScale(0.1f, 0.15f).SetEase(Ease.InCubic))
            .Append(tMPText.DOFade(0f, 0.1f))
            .Join(playerResearchPointsText.rectTransform.DOPunchScale(Vector3.one * 1.25f, 0.25f, 2, .75f).SetEase(Ease.OutCubic))
            .OnComplete(() => playerResearchPointsText.SetText($"{PlayerStats.PSInstance.researchPoints}")) 

            .SetAutoKill(false)
            .Pause();
    }

    public void PlayPopAnimation(int points)
    {
        if (RPPopTween == null)
            CreateTween();

        tMPText.text = $"+{points}";

        RPPopTween.Restart();
    }
}
