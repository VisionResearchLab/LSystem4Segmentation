using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Diagnostics;
using System.Text;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.IO.Compression;

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
    [HideInInspector] public string datasetDirectory;
    [HideInInspector] public string domainDirectory;

    // Setting to decide whether awns should be labelled as wheat heads or passed through
    [SerializeField] private bool labelAwnsAsWheatHeads;

    // Screen width and height
    private int width;
    private int height;

    // Get the InstanceLabels object which will overwrite the JSON after each image
    private InstanceLabels instanceLabels = new InstanceLabels();

    // Define scripts with functions that need to be called
    private void Start(){
        ShowUI();
        datasetDirectory = $"{datasetsDirectory}/{datasetName}/";
        domainDirectory = $"{datasetsDirectory}/{datasetName}/{domainName}/";
        width = Screen.width;
        height = Screen.height;
        if (Directory.Exists(datasetsDirectory)){
            if (!Directory.Exists(domainDirectory)){
                Directory.CreateDirectory(domainDirectory);
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
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        string imageName = dateTime + "_image" + ".png";
        string imagePath = getPath(imageName);

        string labelName = dateTime + "_label" + ".png";
        string labelPath = getPath(labelName);

        HideUI();
        yield return new WaitForSeconds(secondsDelay);
        StartCoroutine(ImageScreenshot(imagePath));
        // AnnotateScreenshotRaycast(labelPath);
        // AddBoundingBoxForImage(imageName);
        // TestBMPExport(getPath($"{dateTime}_label.bmp"), imageName);
        InstanceSegmentLabel();
        yield return new WaitForEndOfFrame();
        ShowUI();
    }

    // Returns a unique filepath in the screenshots folder based on the given name
    private string getPath(string name){
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
        screenShot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
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


    private void InstanceSegmentLabel(){
        Stopwatch sw = new Stopwatch();
        sw.Start(); // track raycast

        // Get raycast hits
        NativeArray<RaycastHit> raycastResults = RaycastFromCamera();

        // Get hit objects, and turn it into a dict mapping Object to the pixels it was hit at
        Dictionary<GameObject, HashSet<int>> objectToPixels = new Dictionary<GameObject, HashSet<int>>();
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                RaycastHit hit = raycastResults[x + y*width];
                GameObject obj = hit.transform ? hit.transform.gameObject : null;
                if (obj != null && !objectToPixels.ContainsKey(obj)){
                    HashSet<int> positions = new HashSet<int>();
                    objectToPixels[obj] = positions;
                    positions.Add(x + y*width);
                } else if (obj != null){
                    HashSet<int> positions = objectToPixels[obj];
                    positions.Add(x + y*width);
                }
            }
        }
        raycastResults.Dispose();

        int index = 1;

        // Initialize black color array of screen size
        Color[] pixels = new Color[width*height];
        for (int i = 0; i < width*height; i++){
            pixels[i] = Color.black;
        }

        foreach (GameObject obj in objectToPixels.Keys){
            int id = index;
            index ++;

            WheatData wheatData = obj.GetComponent<WheatData>();
            if (wheatData){
                Wheat.Part part = wheatData.part;
                
                int r = Wheat.partToFirstChannelValueDict[part];
                int g = index % 255;
                int b = index / 255;

                // Place each ID at each corresponding position in the array
                foreach (int coordinate in objectToPixels[obj]){
                    pixels[coordinate] = new Color(r/255f,g/255f,b/255f);
                }
            }
        };
        EncodeToBMP(pixels);
        byte[] bmpBytes = EncodeToBMP(pixels);
        File.WriteAllBytes(domainDirectory + "/test.bmp", bmpBytes);

        sw.Stop();
        UnityEngine.Debug.Log($"Time to save instance segmented label: {sw.ElapsedMilliseconds} ms");
    }



    private byte[] EncodeToBMP(Color[] pixels)
    {
        int fileSize = 54 + (3 * width * height); // 54-byte header + pixel data

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
        bmpBytes[28] = 24; // Bits per pixel

        // Pixel Data
        int offset = 54;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixel = pixels[(y * width) + x];
                bmpBytes[offset++] = pixel.b;
                bmpBytes[offset++] = pixel.g;
                bmpBytes[offset++] = pixel.r;
            }
        }

        return bmpBytes;
    }

    public void SaveTextureToBMP(Texture2D texture, string path)
    {
        // Color32[] pixels = texture.GetPixels32();
        Color[] pixels = texture.GetPixels();
        byte[] bmpBytes = EncodeToBMP(pixels);
        File.WriteAllBytes(path, bmpBytes);
    }
}
