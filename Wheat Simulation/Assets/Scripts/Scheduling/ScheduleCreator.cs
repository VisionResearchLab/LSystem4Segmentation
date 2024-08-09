using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ScheduleCreator : MonoBehaviour {

    // public void BuildSchedule(){
    //     List<string> fieldNames = new List<string>
    //     {
    //         "golden_inrows",
    //         "green_longawns",
    //         "green_smallheads",
    //         "pale_inrows",
    //         "pale_regular",
    //         "yellowish_green",
    //         "yellowish_green_inrows",
    //         "yellowish_green_largeheads",
    //         "young_green",
    //         "young_green_inrows"
    //     };

    //     // defining constants
    //     int totalImagesToTake = 40;
    //     int maxImagesPerDomain = totalImagesToTake / fieldNames.Count;

    //     int timeLimit = 5; // n minutes maximum
    //     int maxTimePerDomain = timeLimit / fieldNames.Count;

    //     // Create schedule
    //     Schedule schedule = new Schedule();
    //     foreach (string fieldName in fieldNames){
    //         PositionFinder.FieldLayout layout = fieldName.Contains("inrows") ? PositionFinder.FieldLayout.EightRows : PositionFinder.FieldLayout.Uniform;
    //         int wheatCount = UnityEngine.Random.Range(1800,2200);
    //         int underbrushCount = UnityEngine.Random.Range(18000,22000);

    //         Field field = new Field(fieldName, layout, wheatCount, underbrushCount);
            
    //         Event swapToAnyLightsource = new SwapLightSourceEvent("SwapLightsource", 10, 3.0f, 
    //             new List<LightSourceHandler.LightsourceType>{LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim, LightSourceHandler.LightsourceType.dark});

    //         Event swapToAnyGroundTexture = new SwapGroundTextureEvent("SwapGroundTexture", 3, 1.0f, 
    //             new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dark, TerrainHandler.TerrainType.dim, TerrainHandler.TerrainType.bright, TerrainHandler.TerrainType.dry, TerrainHandler.TerrainType.wet});

    //         List<string> eventNames = new List<string>(){swapToAnyLightsource.name, swapToAnyGroundTexture.name};

    //         Domain domain = new Domain(fieldName, eventNames, maxImagesPerDomain, maxTimePerDomain);

    //         schedule.fields.Add(field);
    //         schedule.domains.Add(domain);
    //     }

    //     SaveScheduleWithName(schedule, "instanceSegTest");
    // }

    // public void BuildTestSchedule(){
    //     //  schedule : contains a list of domains to cycle through
    //     //      domain : contains a field of wheat, events that happen at given intervals, and the time or number of images to stop at
    //     //          field : contains a reference to a wheat models directory, arrangement setting, and number of wheat and underbrush to place
    //     //          events : contains a list of events, each having an action (i.e. change texture), an interval (number of images to take between occurrences), and time to wait for baking
    //     //      domain
    //     //          field, events
    //     //      ...

    //     // Defining events
    //     Event swapToRandomDarkGroundTexture = new SwapGroundTextureEvent("swapToRandomDarkGroundTexture", 5, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dark});
    //     Event swapToRandomBrightGroundTexture = new SwapGroundTextureEvent("swapToRandomBrightGroundTexture", 5, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.bright});
    //     Event swapToWetGroundTexture = new SwapGroundTextureEvent("swapToWetGroundTexture", 5, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.wet});

    //     Event swapToRandomDarkLightsource = new SwapLightSourceEvent("swapToRandomDarkLightsource", 5, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dark});
    //     Event swapToRandomBrightOrDimLightsource = new SwapLightSourceEvent("swapToRandomBrightLightsource", 5, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim});

    //     // Defining fields
    //     Field testField = new Field("test", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass}, 500, 5000, 500);
    //     Field golden_inrowsField = new Field("golden_inrows", PositionFinder.FieldLayout.EightRows, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 500, 5000, 500);

    //     // Defining domains
    //     // Domain testDomain = new Domain("test", new List<string>(){"swapToRandomDarkGroundTexture", "swapToRandomDarkLightsource"}, 15, 5);
    //     Domain golden_inrowsDomain = new Domain("domain1", "golden_inrows", new List<string>(){"swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);
    //     Domain golden_inrowsDomain2 = new Domain("domain2", "golden_inrows", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);

    //     // Creating the schedule
    //     Schedule schedule = new Schedule();

    //     schedule.events.Add(swapToRandomDarkGroundTexture);
    //     schedule.events.Add(swapToRandomBrightGroundTexture);
    //     schedule.events.Add(swapToWetGroundTexture);
    //     schedule.events.Add(swapToRandomDarkLightsource);
    //     schedule.events.Add(swapToRandomBrightOrDimLightsource);

    //     schedule.fields.Add(testField);
    //     schedule.fields.Add(golden_inrowsField);

    //     // schedule.domains.Add(testDomain);
    //     schedule.domains.Add(golden_inrowsDomain);
    //     schedule.domains.Add(golden_inrowsDomain2);

    //     SaveScheduleWithName(schedule, "TestSchedule");
    // }

    public void BuildInstanceSeg2Test(){
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
        Field golden = new Field("golden", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass}, 500, 5000, 500);
        Field green_inrows = new Field("green_inrows", PositionFinder.FieldLayout.EightRows, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 500, 5000, 500);
        Field empty = new Field("golden", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass}, 0, 2500, 500);

        // Defining domains
        Domain goldenDomain = new Domain("golden", "golden", new List<string>(){"swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);
        Domain green_inrowsDomain = new Domain("green_inrows", "green_inrows", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);
        Domain emptyDomain = new Domain("empty", "empty", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, 15, 5);

        // Creating the schedule
        Schedule schedule = new Schedule();

        schedule.events.Add(swapToRandomDarkGroundTexture);
        schedule.events.Add(swapToRandomBrightGroundTexture);
        schedule.events.Add(swapToWetGroundTexture);
        schedule.events.Add(swapToRandomDarkLightsource);
        schedule.events.Add(swapToRandomBrightOrDimLightsource);

        schedule.fields.Add(golden);
        schedule.fields.Add(green_inrows);
        schedule.fields.Add(empty);

        // schedule.domains.Add(testDomain);
        schedule.domains.Add(goldenDomain);
        schedule.domains.Add(green_inrowsDomain);
        schedule.domains.Add(emptyDomain);

        SaveScheduleWithName(schedule, "InstanceSeg2Test");
    }

    public void BuildInstanceSeg2(){
        //  schedule : contains a list of domains to cycle through
        //      domain : contains a field of wheat, events that happen at given intervals, and the time or number of images to stop at
        //          field : contains a reference to a wheat models directory, arrangement setting, and number of wheat and underbrush to place
        //          events : contains a list of events, each having an action (i.e. change texture), an interval (number of images to take between occurrences), and time to wait for baking
        //      domain
        //          field, events
        //      ...

        // Defining events
        Event swapToRandomDarkGroundTexture = new SwapGroundTextureEvent("swapToRandomDarkGroundTexture", 10, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dark});
        Event swapToDimGroundTexture = new SwapGroundTextureEvent("swapToDimGroundTexture", 10, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dim});
        Event swapToRandomBrightGroundTexture = new SwapGroundTextureEvent("swapToRandomBrightGroundTexture", 10, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.bright});
        Event swapToDryGroundTexture = new SwapGroundTextureEvent("swapToDryGroundTexture", 10, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dry});
        Event swapToWetGroundTexture = new SwapGroundTextureEvent("swapToWetGroundTexture", 10, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.wet});
        Event swapToAnyGroundTexture = new SwapGroundTextureEvent("swapToAnyGroundTexture", 10, 1f, new List<TerrainHandler.TerrainType>(){TerrainHandler.TerrainType.dark, TerrainHandler.TerrainType.dim, TerrainHandler.TerrainType.bright, TerrainHandler.TerrainType.dry, TerrainHandler.TerrainType.wet});

        Event swapToRandomDarkLightsource = new SwapLightSourceEvent("swapToRandomDarkLightsource", 25, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dark});
        Event swapToRandomBrightOrDimLightsource = new SwapLightSourceEvent("swapToRandomBrightOrDimLightsource", 25, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim});
        Event swapToRandomDimLightsource = new SwapLightSourceEvent("swapToRandomDimLightsource", 25, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dim});
        Event swapToRandomBrightLightsource = new SwapLightSourceEvent("swapToRandomBrightLightsource", 25, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.bright});
        Event swapToAnyLightsource = new SwapLightSourceEvent("swapToAnyLightsource", 25, 3f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dark, LightSourceHandler.LightsourceType.dim, LightSourceHandler.LightsourceType.bright});

        // Defining fields
        Field golden = new Field("golden", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass}, 2000, 15000, 1000);
        Field golden_longawns = new Field("golden_longawns", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass}, 1500, 10000, 1200);
        Field green_inrows = new Field("green_inrows", PositionFinder.FieldLayout.EightRows, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1500, 12000, 500);
        Field green_young_longheads = new Field("green_young_longheads", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 2000, 20000, 1200);
        Field green_young_longheads_inrows = new Field("green_young_longheads_inrows", PositionFinder.FieldLayout.EightRows, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1500, 13000, 1500);
        Field yellow_longheads = new Field("yellow_longheads", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1200, 12000, 800);
        Field yellowishgreen_longawns = new Field("yellowishgreen_longawns", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1400, 12000, 800);
        Field yellowishgreen_longheads = new Field("yellowishgreen_longheads", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1800, 12000, 500);
        Field yellowishgreen_smallawns = new Field("yellowishgreen_smallawns", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass}, 800, 8000, 400);
        Field empty = new Field("empty", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass, WeedHandler.WeedType.dryGrass}, 0, 0, 1500);

        // Global variables
        int domainCount = 1;

        int totalImagesAvailable = 800;
        int maxImagesPerDomain = totalImagesAvailable / domainCount;

        int totalHoursAvailable = 1;
        int minutesAvailablePerDomain = totalHoursAvailable * 60 / domainCount;

        // Defining domains
        Domain goldenDomain = new Domain("golden", "golden", new List<string>(){"swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource", "swapToRandomDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain golden_longawnsDomain = new Domain("golden_longawns", "golden_longawns", new List<string>(){"swapToDimGroundTexture", "swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain green_inrowsDomain = new Domain("green_inrows", "green_inrows", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain green_young_longheadsDomain = new Domain("green_young_longheads", "green_young_longheads", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain green_young_longheads_inrowsDomain = new Domain("green_young_longheads_inrows", "green_young_longheads_inrows", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain yellow_longheadsDomain = new Domain("yellow_longheads", "yellow_longheads", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource", "swapToRandomDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain yellowishgreen_longawnsDomain = new Domain("yellowishgreen_longawns", "yellowishgreen_longawns", new List<string>(){"swapToWetGroundTexture", "swapToRandomBrightOrDimLightsource", "swapToRandomDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain yellowishgreen_longheadsDomain = new Domain("yellowishgreen_longheads", "yellowishgreen_longheads", new List<string>(){"swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource", "swapToRandomDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain yellowishgreen_smallawnsDomain = new Domain("yellowishgreen_smallawns", "yellowishgreen_smallawns", new List<string>(){"swapToRandomBrightGroundTexture", "swapToRandomBrightOrDimLightsource", "swapToRandomDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);
        Domain emptyDomain = new Domain("empty", "empty", new List<string>(){"swapToAnyGroundTexture", "swapToAnyLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain);

        // Creating the schedule
        Schedule schedule = new Schedule();

        // schedule.events.Add(swapToRandomDarkGroundTexture);
        // schedule.events.Add(swapToDimGroundTexture);
        // schedule.events.Add(swapToRandomBrightGroundTexture);
        // schedule.events.Add(swapToDryGroundTexture);
        // schedule.events.Add(swapToWetGroundTexture);
        schedule.events.Add(swapToAnyGroundTexture);
        // schedule.events.Add(swapToRandomDarkLightsource);
        // schedule.events.Add(swapToRandomBrightOrDimLightsource);
        schedule.events.Add(swapToAnyLightsource);
        // schedule.events.Add(swapToRandomDimLightsource);
        // schedule.events.Add(swapToRandomBrightLightsource);

        // schedule.fields.Add(golden);
        // schedule.fields.Add(golden_longawns);
        // schedule.fields.Add(green_inrows);
        // schedule.fields.Add(green_young_longheads);
        // schedule.fields.Add(green_young_longheads_inrows);
        // schedule.fields.Add(yellow_longheads);
        // schedule.fields.Add(yellowishgreen_longawns);
        // schedule.fields.Add(yellowishgreen_longheads);
        // schedule.fields.Add(yellowishgreen_smallawns);
        schedule.fields.Add(empty);

        // schedule.domains.Add(testDomain);
        // schedule.domains.Add(goldenDomain);
        // schedule.domains.Add(golden_longawnsDomain);
        // schedule.domains.Add(green_inrowsDomain);
        // schedule.domains.Add(green_young_longheadsDomain);
        // schedule.domains.Add(green_young_longheads_inrowsDomain);
        // schedule.domains.Add(yellow_longheadsDomain);
        // schedule.domains.Add(yellowishgreen_longawnsDomain);
        // schedule.domains.Add(yellowishgreen_longheadsDomain);
        // schedule.domains.Add(yellowishgreen_smallawnsDomain);
        schedule.domains.Add(emptyDomain);

        SaveScheduleWithName(schedule, "InstanceSeg2");
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