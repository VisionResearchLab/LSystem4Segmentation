using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class InputHandler : MonoBehaviour
{
    // Scripts
    [SerializeField] private ScreenShot screenShot;
    [SerializeField] private OrbitHandler autoOrbitScan;
    [SerializeField] private UnderbrushHandler underbrushHandler;
    [SerializeField] private InstantiateWheat instantiateWheat;
    [SerializeField] private ScheduleInterpreter scheduleInterpreter;
    [SerializeField] private ScheduleCreator scheduleCreator;

    // Map KeyCodes to Actions
    Dictionary<KeyCode, Action> keyMap = new Dictionary<KeyCode, Action>();
    
    // Schedule to run on button press
    [SerializeField] private string nameOfScheduleToRun;
    
    void Start(){
        // Maps keybinds to functions in other scripts

        keyMap[KeyCode.Alpha1] = AddManyWheatUniformLayout;
        keyMap[KeyCode.Alpha2] = AddManyWheatRowLayout;
        keyMap[KeyCode.Alpha3] = AddManyUnderbrushUniformLayout;
        keyMap[KeyCode.Alpha4] = AddManyUnderbrushRowLayout;

        keyMap[KeyCode.T] = TakeScreenShot;

        keyMap[KeyCode.Y] = CreateSchedule;
        keyMap[KeyCode.U] = RunSchedule;
        keyMap[KeyCode.R] = InterruptSchedule; // untested
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

    // Take a screen shot
    private void TakeScreenShot(){
        screenShot.TakeScreenShot();
    }

    // Run the build schedule command from ScheduleCreator
    private void CreateSchedule(){
        scheduleCreator.BuildTestSchedule();
    }

    // Run the schedule given in the editor input
    private void RunSchedule(){
        Schedule schedule = scheduleInterpreter.LoadScheduleByName(nameOfScheduleToRun);
        StartCoroutine(scheduleInterpreter.InterpretSchedule(schedule));
    }

    // Try to interrupt the schedule processing
    private void InterruptSchedule(){
        scheduleInterpreter.Interrupt();
    }

    
    // Add wheat or underbrush en masse
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
