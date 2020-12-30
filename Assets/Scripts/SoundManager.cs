using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip preyEat;
    public static AudioSource audioSource;

    void Start()
    {
        preyEat = Resources.Load<AudioClip>("third_party/freesound/carrot");
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string name)
    {
        switch (name)
        {
            case "eat":
                audioSource.PlayOneShot(preyEat);
                break;
        }
    }

}
