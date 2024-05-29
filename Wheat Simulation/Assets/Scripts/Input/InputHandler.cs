using System.Collections;
using System;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // Scripts

    // Mouse Over Wheat
    [SerializeField] private GameObject MouseOverWheatGameObject;
    private MouseOverWheatHandler mouseOverWheatHandler;

    // Scan Wheat
    [SerializeField] private GameObject WheatScanGameObject;
    private ScanScene scanScene;

    // Annotate Camera
    [SerializeField] private GameObject AnnotateCameraGameObject;
    private AnnotateCamera annotateCamera;

    // Place Wheat at Cursor
    [SerializeField] private GameObject placeWheatGameObject;
    private PlaceWheat placeWheat;

    // Screenshot script
    [SerializeField] private GameObject screenShotGameObject;
    private ScreenShot screenShot;

    // Map KeyCodes to Actions
    Dictionary<KeyCode, Action> keyMap = new Dictionary<KeyCode, Action>();
    
    void Start(){
        //Script references
        mouseOverWheatHandler = MouseOverWheatGameObject.GetComponent<MouseOverWheatHandler>();
        scanScene = WheatScanGameObject.GetComponent<ScanScene>();
        annotateCamera = AnnotateCameraGameObject.GetComponent<AnnotateCamera>();
        placeWheat = placeWheatGameObject.GetComponent<PlaceWheat>();
        screenShot = screenShotGameObject.GetComponent<ScreenShot>();

        // Maps keybinds to functions in other scripts
        keyMap[KeyCode.F] = DetectWheatPart;
        keyMap[KeyCode.E] = ScanWheat;
        keyMap[KeyCode.R] = Annotate;
        keyMap[KeyCode.Q] = PlaceWheatAtCursor;
        keyMap[KeyCode.T] = TakeScreenShot;
    }

    // Update is called once per frame
    void Update()
    {
        // Checks each KeyCode in the keyMap dictionary. If that key is pressed down, activate the corresponding function
        foreach (KeyCode keyCode in keyMap.Keys){
            if (Input.GetKeyDown(keyCode)){
                keyMap[keyCode]();
            }
        }
    }

    // Print the details of the wheat part that is being moused over
    private void DetectWheatPart(){
        mouseOverWheatHandler.DetectWheatPart();
    }
    
    // Print the appr. number of each part that are on screen, and as a percent of total
    private void ScanWheat(){
        scanScene.ScanWheat();
    }

    // Toggle the view to annotate mode
    private void Annotate(){
        Wheat.ToggleAnnotation();
        annotateCamera.SwapCameras();
    }

    private void PlaceWheatAtCursor(){
        placeWheat.PlaceWheatAtCursor();
    }

    // Take a screen shot
    private void TakeScreenShot(){
        screenShot.TakeScreenShot();
    }
}
