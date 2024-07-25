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

    // Place Wheat at Cursor
    [SerializeField] private PlaceAtCursor placeAtCursor;

    // Screenshot script
    [SerializeField] private ScreenShot screenShot;
    
    // Auto orbit scanning script
    [SerializeField] private OrbitHandler autoOrbitScan;

    // Place underbrush objects script
    [SerializeField] private UnderbrushHandler underbrushHandler;

    // Place underbrush objects script
    [SerializeField] private InstantiateWheat instantiateWheat;

    // Map KeyCodes to Actions
    Dictionary<KeyCode, Action> keyMap = new Dictionary<KeyCode, Action>();
    
    [SerializeField] private Scheduler scheduler;
    
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
        keyMap[KeyCode.Y] = RunSchedule;
        keyMap[KeyCode.U] = InterruptSchedule;
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

    // Take a screen shot
    private void TakeScreenShot(){
        screenShot.TakeScreenShot();
    }

    // Auto orbit camera and take screenshots of scene
    private void RunSchedule(){
        scheduler.TestWithDomain();
    }

    private void InterruptSchedule(){
        scheduler.Interrupt();
    }

    private void AddManyWheatUniformLayout(){
        instantiateWheat.LoopAddWheat(100, PositionFinder.FieldLayout.Uniform);
    }

    private void AddManyWheatRowLayout(){
        instantiateWheat.LoopAddWheat(100, PositionFinder.FieldLayout.EightRows);
    }

    private void AddManyUnderbrushUniformLayout(){
        underbrushHandler.LoopInstantiateUnderbrushInBounds(1000, PositionFinder.FieldLayout.Uniform);
    }

    private void AddManyUnderbrushRowLayout(){
        underbrushHandler.LoopInstantiateUnderbrushInBounds(1000, PositionFinder.FieldLayout.EightRows);
    }
}
