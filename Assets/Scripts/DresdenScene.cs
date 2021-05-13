using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DresdenScene : MonoBehaviour
{
    public Material dresdenSkybox1;
    public GameObject slaughterHouse;
    public AudioSource audioSource;
    public AudioClip airRaidSound;
    public LightController lightController;
    public GameObject sun;
    public GameObject triggerObj;
    public Movement playerMovement;
    public TimeTransform timeTransform;
    public AudioClip moonSounds;

    public void TriggerEvent()
    { 
        lightController.TriggerLightEvent();
        audioSource.PlayOneShot(airRaidSound);
        playerMovement.StartCoroutine(playerMovement.Shake(7f, .1f));
        timeTransform.DresdenFadeIn();
        StartCoroutine(WaitForRaid());
    }

    private IEnumerator WaitForRaid()
    {
        yield return new WaitForSecondsRealtime(7f);
        audioSource.PlayOneShot(moonSounds);
        
        yield return new WaitForSecondsRealtime(1f);
        SwitchToMoon();
        
        yield return new WaitForSecondsRealtime(1f);
        timeTransform.DresdenFadeOut();
        
        yield return new WaitForSecondsRealtime(10f);
        timeTransform.NextLevel();
    }

    private void SwitchToMoon()
    {
        audioSource.PlayOneShot(moonSounds);
        triggerObj.SetActive(false);
        sun.SetActive(true);
        slaughterHouse.SetActive(false);
        RenderSettings.skybox = dresdenSkybox1;
    }
    
}
