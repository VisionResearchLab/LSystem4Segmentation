using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using Unity.Profiling;
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
    private string screenshotName = "s-test.png";
    private string annotatedScreenshotName = "a-test.png";


    // Define scripts with functions that need to be called
    private void Start(){
        annotateCameraScript = annotateCam.GetComponent<AnnotateCamera>();
        ShowUI();
    }

    // Take two screenshots: s-name, representing the normal screenshot, and a-name, representing the annotated screenshot
    public void TakeScreenShot(){
        HideUI();
        StartCoroutine(ScreenshotEnum(screenshotName, 1, true, true));
        StartCoroutine(ScreenshotEnum(annotatedScreenshotName, 2, true, false));
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
    private IEnumerator ScreenshotEnum(string name, int delay, bool swapCamerasAfter, bool hideUIAfter){
        // Screenshots must happen at the end of a frame. To make the annotated screenshot display the view of another camera,
        // it is necessary to wait a frame, during which the camera is switched.
        
        for (int i = 0; i < delay; i++){
            yield return new WaitForEndOfFrame();
        }
        
        string path = getPath(name);

        // https://stackoverflow.com/a/36188311
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height);
        screenShot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
        screenShot.Apply();
        byte[] bytes = screenShot.EncodeToPNG();
        Object.Destroy(screenShot);
        Debug.Log(path);
        File.WriteAllBytes(path, bytes);

        if (swapCamerasAfter){
            SwapCameras();
        }

        if (hideUIAfter){
            HideUI();
        } else {
            ShowUI();
        }
    }
}
