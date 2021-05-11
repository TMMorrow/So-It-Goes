using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeTransform : MonoBehaviour
{

    public Animator top;
    public Animator bottom;
    public Animator fadeIn;
    private static readonly int Level = Animator.StringToHash("NextLevel");

    private void OnTriggerEnter(Collider other)
    {
        top.SetBool(Level, true);
        bottom.SetBool(Level, true);
        fadeIn.SetBool(Level, true);
        StartCoroutine(ChangeAnimation());
    }

    private IEnumerator ChangeAnimation()
    {
        yield return new WaitForSecondsRealtime(7);
        if (SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
        top.SetBool(Level, false);
        bottom.SetBool(Level, false);
        fadeIn.SetBool(Level, false);
    }
    
}
