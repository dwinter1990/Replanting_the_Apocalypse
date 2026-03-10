using System.Collections.Generic;
using UnityEngine;

public class PlantGrowthManager : MonoBehaviour
{
    public static PlantGrowthManager Instance;

    private List<Growing> activePlants = new List<Growing>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(Growing plant)
    {
        if (!activePlants.Contains(plant))
            activePlants.Add(plant);
    }

    void Update()
    {
        float time = Time.time;

        for (int i = activePlants.Count - 1; i >= 0; i--)
        {
            Growing plant = activePlants[i];

            if (!plant.UpdateGrowth(time))
            {
                activePlants.RemoveAt(i);
            }
        }
    }
}