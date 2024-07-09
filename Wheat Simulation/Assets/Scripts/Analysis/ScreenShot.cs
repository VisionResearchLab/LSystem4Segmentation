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

public class ScreenShot : MonoBehaviour
{
    // Cameras
    public Camera mainCam;


    // UI canvas group (to hide it for screenshots)
    public CanvasGroup canvasGroup;


    // Save directory
    // private string saveDirectory = "/home/student/elijahmickelson/Datasets/1024x1024-0/"; //lab
    private string saveDirectory = "C:/Users/xSkul/OneDrive/Documents/Projects/Wheat/wheat/Datasets/1024x1024-3/"; //laptop
    // private string saveDirectory = "K:/Users/Skull/Downloads/Datasets/1024x1024-4/"; //home


    // Determine if the annotation should be white or r/g/b
    // TODO: reimplement this
    public static bool annotationIsColored = false;

    // Variables for label generation
    public Shader labelShader;
    public LayerMask labelLayer;
    private CustomPassVolume customPassVolume;
    private CustomPass customPass;

    // Setting to decide whether awns should be labelled as wheat heads or passed through
    [SerializeField] private bool labelAwnsAsWheatHeads;

    // Define scripts with functions that need to be called
    private void Start(){
        ShowUI();
    }

    // Call ScreenshotSequenceEnum with default time to wait of 0.1 seconds
    public void TakeScreenShot(){
        float secondsDelay = 0.1f;
        StartCoroutine(ScreenshotSequenceEnum(secondsDelay));
    }

    public IEnumerator ScreenshotSequenceEnum(float secondsDelay){
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        string imagePath = getPath(dateTime + "_image" + ".png");
        string labelPath = getPath(dateTime + "_label" + ".png");

        HideUI();
        yield return new WaitForSeconds(secondsDelay);
        StartCoroutine(ImageScreenshot(imagePath));
        AnnotateScreenshotRaycast(labelPath);
        yield return new WaitForEndOfFrame();
        ShowUI();
    }

    // Returns a unique filepath in the screenshots folder based on the given name
    private string getPath(string name){
        return AssetDatabase.GenerateUniqueAssetPath(saveDirectory + name);
    }

    // Call SwapCameras() in annotateCameraScript
    private void SwapCameras(){
        Wheat.ToggleAnnotation();
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
        UnityEngine.Debug.Log(path);
        File.WriteAllBytes(path, bytes);
    }

    private void AnnotateScreenshotRaycast(string path){
        int width = Screen.width;
        int height = Screen.height;

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
                raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction, layerMask:Wheat.awnsLayerMask);
                index++;
            }
        }

        // Schedule the batch of raycasts
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 32);

        // Complete the job handle
        handle.Complete();

        sw.Stop();
        float timeToRaycast = sw.ElapsedMilliseconds;
        sw.Reset();
        sw.Start(); // track time to get raycast results

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
        raycastCommands.Dispose();
        raycastResults.Dispose();
        UnityEngine.Object.Destroy(screenShot);

        sw.Stop();
        float timeToEncode = sw.ElapsedMilliseconds;

        UnityEngine.Debug.Log($"Time to raycast: {timeToRaycast} ms\nTime to encode to PNG: {timeToEncode} ms");

        UnityEngine.Debug.Log("Screenshot saved to: " + path);
    }


    // private IEnumerator AnnotateScreenshotShaderEnum(string name, int frameDelay, bool hideUIAfter){
    //     for (int i = 0; i < frameDelay; i++){
    //         yield return new WaitForEndOfFrame();
    //     }

    //     string path = getPath(name);

    //     // Create a temporary camera
    //     Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
    //     tempCamera.CopyFrom(mainCam);
    //     tempCamera.clearFlags = CameraClearFlags.SolidColor;
    //     tempCamera.backgroundColor = Color.black;
    //     tempCamera.cullingMask = labelLayer;

    //     // Render to a RenderTexture
    //     RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
    //     tempCamera.targetTexture = rt;

    //     // Create a custom pass volume
    //     CustomPassVolume customPassVolume = tempCamera.gameObject.AddComponent<CustomPassVolume>();
    //     customPassVolume.isGlobal = true;
    //     customPassVolume.injectionPoint = CustomPassInjectionPoint.AfterOpaqueDepthAndNormal;

    //     // Create and configure the custom pass
    //     LabelCustomPass labelPass = new LabelCustomPass {
    //         name = "LabelPass",
    //         material = labelMaterial,
    //         labelLayer = labelLayer
    //     };

    //     customPassVolume.customPasses.Add(labelPass);

    //     // Render the label image
    //     tempCamera.Render();

    //     // Save the RenderTexture to a Texture2D
    //     RenderTexture.active = rt;
    //     Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
    //     tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
    //     tex.Apply();

    //     // Save the texture as a PNG (optional)
    //     byte[] bytes = tex.EncodeToPNG();
    //     System.IO.File.WriteAllBytes(Application.dataPath + "/../LabelImage.png", bytes);

    //     // Cleanup
    //     RenderTexture.active = null;
    //     tempCamera.targetTexture = null;
    //     Destroy(rt);
    //     Destroy(tempCamera.gameObject);

    //     Debug.Log("Screenshot saved to: " + path);

    //     if (hideUIAfter){
    //         HideUI();
    //     } else {
    //         ShowUI();
    //     }
    // }
}
