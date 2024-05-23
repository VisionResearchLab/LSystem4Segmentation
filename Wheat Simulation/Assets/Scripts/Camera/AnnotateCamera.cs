using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnnotateCamera : MonoBehaviour
{
    Camera mainCamera;
    Camera annotateCamera;

    private void Start(){
        mainCamera = Camera.main;
        annotateCamera = gameObject.GetComponent<Camera>();
        annotateCamera.enabled = false;
    }

    // When R is pressed, toggle wheat annotation
    void Update(){
        if (Input.GetKeyDown(KeyCode.R)){
            Wheat.ToggleAnnotation();
            SwapCameras();
        }
    }

    void SwapCameras(){
        // Start annotation
        if (Wheat.wheatIsAnnotated){
            mainCamera.enabled = false;
            annotateCamera.enabled = true;
        }
        // End annotation
        else {
            mainCamera.enabled = true;
            annotateCamera.enabled = false;
        }
    }
}
