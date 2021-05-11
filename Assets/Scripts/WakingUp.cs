using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WakingUp : MonoBehaviour
{
    public Animator top;
    public Animator bottom;
    public Animator fadeIn;
    private static readonly int Level = Animator.StringToHash("NextLevel");
    
    
    private void Start()
    {
        top.SetBool(Level, true);
        top.Play("Blinking", 0, .85f);
        bottom.SetBool(Level, true);
        bottom.Play("Blinking", 0, .85f);
        fadeIn.SetBool(Level, true);
        fadeIn.Play("ChangeLevels", 0, .85f);
        StartCoroutine(Waking());
    }

    private IEnumerator Waking()
    {
        yield return new WaitForSecondsRealtime(1);
        top.SetBool(Level, false);
        bottom.SetBool(Level, false);
        fadeIn.SetBool(Level, false);
    }

}
