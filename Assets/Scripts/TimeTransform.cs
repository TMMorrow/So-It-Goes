using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTransform : MonoBehaviour
{
    public List<Transform> locationObject;
    public List<float> locationTime;
    public int currentLevel;

    private void Start()
    {
        currentLevel = 0;
        StartCoroutine(ChangePeriod(locationTime[0]));
    }

    private IEnumerator ChangePeriod(float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        NextLevel();
        StartCoroutine(ChangePeriod(locationTime[currentLevel]));
    }

    private void NextLevel()
    {
        GetComponent<CharacterController>().enabled = false;
        StartCoroutine(ChangeAnimation());
        currentLevel++;
        if (currentLevel > locationObject.Count - 1)
        {
            currentLevel = 0;
        }
        transform.position = locationObject[currentLevel].position;
    }

    private IEnumerator ChangeAnimation()
    {
        yield return new WaitForSecondsRealtime(8);
        GetComponent<CharacterController>().enabled = true;
    }
    
}
