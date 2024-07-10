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
    [SerializeField] private MouseOverWheatHandler mouseOverWheatHandler;

    // Scan Wheat
    [SerializeField] private ScanScene scanScene;

    // Place Wheat at Cursor
    [SerializeField] private PlaceAtCursor placeAtCursor;

    // Screenshot script
    [SerializeField] private ScreenShot screenShot;
    
    // Auto orbit scanning script
    [SerializeField] private AutoOrbitScan autoOrbitScan;

    // Place underbrush objects script
    [SerializeField] private UnderbrushHandler underbrushHandler;

    // Place underbrush objects script
    [SerializeField] private InstantiateWheat instantiateWheat;

    // Map KeyCodes to Actions
    Dictionary<KeyCode, Action> keyMap = new Dictionary<KeyCode, Action>();
    
    void Start(){
        // Maps keybinds to functions in other scripts
        // keyMap[KeyCode.F] = DetectWheatPart;
        // keyMap[KeyCode.E] = ScanWheat;
        // keyMap[KeyCode.Q] = PlaceObjectsAtCursor;
        keyMap[KeyCode.Alpha1] = AddManyWheatUniformLayout;
        keyMap[KeyCode.Alpha2] = AddManyWheatRowLayout;
        keyMap[KeyCode.Alpha3] = AddManyUnderbrushUniformLayout;
        keyMap[KeyCode.Alpha4] = AddManyUnderbrushRowLayout;
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

    // private void PlaceObjectsAtCursor(){
    //     placeAtCursor.PlaceObjectsAtCursor();
    // }

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

    private void AddManyWheatUniformLayout(){
        instantiateWheat.LoopAddWheat(PositionFinder.FieldLayout.Uniform);
    }

    private void AddManyWheatRowLayout(){
        instantiateWheat.LoopAddWheat(PositionFinder.FieldLayout.EightRows);
    }

    private void AddManyUnderbrushUniformLayout(){
        underbrushHandler.LoopInstantiateUnderbrushInBounds(PositionFinder.FieldLayout.Uniform);
    }

    private void AddManyUnderbrushRowLayout(){
        underbrushHandler.LoopInstantiateUnderbrushInBounds(PositionFinder.FieldLayout.EightRows);
    }
}
