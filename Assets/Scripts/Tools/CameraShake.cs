using UnityEngine;
using System.Collections;
public class CameraShake : MonoBehaviour
{
    public static CameraShake CameraShakeInstance { get; private set; }
    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Vector3 originalPosition = transform.localPosition;
        float shakeDuration = 0.5f; // Duration of the shake
        float shakeMagnitude = 0.1f; // Magnitude of the shake
        float elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition; // Reset to original position
    }
}
