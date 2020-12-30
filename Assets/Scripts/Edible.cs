using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edible : MonoBehaviour
{
    public event System.Action OnDeath;
    public float nutrition = 0.5f;

    public float Eat()
    {
        if (OnDeath != null)
        {
            OnDeath();
        }
        Destroy(gameObject);
        return nutrition;
    }
}
