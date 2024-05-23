using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ScanScene : MonoBehaviour
{
    private static Camera mainCamera;

    // Number of wheat heads, leaves, and stems in last screenshot
    private static HashSet<GameObject> headsScanned = new HashSet<GameObject>();
    private static HashSet<GameObject> leavesScanned = new HashSet<GameObject>();
    private static HashSet<GameObject> stemsScanned = new HashSet<GameObject>();

    // Number of times rays are cast
    private static int rayCounter = 0;

    // Increases the amount of rays to cast when searching for wheat on screen. Time to calculate increases with the square of this value.
    private static int resolution = 150;

    private void Start()
    {
        mainCamera = this.gameObject.GetComponent<Camera>();
    }

    //TODO: This should return the number of wheat plants, not individual parts
    private static int getWheatCount(){
        int count = 0;
        foreach (GameObject obj in FindObjectsOfType<GameObject>()){
            if (Wheat.IsWheat(obj)){
                count++;
            }
        }
        return count;
    }

    // Returns the sum of how many objects of a specific type exist
    private static int getWheatPartCount(Wheat.Part part){
        int count = 0;
        foreach (GameObject obj in FindObjectsOfType<GameObject>()){
            if (Wheat.IsWheat(obj, part)){
                count++;
            }
        }
        return count;
    }

    private static void ResetVariables(){
        headsScanned.Clear();
        leavesScanned.Clear();
        stemsScanned.Clear();
        rayCounter = 0;
    }

    public void ScanWheat()
    {
        ResetVariables();
        
        int xIncrement = Screen.width/resolution;
        int yIncrement = Screen.height/resolution;
        for (int x = 0; x < Screen.width; x += xIncrement){
            for (int y = 0; y < Screen.height; y += yIncrement){
                rayCounter++;
                Ray ray = mainCamera.ScreenPointToRay(new Vector3(x, y));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject obj = hit.transform.gameObject;
                    name = obj.name;
                    if (Wheat.IsWheat(obj, Wheat.Part.Head)){
                        headsScanned.Add(obj);
                    } else if (Wheat.IsWheat(obj, Wheat.Part.Leaf)){
                        leavesScanned.Add(obj);
                    } else if (Wheat.IsWheat(obj, Wheat.Part.Stem)){
                        stemsScanned.Add(obj);
                    }
                }
            }
        }

        float headsFraction = Mathf.Round(100 * headsScanned.Count / getWheatPartCount(Wheat.Part.Head));
        float leavesFraction = Mathf.Round(100 * leavesScanned.Count / getWheatPartCount(Wheat.Part.Leaf));
        float stemsFraction = Mathf.Round(100 * stemsScanned.Count / getWheatPartCount(Wheat.Part.Stem));
        Debug.Log(String.Format("Scan results: {0} Rays, {1} Wheat Heads ({2}%), {3} Leaves ({4}%), {5} Stems ({6}%)", 
            rayCounter, headsScanned.Count, headsFraction, leavesScanned.Count, leavesFraction, stemsScanned.Count, stemsFraction));
    }
}
