using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using System;

public class Scheduler : MonoBehaviour {
    public Domain currentDomain;

    private EventDictionary eventDictionary = new EventDictionary();

    private bool interrupt = false; // Used to stop the schedule via a key input.


    void Start(){
        Event swapLightSource = new Event("Switch Sky", 4.0f, SwapLightSource);
        eventDictionary.AddEvent(swapLightSource, 20);

        Event moveTerrainPosition = new Event("Move Terrain Position", 0.5f, MoveTerrain);
        eventDictionary.AddEvent(moveTerrainPosition, 15);
    }

    public void TestWithDomain(){
        // Test with domain greenishWheat
        Domain greenishWheatDomain = new Domain("greenishWheat");
        StartCoroutine(ScheduleWithImageLimit(greenishWheatDomain, 100));
        StartCoroutine(ScheduleWithTimeLimit(greenishWheatDomain, 100));
    }

    public IEnumerator ScheduleWithTimeLimit(Domain domain, int minutesLimit=int.MaxValue){
        LoadDomain(domain);
        DateTime initialTime = DateTime.Now;
        int currentIteration = 0;

        while (!interrupt && (DateTime.Now - initialTime).TotalMinutes < minutesLimit){
            // Orbit the camera. Take a picture.
            yield return StartCoroutine(RunIteration(currentIteration));
            currentIteration += 1;
            
            // Log remaining time
            Debug.Log($"Remaining time: {minutesLimit - (DateTime.Now - initialTime).TotalMinutes} min");
        }
        if (interrupt){
            interrupt = false;
        }
    }

    public IEnumerator ScheduleWithImageLimit(Domain domain, int imagesLimit=int.MaxValue){
        LoadDomain(domain);
        int currentIteration = 0;

        while (!interrupt && currentIteration < imagesLimit){
            // Orbit the camera. Take a picture.
            yield return StartCoroutine(RunIteration(currentIteration));
            currentIteration += 1;

            // Log remaining images
            Debug.Log($"{currentIteration}/{imagesLimit} images taken.");
        }
        if (interrupt){
            interrupt = false;
        }
    }

    private IEnumerator RunIteration(int currentIteration){
        // Run each event in the EventDictionary for this iteration, in no particular order.
        float waitTime = eventDictionary.RunEventsForIteration(currentIteration);
        yield return new WaitForSeconds(waitTime);

        // Take the screenshots
        MoveCamera();
        yield return StartCoroutine(TakePicture());
    }

    private class EventDictionary {
        Dictionary<Event, int> iterationsBetweenEvent = new Dictionary<Event, int>();

        public EventDictionary(Dictionary<Event, int> iterationsBetweenEvent = null){
            if (iterationsBetweenEvent != null){
                this.iterationsBetweenEvent = iterationsBetweenEvent;
            }
        }

        public void AddEvent(Event ev, int iterationsBetweenEvent){
            this.iterationsBetweenEvent[ev] = iterationsBetweenEvent;
        }

        public void RemoveEvent(Event ev){
            iterationsBetweenEvent.Remove(ev);
        }

        public float RunEventsForIteration(int currentIteration){
            // Define events list and wait time sum
            List<Event> events = new List<Event>();
            float waitTime = 0f;

            // Skip iteration 0
            if (currentIteration != 0){
                // Get events for this iteration by modulus
                foreach (Event thisEvent in iterationsBetweenEvent.Keys){
                    if (currentIteration % iterationsBetweenEvent[thisEvent] == 0){
                        events.Add(thisEvent);
                    }
                }
            }
           
            // Execute events and sum wait times
            if (events.Count > 0){
                // Add up their wait times
                foreach (Event ev in events){
                    ev.Execute();
                    waitTime += ev.timeToExecute;
                }
            }

            return waitTime;
        }

        public static EventDictionary emptyEventDictionary = new EventDictionary(new Dictionary<Event, int>());
    }

    private class Event {
        public string name;
        public float timeToExecute;
        public Action function;

        public Event(string name, float timeToExecute, Action function){
            this.name = name;
            this.timeToExecute = timeToExecute;
            this.function = function;
        }

        public void Execute(){
            if (function != null){
                function();
            }
        }
    }

    // Functions for events

    //  Load the next Domain in the domains list.
    private void LoadDomain(Domain domain){
        currentDomain = domain;
        currentDomain.Build();
    }

    //  Light source handling
    private void SwapLightSource(){
        LightSourceHandler lightSourceHandler = FindObjectOfType<LightSourceHandler>();
        lightSourceHandler.SwapLightSource();
    }

    //  Move camera using OrbitHandler
    private void MoveCamera(){
        OrbitHandler orbitHandler = FindObjectOfType<OrbitHandler>();
        orbitHandler.MoveCameraRandomly();
    }

    private IEnumerator TakePicture(){
        ScreenShot screenShot = FindObjectOfType<ScreenShot>();
        float timeToWait = 0.1f;
        yield return StartCoroutine(screenShot.ScreenshotSequenceEnum(timeToWait));
        yield return null;
    }

    private void MoveTerrain(){
        TerrainHandler terrainHandler = FindObjectOfType<TerrainHandler>();
        terrainHandler.MoveTerrainPosition();
    }

    public void Interrupt(){
        interrupt = true;
    }
}