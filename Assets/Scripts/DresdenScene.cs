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

    public void TriggerEvent()
    { 
        lightController.TriggerLightEvent();
        audioSource.PlayOneShot(airRaidSound);
        StartCoroutine(WaitForRaid());
    }

    private IEnumerator WaitForRaid()
    {
        yield return new WaitForSecondsRealtime(7f);
        SwitchToMoon();
    }

    private void SwitchToMoon()
    {
        triggerObj.SetActive(false);
        sun.SetActive(true);
        slaughterHouse.SetActive(false);
        RenderSettings.skybox = dresdenSkybox1;
    }
    
}
