using System.Collections;
using System;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor.Build;
using System.IO;
using Newtonsoft.Json.Serialization;

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

        keyMap[KeyCode.Y] = CreateTestSchedule;
        keyMap[KeyCode.U] = RunTestSchedule;

        keyMap[KeyCode.R] = InterruptSchedule; // untested

        keyMap[KeyCode.F] = QuickScheduleMaker;
        keyMap[KeyCode.G] = RunQuickSchedule;


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
        string domainName0 = "green_longawns";
        string domainName1 = "pale_inrows";
        int imagesLimit = 15;
        int timeLimit = 5;
        Domain domain0 = new Domain(domainName0, PositionFinder.FieldLayout.Uniform, 2000, 20000);
        Domain domain1 = new Domain(domainName1, PositionFinder.FieldLayout.EightRows, 2000, 20000);

        Order order0 = new Order(domainName0, imagesLimit, timeLimit);
        Order order1 = new Order(domainName1, imagesLimit, timeLimit);

        // Create the schedule
        Schedule schedule = new Schedule();
        schedule.domains.Add(domain0);
        schedule.domains.Add(domain1);

        schedule.orders.Add(order0);
        schedule.orders.Add(order1);

        // Save the schedule
        SaveScheduleWithName(schedule, "sched");
    }

    private void QuickScheduleMaker(){
        List<string> domainNames = new List<string>
        {
            "golden_inrows",
            "green_longawns",
            "green_smallheads",
            "pale_inrows",
            "pale_regular",
            "yellowish_green",
            "yellowish_green_inrows",
            "yellowish_green_largeheads",
            "young_green",
            "young_green_inrows"
        };

        // defining constants
        int totalImagesToTake = 10000;
        int imagesPerDomain = totalImagesToTake / domainNames.Count;

        int timeLimit = 480; // n minutes maximum
        int timePerDomain = timeLimit / domainNames.Count;

        Schedule schedule = new Schedule();
        foreach (string domainName in domainNames){
            PositionFinder.FieldLayout layout = domainName.Contains("inrows") ? PositionFinder.FieldLayout.EightRows : PositionFinder.FieldLayout.Uniform;
            int wheatCount = UnityEngine.Random.Range(1800,2200);
            int underbrushCount = UnityEngine.Random.Range(18000,22000);

            Domain domain = new Domain(domainName, layout, wheatCount, underbrushCount);
            Order order = new Order(domainName, imagesPerDomain, timePerDomain);

            schedule.domains.Add(domain);
            schedule.orders.Add(order);
        }

        SaveScheduleWithName(schedule, "instanceSeg1");
    }

    // Create a test schedule and run it.
    private void RunTestSchedule(){
        Schedule schedule = LoadScheduleByName("sched");
        RunSchedule(schedule);
    }

    private void RunQuickSchedule(){
        string quickSched = "instanceSeg1";
        Schedule schedule = LoadScheduleByName(quickSched);

        RunSchedule(schedule);
    }

    private void RunSchedule(Schedule schedule){
        StartCoroutine(scheduleInterpreter.RunSchedule(schedule));
    }

    private Schedule LoadScheduleByName(string name){
        string fullPath = Path.GetFullPath($"Assets/Schedules/{name}.json");
        string jsonText = System.IO.File.ReadAllText(fullPath);
        return JsonConvert.DeserializeObject<Schedule>(jsonText);
    }

    private void SaveScheduleWithName(Schedule schedule, string name){
        string jsonString = JsonConvert.SerializeObject(schedule);
        string fullPath = Path.GetFullPath($"Assets/Schedules/{name}.json");
        System.IO.File.WriteAllTextAsync(fullPath, jsonString);
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
