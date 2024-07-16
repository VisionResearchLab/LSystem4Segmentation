using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Xml;

public class ScreenShot : MonoBehaviour
{
    // Cameras
    public Camera mainCam;


    // UI canvas group (to hide it for screenshots)
    public CanvasGroup canvasGroup;


    // Save directory
    [SerializeField] private string datasetsDirectory = "C:/Users/xSkul/OneDrive/Documents/Projects/Wheat/wheat/Datasets";
    [SerializeField] private string datasetName = "1024x1024-8";
    [SerializeField] private string domainName = "testDomain";
    private string datasetDirectory;
    private string domainDirectory;

    // Setting to decide whether awns should be labelled as wheat heads or passed through
    [SerializeField] private bool labelAwnsAsWheatHeads;

    // Screen width and height
    private int width;
    private int height;

    // Get the InstanceLabels object which will overwrite the JSON after each image
    private AnnotationJSON annotationJSON;
    private string annotationJSONPath;


    // Define scripts with functions that need to be called
    private void Start(){
        ShowUI();

        // Define some paths
        datasetDirectory = $"{datasetsDirectory}/{datasetName}/";
        domainDirectory = $"{datasetsDirectory}/{datasetName}/{domainName}/";
        annotationJSONPath = $"{domainDirectory}annotations.json";

        // Define width and height as the current screen size NOTE: This causes errors if you switch screen size during runtime!
        width = Screen.width;
        height = Screen.height;
        
        // Check if the DATASETS directory exists. If it does, proceed.
        if (Directory.Exists(datasetsDirectory)){
            // If the DOMAIN directory does not exist based on the private name, create it.
            // Also create a new JSON.
            if (!Directory.Exists(domainDirectory)){
                Directory.CreateDirectory(domainDirectory);
                annotationJSON = new AnnotationJSON();
            } 
            // Try to load instanceLabels from the JSON if the domain was found
            else {
                if (File.Exists(annotationJSONPath)){
                    annotationJSON = JsonUtility.FromJson<AnnotationJSON>(annotationJSONPath);
                } else {
                    InitializeAnnotationWithCategories();
                }
            }
        }
        else {
            UnityEngine.Debug.LogError("The datasets directory could not be found.");
        }
    }

    // Call ScreenshotSequenceEnum with default time to wait of 0.1 seconds
    public void TakeScreenShot(){
        float secondsDelay = 0.1f;
        StartCoroutine(ScreenshotSequenceEnum(secondsDelay));
    }

    public IEnumerator ScreenshotSequenceEnum(float secondsDelay){
        DateTime dateTimeNow = DateTime.Now;
        string dateTime = dateTimeNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string dateTimeJSONFormatting = dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss");

        string imageName = dateTime + "_image" + ".png";
        string imagePath = GetUniqueAssetPathInDomain(imageName);

        HideUI();
        yield return new WaitForSeconds(secondsDelay);

        // Save the image
        StartCoroutine(ImageScreenshot(imagePath));

        int imageID = annotationJSON.images.Count;
        AddImageToAnnotation(imageID, imageName, dateTimeJSONFormatting);

        // Save the annotation
        // AnnotateScreenshotRaycast(labelPath);
        CreateInstanceLabels(imageID);
        UpdateJSONFile();

        yield return new WaitForEndOfFrame();
        ShowUI();
    }

    // Returns a unique filepath in the screenshots folder based on the given name
    private string GetUniqueAssetPathInDomain(string name){
        return AssetDatabase.GenerateUniqueAssetPath(domainDirectory + name);
    }

    // Hide UI
    private void HideUI(){
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Show UI
    private void ShowUI(){
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // Use texture to avoid screenshot lag
    private IEnumerator ImageScreenshot(string path){
        yield return new WaitForEndOfFrame();
        // https://stackoverflow.com/a/36188311
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true); // mipchain??
        screenShot.ReadPixels(new Rect(0,0,Screen.width,Screen.height), 0, 0);
        screenShot.Apply();
        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);
        File.WriteAllBytes(path, bytes);
    }

    private NativeArray<RaycastHit> RaycastFromCamera(){
        Stopwatch sw = new Stopwatch();
        sw.Start(); // track raycast

        // Prepare for raycast commands and results
        NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(width * height, Allocator.TempJob);
        NativeArray<RaycastHit> raycastResults = new NativeArray<RaycastHit>(width * height, Allocator.TempJob);

        int index = 0;

        // Create raycast commands for each pixel
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 screenPoint = new Vector3(x, y, 0);
                Ray ray = mainCam.ScreenPointToRay(screenPoint);
                if (labelAwnsAsWheatHeads){
                    raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction);
                } else {
                    raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction, layerMask:Wheat.awnsLayerMask);
                }
                
                index++;
            }
        }

        // Schedule the batch of raycasts
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 32);

        // Complete the job handle
        handle.Complete();


        raycastCommands.Dispose();

        sw.Stop();
        UnityEngine.Debug.Log($"Time to raycast: {sw.ElapsedMilliseconds} ms");

        return raycastResults;
    }

    private void AnnotateScreenshotRaycast(string path){
        Stopwatch sw = new Stopwatch();
        sw.Start(); // track raycast

        NativeArray<RaycastHit> raycastResults = RaycastFromCamera();

        // // Create the texture and set pixels based on raycast results
        Texture2D screenShot = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int i = 0; i < width*height; i++){
            RaycastHit hit = raycastResults[i];
            GameObject obj = hit.transform ? hit.transform.gameObject : null;
            if (obj != null && obj.name.StartsWith("Head")) // It would be better, but much more expensive, to use Wheat.isWheat().
            {
                colors[i] = Color.white;
            }
            else
            {
                colors[i] = Color.black;
            }
        }

        screenShot.SetPixels(colors);
        screenShot.Apply();
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Clean up
        raycastResults.Dispose();
        UnityEngine.Object.Destroy(screenShot);

        sw.Stop();
        UnityEngine.Debug.Log($"Time to encode to PNG: {sw.ElapsedMilliseconds} ms");

        UnityEngine.Debug.Log("Screenshot saved to: " + path);
    }

    private void AddBoundingBoxForImage(string imageName){
        Stopwatch sw = new Stopwatch();
        sw.Start(); // track raycast


        string boundingBoxPath = datasetDirectory + "bounding_boxes.csv";

        // Create a stringbuilder to iteratively add to, for each image taken
        StringBuilder newText = new StringBuilder();

        // If the bounding box file does not exist, create it and add the information row
        if(!File.Exists(boundingBoxPath)){
            using (FileStream fs = File.Create(boundingBoxPath))
            {
                newText.Append("image_name,BoxesString,domain\n");
            }
        }

        // Create a dictionary to record the bounding boxes for each object that is hit by the raycast
        //Key: obj, Value: [x_min, y_min, x_max, y_max]
        Dictionary<GameObject, List<int>> objectBoundingBoxDict = new Dictionary<GameObject, List<int>>();
        
        // Get raycast hits
        NativeArray<RaycastHit> raycastResults = RaycastFromCamera();

        // Simple method to get a new bounding box coordinate list based on new coordinates where the object is hit by raycasts
        List<int> UpdateBoundingBoxCoordinates(List<int> coordinates, int x, int y){
            // values: [x_min, y_min, x_max, y_max]
            List<int> newValues = new List<int>(coordinates);
            UnityEngine.Debug.Log(coordinates.Count);
            UnityEngine.Debug.Log(newValues.Count);
            if (coordinates[0] > x){
                newValues[0] = x;
            }
            if (coordinates[1] > y){
                newValues[1] = y;
            }
            if (coordinates[2] < x){
                newValues[2] = x;
            }
            if (coordinates[3] < y){
                newValues[3] = y;
            }
            return newValues;
        }

        // Find bounding boxes using raycast results, saving them to objectBoundingBoxDict
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                RaycastHit hit = raycastResults[x + width*y];
                GameObject obj = hit.transform ? hit.transform.gameObject : null;
                if (obj != null && Wheat.IsWheat(obj, Wheat.Part.Head)){
                    if (objectBoundingBoxDict.ContainsKey(obj)){
                        List<int> coordinates = objectBoundingBoxDict[obj];
                        objectBoundingBoxDict[obj] = UpdateBoundingBoxCoordinates(coordinates, x, y);
                    }
                    else {
                        List<int> coordinates = new List<int>{x, y, x, y};
                        objectBoundingBoxDict[obj] = coordinates;
                    }
                }
            }
        }
        raycastResults.Dispose();
    
        // Add this image to the stringbuilder, with its bounding box results.
        //  Image name
        newText.Append($"{imageName},");
        //  Bounding box coordinates
        if (objectBoundingBoxDict.Values.Count != 0){
            foreach (List<int> values in objectBoundingBoxDict.Values){
                newText.Append($"{values[0]} {values[1]} {values[2]} {values[3]};");
            }
            newText.Length--; // Clear the semicolon in the last position
        }
        else {
            newText.Append("no_box");
        }
        //  Domain name
        newText.Append($",{domainName}\n");

        // Write the stringbuilder to the CSV
        File.AppendAllText(boundingBoxPath, newText.ToString());

        sw.Stop();
        UnityEngine.Debug.Log($"Time to append bounding box: {sw.ElapsedMilliseconds} ms");
    }

    // Takes the annotation json object script and resets it with updated caegories
    private void InitializeAnnotationWithCategories(){
        annotationJSON = new AnnotationJSON();

        // Add the categories from the parts list
        foreach (Wheat.Part part in Wheat.partToIDDict.Keys){
            int id = Wheat.partToIDDict[part];
            string name = Wheat.partToNameDict[part];
            annotationJSON.categories.Add(new Category(id, name));
        }
    }

    private void AddImageToAnnotation(int id, string file_name, string date_captured){
        annotationJSON.images.Add(new Image(id, width, height, file_name, date_captured));
    }

    private void AddLabelToAnnotation(int image_id, int category_id, List<int[]> pixels){
        annotationJSON.annotations.Add(new Annotation(image_id, category_id, pixels));
    }

    private void CreateInstanceLabels(int image_id){
        Stopwatch sw = new Stopwatch();
        sw.Start(); // track raycast

        // Get raycast hits
        NativeArray<RaycastHit> raycastResults = RaycastFromCamera();

        // Get hit objects, and turn it into a dict mapping Object to the pixels it was hit at
        Dictionary<GameObject, List<int[]>> hitPositionsForObject = new Dictionary<GameObject, List<int[]>>();
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                RaycastHit hit = raycastResults[x + y*width];
                GameObject obj = hit.transform ? hit.transform.gameObject : null;
                
                // For each hit position, get the object, and connect the position to the object via a dictionary.
                if(obj != null){
                    if (!hitPositionsForObject.ContainsKey(obj)){
                        hitPositionsForObject[obj] = new List<int[]>();
                    }
                    hitPositionsForObject[obj].Add(new int[]{x,y});
                }
            }
        }
        raycastResults.Dispose();

        // Loop through each unique instance
        foreach (GameObject obj in hitPositionsForObject.Keys){
            WheatData wheatData = obj.GetComponent<WheatData>();
            if (wheatData != null){
                List<int[]> hitPixels = hitPositionsForObject[obj];
                Wheat.Part part = wheatData.part;
                int category_id = Wheat.partToIDDict[part];
                
                // Place each ID at each corresponding position in the array
                AddLabelToAnnotation(image_id, category_id, hitPixels);
            }
        };
        
        sw.Stop();
        UnityEngine.Debug.Log($"Time to update JSON: {sw.ElapsedMilliseconds} ms");
    }

    private void UpdateJSONFile(){
        string stringJSON = JsonUtility.ToJson(annotationJSON, true);
        File.WriteAllTextAsync(annotationJSONPath, stringJSON);
    }
}
