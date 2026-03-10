using UnityEngine;
using DG.Tweening;

public class DOTweenBootstrap : MonoBehaviour
{
    void Awake()
    {
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);

        // Pre-allocate tween capacity to avoid runtime resizing
        DOTween.SetTweensCapacity(2000, 200);

        // Enable tween recycling
        DOTween.defaultRecyclable = true;

        // Optional: faster updates when using many tweens
        DOTween.useSafeMode = false;
    }
}