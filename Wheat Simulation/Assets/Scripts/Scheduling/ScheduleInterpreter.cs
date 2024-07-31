using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class ScheduleInterpreter : MonoBehaviour {
    public Domain currentDomain;

    private bool interrupt = false; // Used to stop the schedule via a key input.


    void Start(){
        Event swapLightSource = new Event("Switch Sky", 4.0f, SwapLightSource);
        eventDictionary.AddEvent(swapLightSource, 20);

        // Event moveTerrainPosition = new Event("Move Terrain Position", 0.5f, MoveTerrain);
        // eventDictionary.AddEvent(moveTerrainPosition, 15);
    }

    public IEnumerator RunSchedule(Schedule schedule){
        List<Domain> domains = schedule.domains;
        List<Order> orders = schedule.orders;

        Domain GetDomainForOrder(Order order){
            foreach (Domain domain in domains){
                if (domain.name == order.domainName) { return domain; }
            }
            return null;
        }

        foreach (Order order in orders){
            Domain domain = GetDomainForOrder(order);
            yield return StartCoroutine(InterpretOrder(order, domain));
        }
    }

    public IEnumerator InterpretOrder(Order order, Domain domain){
        LoadDomain(domain);

        DateTime initialTime = DateTime.Now;
        int minutesLimit = order.minutesLimit;

        int currentIteration = 0;
        int imagesLimit = order.imagesLimit;

        bool timeIsValid(){
            // Check if there is a time limit
            if (minutesLimit == -1){
                return true;
            }
            // If there is a time limit, check if the time has passed
            if ((DateTime.Now - initialTime).TotalMinutes < minutesLimit){
                return true;
            }
            return false;
        }

        bool imageCountIsValid(){
            // Check if there is a image count limit
            if (imagesLimit == -1){
                return true;
            }
            // Return true if there are less images than the image limit
            if (currentIteration < imagesLimit){
                return true;
            }
            return false;
        }

        while (!interrupt && timeIsValid() && imageCountIsValid()){
            yield return null; // Wait a frame to allow the user to see changes

            // Run iteration: Check events, move camera, take picture.
            yield return StartCoroutine(RunIteration(currentIteration));
            
            // Print a progress message
            StringBuilder progress = new StringBuilder();
                progress.Append($"Domain name: {domain.name}\n");
            if (imageCountIsValid()){
                progress.Append($"{currentIteration + 1}/{imagesLimit} images created.\n");
            } if (timeIsValid()){
                progress.Append($"Up to {minutesLimit - (DateTime.Now - initialTime).TotalMinutes} minutes remaining");
            }
            Debug.Log(progress.ToString());
            currentIteration += 1;
        }
        if (interrupt){
            interrupt = false;
        }
    }

    private IEnumerator RunIteration(int currentIteration){

        // Run each event in the EventDictionary for this iteration, in no particular order.
        MoveCamera();
        yield return new WaitForSeconds(1.0f); // TEST, delete this later
        
        float waitTime = eventDictionary.RunEventsForIteration(currentIteration);
        // Debug.Log($"Wait time: {waitTime}");
        yield return new WaitForSeconds(waitTime);

        // Take the screenshots
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
        if (currentDomain != domain){
            currentDomain = domain;
            currentDomain.Build();
        }
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