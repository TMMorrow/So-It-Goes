using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DresdenTrigger : MonoBehaviour
{
    public LightController lightController;
    public DresdenScene dresdenScene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dresdenScene.TriggerEvent();
        }
    }
}
