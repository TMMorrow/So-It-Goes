using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public void PlayFlap()
    {
        GetComponent<AudioSource>().Play();
    }
}
