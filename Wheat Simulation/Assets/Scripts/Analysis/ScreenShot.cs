using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
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
        StartCoroutine(AnnotateScreenshotEnum(annotatedScreenshotName, 2, false));
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
        // Screenshots must happen at the end of a frame. To make the annotated screenshot display the view of another camera,
        // it is necessary to wait a frame, during which the camera is switched.

        for (int i = 0; i < frameDelay; i++){
            yield return new WaitForEndOfFrame();
        }
        
        string path = getPath(name);

        // https://stackoverflow.com/a/36188311
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height);
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
        // Screenshots must happen at the end of a frame. To make the annotated screenshot display the view of another camera,
        // it is necessary to wait a frame, during which the camera is switched.

        for (int i = 0; i < frameDelay; i++){
            yield return new WaitForEndOfFrame();
        }
        
        string path = getPath(name);


        // https://stackoverflow.com/a/36188311
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height);

        // Raycast to every pixel, and recolor it based on what wheat part it hits (if any)
        for (int x = 0; x < Screen.width; x++){
            for (int y = 0; y < Screen.height; y++){
                Ray ray = mainCam.ScreenPointToRay(new Vector3(x, y));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject obj = hit.transform.gameObject;
                    if (Wheat.IsWheat(obj)){
                        screenShot.SetPixel(x, y, obj.GetComponent<WheatData>().color);
                    }
                    else { // Hits an untagged object, usually ground
                        screenShot.SetPixel(x, y, Color.black);
                    }
                }
            }
        }

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

}
