using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : Animal
{
    void Start()
    {
        AnimalConfig predator = new AnimalConfig();
        predator.foodTag = "Prey";
        predator.eatSoundName = "predator_eat";
        OnStart(predator);
    }

    void Update()
    {
        OnUpdate();
    }
}
