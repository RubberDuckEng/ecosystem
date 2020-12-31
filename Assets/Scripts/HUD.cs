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


        // Probably should be a separate Object?
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale *= 2f;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale *= .5f;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Smite();
        }

    }

    void Smite()
    {
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("Prey"))
        {
            if (Random.value > 0.5)
            {
                gameObject.GetComponent<Edible>().Eat();
            }
        }
    }
}
