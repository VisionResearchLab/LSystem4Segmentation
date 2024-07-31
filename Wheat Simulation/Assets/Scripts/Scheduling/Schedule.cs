using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

[Serializable]
public class Schedule {
    public List<Domain> domains = new List<Domain>();
    public List<Event> events = new List<Event>();
    public List<Order> orders = new List<Order>();
}

[Serializable]
public class Domain {
    public string name;
    public PositionFinder.FieldLayout arrangement;
    public int wheatCount;
    public int underbrushCount;


    // Inputs
    public Domain(
        string name,
        PositionFinder.FieldLayout arrangement  = PositionFinder.FieldLayout.Uniform, 
        int wheatCount                          = 2000, 
        int underbrushCount                     = 20000
        )
        {
            this.name = name;
            this.arrangement = arrangement;
            this.wheatCount = wheatCount;
            this.underbrushCount = underbrushCount;
        }

    public void Build(){
        // Clear previous domain
        ObjectPooler.ClearAllPools();

        // Create the wheat prefabs pool for this domain
        string wheatPrefabsDirectory = GetPrefabDirectory("Wheat Models", name);
        // Debug.Log("Trying to initialize pools from directory: " + wheatPrefabsDirectory);
        ObjectPooler.InitializePoolsFromDirectory(ObjectPooler.PoolType.Wheat, wheatPrefabsDirectory, wheatCount);

        // Create the underbrush prefabs pool for this domain
        string underbrushPrefabsDirectory = GetPrefabDirectory("Ground Cover Models", name);
        // Debug.Log("Trying to initialize pools from directory: " + underbrushPrefabsDirectory);
        ObjectPooler.InitializePoolsFromDirectory(ObjectPooler.PoolType.Underbrush, underbrushPrefabsDirectory, underbrushCount);

        // Instantiate the wheat
        InstantiateWheat instantiateWheat = Object.FindObjectOfType<InstantiateWheat>();
        instantiateWheat.LoopAddWheat(wheatCount, arrangement, 5);
        // Debug.Log(arrangement.ToString());

        // Instantiate underbrush
        UnderbrushHandler underbrushHandler = Object.FindObjectOfType<UnderbrushHandler>();
        underbrushHandler.LoopInstantiateUnderbrushInBounds(underbrushCount, arrangement);
    }

    static private string GetPrefabDirectory(string prefabType, string domainName){
        string relativePath = $"Assets/Resources/Prefabs/{prefabType}/{domainName}";
        string fullPath = Path.GetFullPath(relativePath);
        if (Directory.Exists(fullPath))
            return fullPath;
        else {
            Debug.LogError("Could not find the domain path.");
            return null;
        }
    }
}


[Serializable]
public abstract class Event {
    public string name;
    public int frequency;
    public float timeToExecute;

    // Inputs
    public Event(string name, int frequency, float timeToExecute) 
    {
        this.name = name;
        this.frequency = frequency;
        this.timeToExecute = timeToExecute;
    }

    public bool CheckEventForIteration(int iteration){
        if (iteration != 0 && iteration % frequency == 0){
            RunEvent();
            return true;
        }
        return false;
    }

    public abstract void RunEvent();
}

[Serializable]
public class SwapLightSourceEvent : Event {
    public List<LightSourceHandler.LightsourceType> lightsourceTypes;

    public SwapLightSourceEvent(string name, int frequency, float timeToExecute, List<LightSourceHandler.LightsourceType> lightsourceTypes = null)
    : base (name, frequency, timeToExecute)
    {
        this.lightsourceTypes = lightsourceTypes;
    }

    public void RunEvent(){
        
    }
}

[Serializable]
public class SwapGroundTextureEvent : Event {
    public List<TerrainHandler.TerrainType> terrainTypes;

    public SwapGroundTextureEvent(string name, int frequency, float timeToExecute, List<TerrainHandler.TerrainType> terrainTypes = null)
    : base (name, frequency, timeToExecute)
    {
        this.terrainTypes = terrainTypes;
    }
}

[Serializable]
public class MoveGroundEvent : Event {
    public int distanceToMoveTerrain;

    public MoveGroundEvent(string name, int frequency, float timeToExecute, int distanceToMoveTerrain = -1)
    : base (name, frequency, timeToExecute)
    {
        this.distanceToMoveTerrain = distanceToMoveTerrain;
    }
}

[Serializable]
public class Order {
    public string domainName;
    public List<string> eventNames;
    public int imagesLimit; // -1 represents no limit
    public int minutesLimit; // -1 represents no limit

    public Order(string domainName, List<string> eventNames, int imagesLimit = -1, int minutesLimit = -1) 
    {
        this.domainName = domainName;
        this.imagesLimit = imagesLimit;
        this.minutesLimit = minutesLimit;
    }
}