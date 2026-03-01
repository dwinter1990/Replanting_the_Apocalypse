using UnityEngine;

public class PlantPoolManager : MonoBehaviour
{
    public static PlantPoolManager PlantPoolManagerInstance;

    public PlantPool[] plantPools;

    private void Awake()
    {
        PlantPoolManagerInstance = this;
    }
    public GameObject GetRandomPlant()
    {
        int index = Random.Range(0, plantPools.Length);
        return plantPools[index].Get();
    }
}
