using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip preyEat, predatorEat;
    public static AudioSource audioSource;

    void Start()
    {
        preyEat = Resources.Load<AudioClip>("third_party/freesound/carrot");
        predatorEat = Resources.Load<AudioClip>("third_party/freesound/meat");
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string name)
    {
        if (!User.playSounds) return;
        switch (name)
        {
            case "prey_eat":
                audioSource.PlayOneShot(preyEat);
                break;
            case "predator_eat":
                audioSource.PlayOneShot(predatorEat);
                break;

        }
    }

}
