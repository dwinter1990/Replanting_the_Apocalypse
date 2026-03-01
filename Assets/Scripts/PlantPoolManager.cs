using UnityEngine;


public class PlantPoolManager : MonoBehaviour
{
    public static PlantPoolManager PlantPoolManagerInstance;

    public PlantPool[] plantPools;

    public PlantType selectedType = PlantType.Grass;
    private void Awake()
    {
        PlantPoolManagerInstance = this;
    }

    public GameObject GetRandomPlant(PlantType type)
    {
        var filteredPools = System.Array.FindAll(plantPools, p => p.plantType == type);
        if(filteredPools.Length == 0)
        {
            Debug.LogWarning("No pools of type " + type);
            return null;
        }
        int index = Random.Range(0, filteredPools.Length);
        return filteredPools[index].Get();
    }

    public void ChangeSelectedType(int delta)
    {
        int typeCount = System.Enum.GetNames(typeof(PlantType)).Length;
        int newType =((int)selectedType + delta + typeCount) % typeCount;
        selectedType = (PlantType)newType;
        Debug.Log("Selected plant type is: " + selectedType);
    }
}
