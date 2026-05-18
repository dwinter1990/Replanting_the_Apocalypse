using PaintIn3D;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Triggers Paint in 3D-compatible hit events when the plant reaches half growth.
///
/// This follows the CwHitCollisions/CwPaintDecal style flow by emitting a hit point
/// (preview, priority, pressure, seed, position, rotation) to paint receivers.
/// </summary>
public class GroundPaintOnGrowth : MonoBehaviour
{
    [Header("Paint Position")]
    [SerializeField] private Transform paintOrigin;
    [SerializeField] private Vector3 paintOffset = Vector3.zero;
    [SerializeField] private Vector3 paintNormal = Vector3.up;

    [Header("PaintIn3D Hit Payload")]
    [SerializeField] private bool preview;
    [SerializeField] private int priority;
    [SerializeField] private float pressure = 1f;
    [SerializeField] private int seed;

    [Header("PaintIn3D Routing")]
    [Tooltip("Optional override for where hit events should be routed. If null, this GameObject is used.")]
    [SerializeField] private GameObject rootOverride;

    [Header("Optional Generic Fallback")]
    [SerializeField] private Component fallbackTarget;
    [SerializeField] private string fallbackMethodName = "PaintAtWorldPosition";

    public void TriggerPaint()
    {
        var finalPosition = (paintOrigin != null ? paintOrigin.position : transform.position) + paintOffset;
        var finalRotation = Quaternion.LookRotation(-paintNormal.normalized, Vector3.up);

        //if (TryInvokeHitCachePoint(finalPosition, finalRotation))
        //{
        //    return;
        //}

        if (TryInvokePaintReceiverPoint(finalPosition, finalRotation))
        {
            return;
        }

        TryTriggerFallback(finalPosition);
        
    }

    private bool TryInvokePaintReceiverPoint(Vector3 position, Quaternion rotation)
    {
        var root = rootOverride != null ? rootOverride : gameObject;
        var receivers = root.GetComponentsInChildren<MonoBehaviour>(true);
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var hit = false;

        foreach (var receiver in receivers)
        {
            if (receiver == null)
            {
                continue;
            }

            var type = receiver.GetType();
            var method = type.GetMethod(
                "HandleHitPoint",
                flags,
                null,
                new[] { typeof(bool), typeof(int), typeof(float), typeof(int), typeof(Vector3), typeof(Quaternion) },
                null);

            if (method == null)
            {
                continue;
            }

            method.Invoke(receiver, new object[] { preview, priority, pressure, seed, position, rotation });
            hit = true;
        }

        return hit;
    }

    private void TryTriggerFallback(Vector3 paintPosition)
    {
        if (fallbackTarget == null)
        {
            Debug.LogWarning($"{nameof(GroundPaintOnGrowth)} on {name}: no PaintIn3D receiver found and no fallback target configured.");
            return;
        }

        fallbackTarget.SendMessage(fallbackMethodName, paintPosition, SendMessageOptions.DontRequireReceiver);
    }
}