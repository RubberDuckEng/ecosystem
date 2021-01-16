using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keyboard : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale *= 2f;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale *= .5f;
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            // Maximum value during editing.
            Time.timeScale = 100f;
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Time.timeScale = 1f;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            User.playSounds = !User.playSounds;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            User.showSightRings = !User.showSightRings;
            UpdateSightRings();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Smite();
        }
    }

    void UpdateSightRings()
    {
        foreach (Animal animal in GameObject.FindObjectsOfType<Animal>())
        {
            animal.ShowOrHideSightRadius();
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
