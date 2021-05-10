using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeirdSkyTrickery : MonoBehaviour
{
    public Transform playerTransform;
    private Vector3 _defaultOffset;

    private void Start()
    {
        _defaultOffset = transform.position - playerTransform.position;
    }

    private void FixedUpdate()
    {
        Vector3 tempVec = playerTransform.position + _defaultOffset;
        tempVec = new Vector3(0, 0, Mathf.Clamp(tempVec.z, 0.5f, 8f));

        transform.position = tempVec;
    }
}
