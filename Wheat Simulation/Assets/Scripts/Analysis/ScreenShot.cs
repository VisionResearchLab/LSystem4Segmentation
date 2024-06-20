using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
public class ScreenShot : MonoBehaviour
{
    // Cameras
    public Camera mainCam;
    public Camera annotateCam;
    private AnnotateCamera annotateCameraScript;


    // UI canvas group (to hide it for screenshots)
    public CanvasGroup canvasGroup;


    // Save directory
    private string saveDirectory = "Screenshots";


    // Determine if the annotation should be white or r/g/b
    public static bool annotationIsColored = false;

    // Define scripts with functions that need to be called
    private void Start(){
        annotateCameraScript = annotateCam.GetComponent<AnnotateCamera>();
        ShowUI();
    }

    // Take two screenshots: s-name, representing the normal screenshot, and a-name, representing the annotated screenshot
    public void TakeScreenShot(){
        HideUI();
        string timeInSeconds = Time.time.ToString();
        string screenshotName = "image_" + timeInSeconds + ".png";
        string annotatedScreenshotName = "annotation_" + timeInSeconds + ".png";
        StartCoroutine(ScreenshotEnum(screenshotName, 1, true));
        StartCoroutine(AnnotateScreenshotEnum(annotatedScreenshotName, 1, false));
    }

    // Returns a unique filepath in the screenshots folder based on the given name
    private string getPath(string name){
        return AssetDatabase.GenerateUniqueAssetPath("Assets/" + saveDirectory + "/" + name);
    }

    // Call SwapCameras() in annotateCameraScript
    private void SwapCameras(){
        Wheat.ToggleAnnotation();
        annotateCameraScript.SwapCameras();
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
    private IEnumerator ScreenshotEnum(string name, int frameDelay, bool hideUIAfter){
        // Screenshots must happen at the end of a frame
        for (int i = 0; i < frameDelay; i++){
            yield return new WaitForEndOfFrame();
        }
        
        string path = getPath(name);

        // https://stackoverflow.com/a/36188311
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true); // mipchain??
        screenShot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
        screenShot.Apply();
        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);
        Debug.Log(path);
        File.WriteAllBytes(path, bytes);

        if (hideUIAfter){
            HideUI();
        } else {
            ShowUI();
        }
    }

    private IEnumerator AnnotateScreenshotEnum(string name, int frameDelay, bool hideUIAfter){
        // Do this after one frame
        for (int i = 0; i < frameDelay; i++){
            yield return new WaitForEndOfFrame();
        }
        
        string path = getPath(name);

        // // https://stackoverflow.com/a/36188311
        // Texture2D screenShot = new Texture2D(Screen.width, Screen.height);

        // // Raycast to every pixel, and recolor it based on what wheat part it hits (if any)
        // for (int x = 0; x < Screen.width; x++){
        //     for (int y = 0; y < Screen.height; y++){
        //         Ray ray = mainCam.ScreenPointToRay(new Vector3(x, y));
        //         RaycastHit hit;
        //         if (Physics.Raycast(ray, out hit))
        //         {
        //             GameObject obj = hit.transform.gameObject;
        //             if (Wheat.IsWheat(obj)){
        //                 screenShot.SetPixel(x, y, obj.GetComponent<WheatData>().color);
        //             }
        //             else { // Hits an untagged object, usually ground
        //                 screenShot.SetPixel(x, y, Color.black);
        //             }
        //         }
        //     }
        // }

        // screenShot.Apply();
        // byte[] bytes = screenShot.EncodeToPNG();
        // UnityEngine.Object.Destroy(screenShot);
        // Debug.Log(path);
        // File.WriteAllBytes(path, bytes);

        int width = Screen.width;
        int height = Screen.height;

        // Prepare for raycast commands and results
        NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(width * height, Allocator.TempJob);
        NativeArray<RaycastHit> raycastResults = new NativeArray<RaycastHit>(width * height, Allocator.TempJob);

        int index = 0;

        // Create raycast commands for each pixel
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 screenPoint = new Vector3(x, y, 0);
                Ray ray = mainCam.ScreenPointToRay(screenPoint);
                raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction);
                index++;
            }
        }

        // Schedule the batch of raycasts
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 32);

        // Complete the job handle
        handle.Complete();

        // Create the texture and set pixels based on raycast results
        Texture2D screenShot = new Texture2D(width, height);

        index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RaycastHit hit = raycastResults[index];
                GameObject obj = hit.transform ? hit.transform.gameObject : null;

                if (obj != null && Wheat.IsWheat(obj))
                {
                    WheatData wheatData = obj.GetComponent<WheatData>();
                    screenShot.SetPixel(x, y, wheatData != null ? wheatData.color : Color.white);
                }
                else
                {
                    screenShot.SetPixel(x, y, Color.black);
                }

                index++;
            }
        }

        screenShot.Apply();
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Clean up
        raycastCommands.Dispose();
        raycastResults.Dispose();
        UnityEngine.Object.Destroy(screenShot);

        Debug.Log("Screenshot saved to: " + path);

        if (hideUIAfter){
            HideUI();
        } else {
            ShowUI();
        }
    }

}
