using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaycastPlayer : MonoBehaviour
{
    public int drawDistance;
    public GameObject text;
    public List<String> photoInfo = new List<string>();
    public List<String> photoTitle = new List<string>();
    public GameObject infoCanvas;
    public GameObject camCanvas;
    public GameObject bradyCanvas;
    private GameObject player;
    private Camera mainCam;

    private void Start()
    {
        mainCam = gameObject.GetComponent<Camera>();
    }

    private void Interact(GameObject x)
    {
        //Debug.Log(photoData[int.Parse(x.name)]);
        text.SetActive(true);
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (x.CompareTag("Camera"))
        {
            camCanvas.SetActive(true);
        }
        else if(x.CompareTag("Brady"))
        {
            bradyCanvas.SetActive(true);
        }
        else
        {
            infoCanvas.SetActive(true);
            infoCanvas.transform.GetChild(1).GetComponent<Text>().text = photoTitle[int.Parse(x.transform.parent.name)];
            infoCanvas.transform.GetChild(2).GetComponent<Text>().text = photoInfo[int.Parse(x.transform.parent.name)];
        }
    }
    
    void FixedUpdate()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, drawDistance, 9) && (hit.collider.gameObject.CompareTag("Photo") || hit.collider.gameObject.CompareTag("Camera") || hit.collider.gameObject.CompareTag("Brady")))
        {
            Interact(hit.collider.gameObject);
        }
        else
        {
            text.SetActive(false);
            infoCanvas.SetActive(false);
            camCanvas.SetActive(false);
            bradyCanvas.SetActive(false);
        }
    }
}
