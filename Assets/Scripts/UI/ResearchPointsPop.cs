using DG.Tweening;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ResearchPointsPop : MonoBehaviour
{
    public static ResearchPointsPop RPPInstance { get; private set; }

    [SerializeField] private TextMeshProUGUI tMPText;
    private Sequence RPPopTween = DOTween.Sequence();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (RPPInstance != null && RPPInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        RPPInstance = this;
        DontDestroyOnLoad(this.gameObject);

        
    }

    private void Start()
    {
        CreateTween();
    
    }
    private void CreateTween()
    {
        RPPopTween.Append(tMPText.DOFade(1f, 0.1f))
            .Join(transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack))
            .Append(transform.DOScale(1f, 0.3f).SetEase(Ease.InBack))
            .Join(tMPText.DOFade(0f, 0.1f))
            .SetAutoKill(false)
            .Pause();
    }

    public void PlayPopAnimation(int points)
    {
        tMPText.text = $"+{points}";
        RPPopTween.Restart();
    }
}
