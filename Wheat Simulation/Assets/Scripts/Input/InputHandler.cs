using System.Collections;
using System;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor.Build;
using System.IO;

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
    
    [SerializeField] private ScheduleInterpreter scheduleInterpreter;
    
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
        keyMap[KeyCode.R] = CreateTestSchedule;
        keyMap[KeyCode.Y] = RunTestSchedule;
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

    private void CreateTestSchedule(){
        // Create the domain and order
        string domainName = "greenishWheat";
        int imagesLimit = 15;
        int timeLimit = 5;
        Domain domain = new Domain(domainName, PositionFinder.FieldLayout.EightRows, 4000, 40000);
        Order order = new Order(domainName, imagesLimit, timeLimit);

        // Create the schedule
        Schedule schedule = new Schedule();
        schedule.domains.Add(domain);
        schedule.orders.Add(order);

        string testScheduleName = "sched";
        string fullPath = Path.GetFullPath($"Assets/Schedules/{testScheduleName}.json");
        JsonConvert.SerializeObject(fullPath);
    }

    // Create a test schedule and run it.
    private void RunTestSchedule(){
        string testSched = "sched";
        Schedule schedule = LoadScheduleByName(testSched);

        RunSchedule(schedule);
    }

    private void RunSchedule(Schedule schedule){
        StartCoroutine(scheduleInterpreter.RunSchedule(schedule));
    }

    private Schedule LoadScheduleByName(string name){
        string fullPath = Path.GetFullPath($"Assets/Schedules/{name}.json");
        return JsonConvert.DeserializeObject<Schedule>(fullPath);
    }

    private void InterruptSchedule(){
        scheduleInterpreter.Interrupt();
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
