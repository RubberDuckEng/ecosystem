using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text countText;

    EvolvableStats GatherStatsFrom(GameObject[] gos)
    {
        EvolvableStats stats = new EvolvableStats();
        foreach (var go in gos)
        {
            stats.Add(go.GetComponent<Animal>().traits);
        }
        return stats;
    }

    void Update()
    {
        // FIXME: There is probably a more efficient way to get these counts
        // or perhaps we should update this less frequently than once per frame.
        int plantCount = GameObject.FindGameObjectsWithTag("Plant").Length;

        EvolvableStats prey = GatherStatsFrom(GameObject.FindGameObjectsWithTag("Prey"));
        Evolvable avgPrey = prey.Average();

        EvolvableStats predators = GatherStatsFrom(GameObject.FindGameObjectsWithTag("Predator"));
        Evolvable avgPredator = predators.Average();

        countText.text = $"Predators: {predators.Length} ({avgPredator.ToDebugString()})";
        countText.text += $"\nPrey: {prey.Length} ({avgPrey.ToDebugString()})";
        countText.text += $"\nPlants: {plantCount}";
    }

}
