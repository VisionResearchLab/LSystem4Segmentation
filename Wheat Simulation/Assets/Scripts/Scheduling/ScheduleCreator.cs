using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ScheduleCreator : MonoBehaviour {

    public void BuildSchedule(){
        List<string> fieldNames = new List<string>
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
        int totalImagesToTake = 40;
        int maxImagesPerDomain = totalImagesToTake / fieldNames.Count;

        int timeLimit = 5; // n minutes maximum
        int maxTimePerDomain = timeLimit / fieldNames.Count;

        // Create schedule
        Schedule schedule = new Schedule();
        foreach (string fieldName in fieldNames){
            PositionFinder.FieldLayout layout = fieldName.Contains("inrows") ? PositionFinder.FieldLayout.EightRows : PositionFinder.FieldLayout.Uniform;
            int wheatCount = UnityEngine.Random.Range(1800,2200);
            int underbrushCount = UnityEngine.Random.Range(18000,22000);

            Field field = new Field(fieldName, layout, wheatCount, underbrushCount);
            
            Event swapToAnyLightsource = new SwapLightSourceEvent("SwapLightsource", 10, 3.0f, 
                new List<LightSourceHandler.LightsourceType>{LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim, LightSourceHandler.LightsourceType.dark});

            Event swapToAnyGroundTexture = new SwapGroundTextureEvent("SwapGroundTexture", 3, 1.0f, 
                new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dark, TerrainHandler.TerrainType.dim, TerrainHandler.TerrainType.bright, TerrainHandler.TerrainType.dry, TerrainHandler.TerrainType.wet});

            List<string> eventNames = new List<string>(){swapToAnyLightsource.name, swapToAnyGroundTexture.name};

            Domain domain = new Domain(fieldName, eventNames, maxImagesPerDomain, maxTimePerDomain);

            schedule.fields.Add(field);
            schedule.domains.Add(domain);
        }

        SaveScheduleWithName(schedule, "instanceSegTest");
    }

    public void BuildTestSchedule(){
        //  schedule : contains a list of domains to cycle through
        //      domain : contains a field of wheat, events that happen at given intervals, and the time or number of images to stop at
        //          field : contains a reference to a wheat models directory, arrangement setting, and number of wheat and underbrush to place
        //          events : contains a list of events, each having an action (i.e. change texture), an interval (number of images to take between occurrences), and time to wait for baking
        //      domain
        //          field, events
        //      ...

        // Defining events
        Event swapToRandomDarkGroundTexture = new SwapGroundTextureEvent("swapToRandomDarkGroundTexture", 5, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dark});
        Event swapToRandomBrightGroundTexture = new SwapGroundTextureEvent("swapToRandomBrightGroundTexture", 5, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.bright});
        Event swapToWetGroundTexture = new SwapGroundTextureEvent("swapToWetGroundTexture", 5, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.wet});

        Event swapToRandomDarkLightsource = new SwapLightSourceEvent("swapToRandomDarkLightsource", 5, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dark});
        Event swapToRandomBrightOrDimLightsource = new SwapLightSourceEvent("swapToRandomBrightLightsource", 5, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim});

        // Defining fields
        Field testField = new Field("test", PositionFinder.FieldLayout.Uniform, 2000, 20000);
        Field golden_inrowsField = new Field("golden_inrows", PositionFinder.FieldLayout.EightRows, 2000, 20000);

        // Defining domains
        Domain testDomain = new Domain("test", new List<string>(){"swapToRandomDarkGroundTexture", "swapToRandomDarkLightsource"}, 15, 5);
        Domain golden_inrowsDomain = new Domain("golden_inrows", new List<string>(){"swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);
        Domain golden_inrowsDomain2 = new Domain("golden_inrows", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);

        // Creating the schedule
        Schedule schedule = new Schedule();

        schedule.events.Add(swapToRandomDarkGroundTexture);
        schedule.events.Add(swapToRandomBrightGroundTexture);
        schedule.events.Add(swapToWetGroundTexture);
        schedule.events.Add(swapToRandomDarkLightsource);
        schedule.events.Add(swapToRandomBrightOrDimLightsource);

        schedule.fields.Add(testField);
        schedule.fields.Add(golden_inrowsField);

        schedule.domains.Add(testDomain);
        schedule.domains.Add(golden_inrowsDomain);
        schedule.domains.Add(golden_inrowsDomain2);

        SaveScheduleWithName(schedule, "TestSchedule");
    }

    private JsonSerializerSettings settings = new JsonSerializerSettings{
        TypeNameHandling = TypeNameHandling.All
    };

    public void SaveScheduleWithName(Schedule schedule, string name){
        string jsonString = JsonConvert.SerializeObject(schedule, Formatting.Indented, settings);
        string fullPath = Path.GetFullPath($"Assets/Schedules/{name}.json");
        File.WriteAllTextAsync(fullPath, jsonString);
    }
}