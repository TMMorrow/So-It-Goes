using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public List<Light> lightsToFlicker;
    public List<Light> lightsToDisable;
    public List<Light> lightsToChangeColor;
    public float minIntensity = 0f;

    [Tooltip("Maximum random light intensity")]
    public float maxIntensity = 1f;

    [Tooltip("How much to smooth out the randomness; lower values = sparks, higher = lantern")] [Range(1, 50)]
    public int smoothing = 5;

    Queue<float> _smoothQueue;
    private bool _flickering;
    float _lastSum = 0;

    public void TriggerLightEvent()
    {
        StartCoroutine(FadeToEvent());
    }

    public void Reset()
    {
        _smoothQueue.Clear();
        _lastSum = 0;
    }

    private void Start()
    {
        _smoothQueue = new Queue<float>(smoothing);
    }

    private IEnumerator FadeToEvent()
    {
        List<Light> allLights = new List<Light>();
        allLights.AddRange(lightsToDisable);
        allLights.AddRange(lightsToFlicker);
        allLights.AddRange(lightsToChangeColor);
        
        
        foreach (var l in allLights)
        {
            l.gameObject.SetActive(false);
        }
        yield return new WaitForSecondsRealtime(0.5f);

        foreach (var l in allLights)
        {
            l.gameObject.SetActive(true);
        }

        foreach (var l in lightsToChangeColor)
        {
            l.color = Color.red;
        }

        foreach (var l in lightsToDisable)
        {
            l.gameObject.SetActive(false);
        }

        _flickering = true;
        Reset();
    }

    private void Update()
    {
        if (!_flickering) return;

        foreach (var l in lightsToFlicker)
        {
            // pop off an item if too big
            while (_smoothQueue.Count >= smoothing)
            {
                _lastSum -= _smoothQueue.Dequeue();
            }

            // Generate random new item, calculate new average
            float newVal = Random.Range(minIntensity, maxIntensity);
            _smoothQueue.Enqueue(newVal);
            _lastSum += newVal;

            // Calculate new smoothed average
            l.intensity = _lastSum / _smoothQueue.Count;
        }
    }
}