using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSwap : MonoBehaviour
{
    public AudioSource audioSourceFadeOut;
    public AudioSource audioSourceFadeIn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeOutAndIn());
        } 
    }

    private IEnumerator FadeOutAndIn()
    {
        while (audioSourceFadeOut.volume > 0)
        {
            audioSourceFadeOut.volume -= 0.01f;
            audioSourceFadeIn.volume += 0.01f;
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
}
