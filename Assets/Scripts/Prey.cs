using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Prey : Animal
{
    void Start()
    {
        AnimalConfig prey = new AnimalConfig();
        prey.foodTag = "Plant";
        prey.eatSoundName = "prey_eat";
        prey.fleeFromTag = "Predator";
        OnStart(prey);
    }

    void Update()
    {
        OnUpdate();
    }
}
