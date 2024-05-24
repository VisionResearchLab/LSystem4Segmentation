using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Wheat : MonoBehaviour
{
    // All parts of a wheat object are considered to have the Wheat type, as well as their own (i.e. Head, Stem, or Leaf) type.
    public enum Part {
        Wheat = 0,
        Head = 1,
        Stem = 2,
        Leaf = 3
    }

    // Maps each Part to a color material for annotating data
    public static Dictionary<Part, Material> partAnnotationMaterials = new Dictionary<Part, Material> {
        {Part.Head, new Material(Shader.Find("Standard")) { color = Color.red }},
        {Part.Stem, new Material(Shader.Find("Standard")) { color = Color.blue }},
        {Part.Leaf, new Material(Shader.Find("Standard")) { color = Color.green }}
    };

    // Track whether wheat objects are currently annotated (materials are simplified colors) or not
    public static bool wheatIsAnnotated = false;

    public static int wheatLayer = 6;
    public static int wheatLayerMask = 1 << 6;


    // Check if an obj is a wheat. If there is a second argument, also check if the obj is of the same part as the parameter.
    public static bool IsWheat(GameObject obj, Part part = Part.Wheat){
        WheatData wheatData = obj.GetComponent<WheatData>();
        if (wheatData){
            // If the user doesn't specify the part, but the object has a wheatData script, it is a wheat
            if (part == Part.Wheat){
                return true;
            }

            // If the user does specify the part, the wheatData.part must be the same, or return false
            if (part.Equals(wheatData.part)){
                return true;
            }
        } 
        return false;
    }


    // Returns all Wheat objects in the scene
    public static HashSet<GameObject> getAllWheats(){
        HashSet<GameObject> wheats = new HashSet<GameObject>();
        foreach (GameObject obj in FindObjectsOfType<GameObject>()){
            if (IsWheat(obj)){
                wheats.Add(obj);
            }
        }
        return wheats;
    }

    
    // Swap between Annotation On and Annotation Off modes
    public static void ToggleAnnotation(){
        if (!wheatIsAnnotated){
            ToggleAnnotationOn();
        } else {
            ToggleAnnotationOff();
        }
    }

    // Swap materials of each wheat with simple colors, for annotation purposes
    public static void ToggleAnnotationOn(){
        foreach (GameObject wheat in getAllWheats()){
            WheatData wheatData = wheat.GetComponent<WheatData>();
            wheatData.ToggleAnnotationOn(partAnnotationMaterials);
        }
        wheatIsAnnotated = true;
    }


    // Reset the materials of each wheat
    public static void ToggleAnnotationOff(){
        foreach (GameObject wheat in getAllWheats()){
            WheatData wheatData = wheat.GetComponent<WheatData>();
            wheatData.ToggleAnnotationOff();
        }
        wheatIsAnnotated = false;
    }

    // Get all wheat prefabs
    public static GameObject[] GetAllWheatPrefabs(){
        string wheatPrefabPath = "Prefabs/Wheat Models";
        return Resources.LoadAll<GameObject>(wheatPrefabPath);
    }
}
