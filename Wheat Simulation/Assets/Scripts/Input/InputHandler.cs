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

    // Place Wheat at Cursor
    [SerializeField] private GameObject placeWheatGameObject;
    private PlaceWheat placeWheat;

    // Screenshot script
    [SerializeField] private GameObject screenShotGameObject;
    private ScreenShot screenShot;
    
    // Auto orbit scanning script
    [SerializeField] private GameObject autoOrbitGameObject;
    private AutoOrbitScan autoOrbitScan;

    // Place underbrush objects script
    [SerializeField] private GameObject massAddObjectsGameObject;
    private MassAddObjects massAddObjects;

    // Map KeyCodes to Actions
    Dictionary<KeyCode, Action> keyMap = new Dictionary<KeyCode, Action>();
    
    void Start(){
        //Script references
        mouseOverWheatHandler = MouseOverWheatGameObject.GetComponent<MouseOverWheatHandler>();
        scanScene = WheatScanGameObject.GetComponent<ScanScene>();
        placeWheat = placeWheatGameObject.GetComponent<PlaceWheat>();
        screenShot = screenShotGameObject.GetComponent<ScreenShot>();
        autoOrbitScan = autoOrbitGameObject.GetComponent<AutoOrbitScan>();
        massAddObjects = massAddObjectsGameObject.GetComponent<MassAddObjects>();

        // Maps keybinds to functions in other scripts
        // keyMap[KeyCode.F] = DetectWheatPart;
        // keyMap[KeyCode.E] = ScanWheat;
        keyMap[KeyCode.Q] = PlaceWheatAtCursor;
        keyMap[KeyCode.R] = LoopAddObjects;
        keyMap[KeyCode.T] = TakeScreenShot;
        keyMap[KeyCode.Y] = BeginOrbiting;
        keyMap[KeyCode.U] = EndOrbiting;
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

    private void PlaceWheatAtCursor(){
        placeWheat.PlaceWheatAtCursor();
    }

    // Take a screen shot
    private void TakeScreenShot(){
        screenShot.TakeScreenShot();
    }

    // Auto orbit camera and take screenshots of scene
    private void BeginOrbiting(){
        StartCoroutine(autoOrbitScan.BeginOrbiting());
    }

    private void EndOrbiting(){
        autoOrbitScan.EndOrbiting();
    }

    private void LoopAddObjects(){
        massAddObjects.LoopInstantiate();
    }
}
