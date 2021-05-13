using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalScene : MonoBehaviour
{
    public void Finale()
    {
        transform.parent.GetComponent<TimeTransform>().NextLevel();
    }
}
