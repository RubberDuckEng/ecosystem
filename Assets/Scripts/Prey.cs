using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Prey : Animal
{
    void Start()
    {
        OnStart("Plant");
    }

    void Update()
    {
        OnUpdate();
    }
}
