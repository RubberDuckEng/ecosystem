using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text countText;

    void Update()
    {
        // FIXME: There is probably a more efficient way to get these counts
        // or perhaps we should update this less frequently than once per frame.
        int preyCount = GameObject.FindGameObjectsWithTag("Prey").Length;
        int plantCount = GameObject.FindGameObjectsWithTag("Plant").Length;
        int predatorCount = GameObject.FindGameObjectsWithTag("Predator").Length;

        countText.text = $"Predators: {predatorCount}\nPrey: {preyCount}\nPlants: {plantCount}";
    }
}
