using UnityEngine;

[CreateAssetMenu(menuName = "Plants/Growth Profile")]
public class GrowthSO : ScriptableObject
{
    [Header("Growth")]
    public float minGrowthSpeed = 0.8f;
    public float maxGrowthSpeed = 1.2f;
    public float growthSteps = 0.15f;
    public float waterMemory = 0.4f;
    public float growthDuration = 10f;

    [Header("Scale")]
    public Vector3 startScale = Vector3.one;
    public float minScale = 0.75f;
    public float maxScale = 1.5f;
    public float maxScaleMultiplier = 2f;

    [Header("Animation")]
    public float stepOvershoot = 0.15f;
    public float finalOvershoot = 0.3f;
    public float finalDuration = 0.6f;
    public float wobbleAmount = 10f;
}