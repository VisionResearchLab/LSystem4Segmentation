using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class ScreenShot : MonoBehaviour
{
    // Cameras
    public Camera mainCam;


    // UI canvas group (to hide it for screenshots)
    public CanvasGroup canvasGroup;


    // Save directory
    DirectoryManager dm;
    private string datasetsDirectory => dm.currentDatasetDirectory;
    private string datasetName => dm.currentDatasetName;
    private string domainName => scheduleInterpreter.currentDomain.name;
    [HideInInspector] public string datasetDirectory => $"{datasetsDirectory}/{datasetName}/";
    private string domainDirectory => $"{datasetsDirectory}/{datasetName}/{domainName}/";
    private string datasetJSONPath => $"{datasetsDirectory}/{datasetName}/{domainName}/annotations.json";

    // Setting to decide whether awns should be labelled as wheat heads or passed through
    [SerializeField] private bool labelAwnsAsWheatHeads;

    // Screen width and height
    private int width => Screen.width;
    private int height => Screen.height;

    // Get the InstanceLabels object which will overwrite the JSON after each image
    private DatasetJSON datasetJSON;
    [SerializeField] private ScheduleInterpreter scheduleInterpreter;


    // Define scripts with functions that need to be called
    private void Start(){
        dm = FindObjectOfType<DirectoryManager>();
        // ShowUI();
        HideUI();
    }

    public bool DirectoryIsValid(){
        // Check if the dataset and domain fields are filled in
        if (domainName == null || domainName == "" ||  datasetDirectory == null || datasetDirectory == "" ||  domainDirectory == null  || domainDirectory == "")
            {
                UnityEngine.Debug.LogError("The dataset and domain directories must be declared before generating a dataset.");
                return false;
            }

        // Check if the DATASETS directory exists. If it does, proceed.
        if (Directory.Exists(datasetsDirectory)){
            // If the DOMAIN directory does not exist based on the private name, create it.
            // Also create a new JSON.
            if (!Directory.Exists(domainDirectory)){
                Directory.CreateDirectory(domainDirectory);
                InitializeDatasetJSONWithCategories();
                return true;
            } 
            // Try to load instanceLabels from the JSON if the domain was found
            else {
                if (File.Exists(datasetJSONPath)){
                    string jsonContent = File.ReadAllText(datasetJSONPath);
                    datasetJSON = JsonConvert.DeserializeObject<DatasetJSON>(jsonContent);
                    return true;
                } else {
                    InitializeDatasetJSONWithCategories();
                    return true;
                }
            }
        }
        else {
            UnityEngine.Debug.LogError("The datasets directory could not be found.");
            return false;
        }
    }

    // Call ScreenshotSequenceEnum with default time to wait of 0.1 seconds
    public void TakeScreenShot(){
        float secondsDelay = 0.1f;
        StartCoroutine(ScreenshotSequenceEnum(secondsDelay));
    }

    public IEnumerator ScreenshotSequenceEnum(float secondsDelay){
        if(DirectoryIsValid()){
            DateTime dateTimeNow = DateTime.Now;
            string dateTime = dateTimeNow.ToString("yyyy-MM-dd_HH-mm-ss");
            string dateTimeJSONFormatting = dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss");

            string imageName = dateTime + "_image" + ".png";
            string imagePath = GetUniqueAssetPathInDomain(imageName);

            string labelName = dateTime + "_label" + ".png";
            string labelPath = GetUniqueAssetPathInDomain(labelName);

            HideUI();
            yield return new WaitForSeconds(secondsDelay);

            // Create the image, assign it an ID, then add it to the JSON
            StartCoroutine(ImageScreenshot(imagePath));
            int imageID = datasetJSON.images.Count;
            AddImageToDataset(imageID, imageName, dateTimeJSONFormatting);

            // Use raycasts to create a BMP of annotation IDs for the current image, then add them to the JSON
            Annotate(imageID, labelName, labelPath);

            // Update the JSON so prior changes are included
            UpdateJSONFile();

            yield return new WaitForEndOfFrame();
            // ShowUI();
        } else {
            UnityEngine.Debug.LogError("One or more directories are invalid.");
        }
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
                    raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction, new QueryParameters());
                } else {
                    raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction, new QueryParameters(layerMask:Wheat.awnsLayerMask));
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

    // private void AddBoundingBoxForImage(string imageName){
    //     Stopwatch sw = new Stopwatch();
    //     sw.Start(); // track raycast


    //     string boundingBoxPath = datasetDirectory + "bounding_boxes.csv";

    //     // Create a stringbuilder to iteratively add to, for each image taken
    //     StringBuilder newText = new StringBuilder();

    //     // If the bounding box file does not exist, create it and add the information row
    //     if(!File.Exists(boundingBoxPath)){
    //         using (FileStream fs = File.Create(boundingBoxPath))
    //         {
    //             newText.Append("image_name,BoxesString,domain\n");
    //         }
    //     }

    //     // Create a dictionary to record the bounding boxes for each object that is hit by the raycast
    //     //Key: obj, Value: [x_min, y_min, x_max, y_max]
    //     Dictionary<GameObject, List<int>> objectBoundingBoxDict = new Dictionary<GameObject, List<int>>();
        
    //     // Get raycast hits
    //     NativeArray<RaycastHit> raycastResults = RaycastFromCamera();

    //     // Simple method to get a new bounding box coordinate list based on new coordinates where the object is hit by raycasts
    //     List<int> UpdateBoundingBoxCoordinates(List<int> coordinates, int x, int y){
    //         // values: [x_min, y_min, x_max, y_max]
    //         List<int> newValues = new List<int>(coordinates);
    //         UnityEngine.Debug.Log(coordinates.Count);
    //         UnityEngine.Debug.Log(newValues.Count);
    //         if (coordinates[0] > x){
    //             newValues[0] = x;
    //         }
    //         if (coordinates[1] > y){
    //             newValues[1] = y;
    //         }
    //         if (coordinates[2] < x){
    //             newValues[2] = x;
    //         }
    //         if (coordinates[3] < y){
    //             newValues[3] = y;
    //         }
    //         return newValues;
    //     }

    //     // Find bounding boxes using raycast results, saving them to objectBoundingBoxDict
    //     for (int y = 0; y < height; y++){
    //         for (int x = 0; x < width; x++){
    //             RaycastHit hit = raycastResults[x + width*y];
    //             GameObject obj = hit.transform ? hit.transform.gameObject : null;
    //             if (obj != null && Wheat.IsWheat(obj, Wheat.Part.Head)){
    //                 if (objectBoundingBoxDict.ContainsKey(obj)){
    //                     List<int> coordinates = objectBoundingBoxDict[obj];
    //                     objectBoundingBoxDict[obj] = UpdateBoundingBoxCoordinates(coordinates, x, y);
    //                 }
    //                 else {
    //                     List<int> coordinates = new List<int>{x, y, x, y};
    //                     objectBoundingBoxDict[obj] = coordinates;
    //                 }
    //             }
    //         }
    //     }
    //     raycastResults.Dispose();
    
    //     // Add this image to the stringbuilder, with its bounding box results.
    //     //  Image name
    //     newText.Append($"{imageName},");
    //     //  Bounding box coordinates
    //     if (objectBoundingBoxDict.Values.Count != 0){
    //         foreach (List<int> values in objectBoundingBoxDict.Values){
    //             newText.Append($"{values[0]} {values[1]} {values[2]} {values[3]};");
    //         }
    //         newText.Length--; // Clear the semicolon in the last position
    //     }
    //     else {
    //         newText.Append("no_box");
    //     }
    //     //  Domain name
    //     newText.Append($",{domainName}\n");

    //     // Write the stringbuilder to the CSV
    //     File.AppendAllText(boundingBoxPath, newText.ToString());

    //     sw.Stop();
    //     UnityEngine.Debug.Log($"Time to append bounding box: {sw.ElapsedMilliseconds} ms");
    // }

    // Takes the annotation json object script and resets it with updated caegories
    private void InitializeDatasetJSONWithCategories(){
        datasetJSON = new DatasetJSON();

        // Add ground category
        datasetJSON.categories.Add(new Category(0, "Ground"));

        // Add the categories from the parts list
        foreach (Wheat.Part part in Wheat.partToIDDict.Keys){
            int id = Wheat.partToIDDict[part];
            string name = Wheat.partToNameDict[part];
            datasetJSON.categories.Add(new Category(id, name));
        }
    }

    private void AddImageToDataset(int id, string file_name, string date_captured){
        datasetJSON.images.Add(new Image(id, width, height, file_name, date_captured));
    }

    private void Annotate(int image_id, string labelName, string labelPath){
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

        // Create an int array for the bitmap
        int[] annotationMap1DArray = new int[width*height];

        // Loop through each unique instance
        foreach (GameObject obj in hitPositionsForObject.Keys){
            WheatData wheatData = obj.GetComponent<WheatData>();
            if (wheatData != null){
                // Get a list of pixels hit, then convert it to a list of polygons
                List<int[]> hitPixels = hitPositionsForObject[obj];

                // Get category
                Wheat.Part part = wheatData.part;
                int category_id = Wheat.partToIDDict[part];
                
                // Get ID number
                int id = datasetJSON.annotations.Count + 1;

                // Add this object to the annotation
                AddAnnotationToDataset(id, image_id, category_id, hitPixels);

                // Update the annotation map at each pixel with the annotation ID
                foreach (int[] hitPixel in hitPixels){
                    annotationMap1DArray[hitPixel[0] + width * hitPixel[1]] = id;
                }
            }
        };
        
        // SavePixelsToBMP(annotationMap1DArray, labelPath);
        SaveIntArrayAsPNG(annotationMap1DArray, width, height, labelPath);
        AddAnnotationMapToDataset(image_id, labelName);

        sw.Stop();
        UnityEngine.Debug.Log($"Time to update JSON and create bitmap: {sw.ElapsedMilliseconds} ms");
    }

    private void AddAnnotationToDataset(int annotation_id, int image_id, int category_id, List<int[]> pixels){
        // error check
        if (annotation_id == 0){
            UnityEngine.Debug.LogError("Should not annotate ground!");
        }    
    
        // get area
        int area = pixels.Count();
        
        // get bounding box
        HashSet<int> xValues = new HashSet<int>();
        HashSet<int> yValues = new HashSet<int>();
        foreach (int[] pixel in pixels){
            xValues.Add(pixel[0]);
            yValues.Add(pixel[1]);
        }
        int[] bbox = new int[]{xValues.Min(), yValues.Min(), xValues.Max(), yValues.Max()};

        datasetJSON.annotations.Add(new Annotation(annotation_id, image_id, category_id, area, bbox));
    }

    private void AddAnnotationMapToDataset(int image_id, string file_name){
        datasetJSON.annotationMaps.Add(new AnnotationMap(image_id, width, height, file_name));
    }

    private void UpdateJSONFile(){
        string stringJSON = JsonConvert.SerializeObject(datasetJSON, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllTextAsync(datasetJSONPath, stringJSON);
    }


    private byte[] EncodeToBMP(int[] pixels)
    {
        int fileSize = 54 + (4 * width * height); // 54-byte header + pixel data

        byte[] bmpBytes = new byte[fileSize];

        // BMP Header
        bmpBytes[0] = (byte)'B';
        bmpBytes[1] = (byte)'M';
        BitConverter.GetBytes(fileSize).CopyTo(bmpBytes, 2);
        bmpBytes[10] = 54; // Pixel data offset

        // DIB Header
        bmpBytes[14] = 40; // DIB header size
        BitConverter.GetBytes(width).CopyTo(bmpBytes, 18);
        BitConverter.GetBytes(height).CopyTo(bmpBytes, 22);
        bmpBytes[26] = 1; // Number of color planes
        bmpBytes[28] = 32; // Bits per pixel

        // Pixel Data
        int offset = 54;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixel = pixels[(y * width) + x];
                bmpBytes[offset++] = (byte)(pixel & 0xFF);         // Low byte
                bmpBytes[offset++] = (byte)((pixel >> 8) & 0xFF);  // Second byte
                bmpBytes[offset++] = (byte)((pixel >> 16) & 0xFF); // Third byte
                bmpBytes[offset++] = (byte)((pixel >> 24) & 0xFF); // High byte
            }
        }

        return bmpBytes;
    }

    // Input: 32 bit array of integers. Bottom left to bottom right, then up. Needs full path.
    // Consider using Convert.ToUInt32(x)
    public void SavePixelsToBMP(int[] pixels, string path)
    {
        byte[] bmpBytes = EncodeToBMP(pixels);
        File.WriteAllBytes(path, bmpBytes);
    }

    public void SaveIntArrayAsPNG(int[] pixelData, int width, int height, string filePath)
    {
        // Check if the pixelData length matches the width and height
        if (pixelData.Length != width * height)
        {
            UnityEngine.Debug.LogError("Pixel data length does not match the dimensions.");
            return;
        }

        // Create a Texture2D with the specified dimensions and format
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Convert the int[] to Color[] for the texture
        Color[] colors = new Color[width * height];
        for (int i = 0; i < pixelData.Length; i++)
        {
            // Extract RGBA components from the integer
            int value = pixelData[i];
            float r = ((value >> 24) & 0xFF) / 255f;
            float g = ((value >> 16) & 0xFF) / 255f;
            float b = ((value >> 8) & 0xFF) / 255f;
            float a = (value & 0xFF) / 255f;

            colors[i] = new Color(r, g, b, a);
        }

        // Set the pixels of the texture
        texture.SetPixels(colors);
        texture.Apply();

        // Encode texture to PNG
        byte[] pngData = texture.EncodeToPNG();

        // Save the PNG file
        File.WriteAllBytes(filePath, pngData);

        UnityEngine.Debug.Log("PNG saved to " + filePath);
    }
}
