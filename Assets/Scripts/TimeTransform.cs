using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTransform : MonoBehaviour
{
    public List<Transform> locationObject;
    public List<float> locationTime;
    public int currentLevel;

    public Animator top;
    public Animator bottom;
    public Animator fadeIn;
    private static readonly int Level = Animator.StringToHash("NextLevel");

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
        top.SetBool(Level, true);
        bottom.SetBool(Level, true);
        fadeIn.SetBool(Level, true);
        StartCoroutine(ChangeAnimation());
        currentLevel++;
        if (currentLevel > locationObject.Count - 1)
        {
            currentLevel = 0;
        }
    }

    private IEnumerator ChangeAnimation()
    {
        yield return new WaitForSecondsRealtime(7);
        transform.position = locationObject[currentLevel].position;
        GetComponent<CharacterController>().enabled = true;
        top.SetBool(Level, false);
        bottom.SetBool(Level, false);
        fadeIn.SetBool(Level, false);
    }
    
}
