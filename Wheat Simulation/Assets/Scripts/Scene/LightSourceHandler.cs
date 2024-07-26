using System.Collections.Generic;
using UnityEngine;

public class LightSourceHandler : MonoBehaviour {
    [SerializeField] private GameObject defaultSun;

    [SerializeField] private List<GameObject> lightSources = new List<GameObject>();
    private GameObject activeLightSource;

    public void SwapLightSource(){
        activeLightSource = lightSources[Random.Range(0, lightSources.Count - 1)];

        // Set the active light source to the new light source.
        activeLightSource.SetActive(true);

        // Disable all other light sources.
        DisableAllOtherLightSources(activeLightSource);
    }

    public void ResetLightSource(){
        activeLightSource = defaultSun;
        DisableAllOtherLightSources(activeLightSource);
    }

    private void DisableAllOtherLightSources(GameObject exception){
        // Disable default sun
        if (exception != defaultSun){
            defaultSun.SetActive(false);
        }

        // Disable everything else
        foreach (GameObject lightSource in lightSources){
            if (lightSource != exception){
                lightSource.SetActive(false);
            }
        }
    }
}