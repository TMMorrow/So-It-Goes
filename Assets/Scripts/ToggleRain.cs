using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.RainMaker;
using UnityEngine;

public class ToggleRain : MonoBehaviour
{
    public GameObject rainPrefab;

    private void OnTriggerEnter(Collider player)
    {
        if (rainPrefab.GetComponent<RainScript>().RainFallParticleSystem.isPlaying)
        {
            rainPrefab.GetComponent<RainScript>().RainFallParticleSystem.Stop();
        }
        else if(rainPrefab.GetComponent<RainScript>().RainFallParticleSystem.isPaused || rainPrefab.GetComponent<RainScript>().RainFallParticleSystem.isStopped)
        {
            rainPrefab.GetComponent<RainScript>().RainIntensity = .4f;
            rainPrefab.GetComponent<RainScript>().RainFallParticleSystem.Play();
        }
    }
    
}
