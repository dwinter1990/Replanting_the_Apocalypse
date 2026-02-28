using UnityEngine;

[CreateAssetMenu(fileName = "GrowthSO", menuName = "Scriptable Objects/GrowthSO")]
public class GrowthSO : ScriptableObject
{
    [Header("Growth")]
    public float growthSpeed = 0.5f;
    public float growthStep = 0.15f;
    public float maxScale = 2f;

    [Header("Step Bounce")]
    public float stepOvershoot = 0.15f;

    [Header("Final Bounce")]
    public float finalOvershoot = 0.2f;
    public float finalDuration = 0.5f;
    public float wobbleAmount = 6f;
}

