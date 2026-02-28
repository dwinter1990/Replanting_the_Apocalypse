using UnityEngine;
using DG.Tweening;

public class TweenQueue : MonoBehaviour
{
    public void KillChannel(string id)
    {
        DOTween.Kill(GetId(id));
    }

    public Sequence CreateSequence(string id)
    {
        KillChannel(id);
        return DOTween.Sequence().SetId(GetId(id));
    }

    public Tween CreateTween(Tween tween, string id)
    {
        KillChannel(id);
        return tween.SetId(GetId(id));
    }

    private string GetId(string id)
    {
        return gameObject.GetInstanceID() + "_" + id;
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject.GetInstanceID());
    }
}