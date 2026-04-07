using UnityEngine;
using System.Collections.Generic;

[System.Serializable] 

public enum PlantType
{
    Grass,
    Flower,
    Bush,
    Tree
}


public class PlantPool : MonoBehaviour
{
    public PlantType plantType;

    [SerializeField] private GameObject prefab;
    [SerializeField] private int preloadAmount = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    [SerializeField] private Transform firstPlacementLocation;
    private bool worldPlant = true;
    public bool IsWorldPlant => worldPlant;
    private void Awake()
    {
        for (int i = 0; i < preloadAmount; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        if (firstPlacementLocation == null)
        {
            firstPlacementLocation = GetComponentInChildren<Transform>();
        }

        if (worldPlant)
        {
            Get();
        }
    }

    public GameObject Get()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
        }

        obj.SetActive(true);

        // Tell the plant which pool it came from
        Growing growing = obj.GetComponent<Growing>();
        if (growing != null)
        {
            growing.SetPool(this);
        }

        if (worldPlant)
        {
            obj.transform.position = firstPlacementLocation.position;
            
            worldPlant = false;
        }
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}