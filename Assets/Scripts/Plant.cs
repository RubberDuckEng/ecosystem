using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public event System.Action OnDeath;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Eat() {
        if (OnDeath != null) {
            OnDeath();
        }
        Destroy(gameObject);
    }
}
